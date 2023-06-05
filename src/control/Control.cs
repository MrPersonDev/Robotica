using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class Control : Node3D
{
    private const float BOX_SELECT_RAY_ORIGIN_DIST = 0.1f;
    private const float RAY_DIST = 100.0f;
    private const int TRANSFORM_LAYER = 2, MAIN_LAYER = 1;
    private const double DEFAULT_ROTATE_SNAP = 0.261799; // 15 degrees
    private const double QUARTER_ROTATE_SNAP = 1.570796; // 90 degrees

    private const int MAX_UNDO_STEPS = 10;
    private const float BOX_SELECT_MIN_DIST = 10.0f;

    private PartGroup selection = new PartGroup(), actualSelection = new PartGroup(), tempSelection = new PartGroup();
    private bool hadDefaultHoleBody;
    private HoleBody selectedHoleBody, prevSelectedHoleBody;
    private bool deleting = false, selectingNewPart = false;
    private PartOption currentPartOption;
    private PartObject currentPartObject;
    private Dictionary<String, Variant> currentPartParameters;
    private Part currentNewPart;
    private Node3D hovering;
    private bool creatingPartGroups = false;
    private List<Part> queuedToDisable = new List<Part>();
    private bool transforming = false, moving = false, rotating = false;
    private bool snapping = true;
    private Vector3 offset, axis;
    private Vector2 mousePos, prevMousePos, clickMousePos;
    private bool nothingOnClick = false, transformOnClick = false;
    private bool boxSelecting = false;
    private double originalAngle, prevAngle;
    private List<UndoStep> undoSteps = new List<UndoStep>(), redoSteps = new List<UndoStep>();
    private DisplayServer.WindowMode prevWindowMode = DisplayServer.WindowMode.Windowed;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath gridPath, pivotPath, transformPath, partsPath, uiPath;

    private Grid grid;
    private Pivot pivot;
    private Transform transform;
    private Parts parts;
    private Interface ui;

    public override void _Ready()
    {
        grid = (Grid)GetNode(gridPath);
        pivot = (Pivot)GetNode(pivotPath);
        transform = (Transform)GetNode(transformPath);
        parts = (Parts)GetNode(partsPath);
        ui = (Interface)GetNode(uiPath);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (!ui.IsSettingsOpen() && inputEvent is InputEventMouseMotion)
        {
            UpdateMousePosition();
            UpdateDragging();
        }

        bool opposingColliderInput = inputEvent.IsActionPressed("opposing_collider") || inputEvent.IsActionReleased("opposing_collider");
        bool shouldUpdateHovering = !(moving && rotating) && !ui.IsSettingsOpen() && (inputEvent is InputEventMouseMotion || opposingColliderInput);
        if (shouldUpdateHovering)
            UpdateHovering();

        if (inputEvent.IsActionPressed("select"))
            HandleSelectPressed();
        else if (inputEvent.IsActionReleased("select"))
            HandleSelectReleased();
        else if (inputEvent.IsActionPressed("transform_rotate"))
            transform.SetTransformMode(TransformMode.ROTATE);
        else if (inputEvent.IsActionPressed("transform_move"))
            transform.SetTransformMode(TransformMode.TRANSLATE);
        else if (inputEvent.IsActionPressed("transform_both"))
            transform.SetTransformMode(TransformMode.BOTH);
        else if (inputEvent.IsActionPressed("move"))
            HandleMoveInput();
        else if (inputEvent.IsActionPressed("move_rotate"))
            HandleMoveRotateInput();
        else if (inputEvent.IsActionPressed("focus"))
            HandleFocusInput();
        else if (inputEvent.IsActionPressed("cancel"))
            HandleCancelInput();
        else if (inputEvent.IsActionPressed("fullscreen"))
            HandleFullScreenInput();
        else if (inputEvent.IsActionPressed("hot_box"))
            HandleHotBoxPressed();
        else if (inputEvent.IsActionReleased("hot_box"))
            HandleHotBoxReleased();
        else if (inputEvent.IsActionPressed("orthographic"))
            HandleOrthographicPressed();
        else if (inputEvent.IsActionPressed("perspective"))
            HandlePerspectivePressed();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (deleting)
            return;

        if (moving)
        {
            if (rotating)
                MoveRotateInteraction();
            else
                MoveInteraction();
        }

        if (transforming)
            TransformInteraction();
    }

    private void UpdateDragging()
    {
        if (!Input.IsActionPressed("select") || moving || transforming)
            return;

        if (boxSelecting)
        {
            ui.UpdateSelectionBox(clickMousePos, mousePos);
            return;
        }

        float distance = clickMousePos.DistanceTo(mousePos);
        if (distance >= BOX_SELECT_MIN_DIST)
            boxSelecting = true;
    }

    private void UpdateHovering()
    {
        if (!rotating)
            snapping = false;

        Node3D prevHovering = hovering;
        hovering = null;

        bool hoveringTransform = MouseHoverRaycast(TRANSFORM_LAYER);
        bool hoveringMain = false;
        if (!hoveringTransform)
            hoveringMain = MouseHoverRaycast(MAIN_LAYER);

        if (prevHovering != null && hovering != prevHovering)
        {
            bool notHovering = !(hovering is Insert) && !(hovering is HoleBody) && prevHovering != hovering;
            bool shouldDisablePartColliders = notHovering;
            if (shouldDisablePartColliders)
            {
                Part partOfColliders = null;
                if (prevHovering is Part)
                    partOfColliders = (Part)prevHovering;
                else if (prevHovering is HoleBody && ((HoleBody)prevHovering).GetHole() != null)
                    partOfColliders = ((HoleBody)prevHovering).GetHole().GetPart();
                else if (prevHovering is Insert)
                    partOfColliders = ((Insert)prevHovering).GetPart();

                bool selectionExists = selectedHoleBody != null && selectedHoleBody.GetHole() != null;
                bool sameAsSelection = selectionExists && selectedHoleBody.GetHole().GetPart() == partOfColliders;
                if (partOfColliders != null && !sameAsSelection)
                {
                    if (creatingPartGroups)
                        queuedToDisable.Add(partOfColliders);
                    else
                        partOfColliders.DisableColliders(false);
                }
            }

            if (prevHovering is Selectable)
                ((Selectable)prevHovering).Unhover();
        }
    }

    private void HandleSelectPressed()
    {
        clickMousePos = mousePos;

        PressClickInteractions();
    }

    private void HandleSelectReleased()
    {
        creatingPartGroups = true;

        if (boxSelecting)
            BoxSelectInteraction();
        else
            ReleaseClickInteractions();

        boxSelecting = false;
        ui.HideSelectionBox();
    }

    private void BoxSelectInteraction()
    {
        Deselect();

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        Vector2 boxTopLeft = ui.GetSelectionBoxTopLeft(clickMousePos, mousePos);
        Vector2 boxSize = ui.GetSelectionBoxSize(clickMousePos, mousePos);

        PhysicsShapeQueryParameters3D param = new PhysicsShapeQueryParameters3D();

        ConvexPolygonShape3D shape = new ConvexPolygonShape3D();
        Vector3[] points = new Vector3[8];
        int i = 0;
        points[i++] = pivot.GetWorldPos(boxTopLeft, BOX_SELECT_RAY_ORIGIN_DIST);
        points[i++] = pivot.GetWorldPos(boxTopLeft, RAY_DIST);
        points[i++] = pivot.GetWorldPos(new Vector2(boxTopLeft.X + boxSize.X, boxTopLeft.Y), BOX_SELECT_RAY_ORIGIN_DIST);
        points[i++] = pivot.GetWorldPos(new Vector2(boxTopLeft.X + boxSize.X, boxTopLeft.Y), RAY_DIST);
        points[i++] = pivot.GetWorldPos(boxTopLeft + boxSize, BOX_SELECT_RAY_ORIGIN_DIST);
        points[i++] = pivot.GetWorldPos(boxTopLeft + boxSize, RAY_DIST);
        points[i++] = pivot.GetWorldPos(new Vector2(boxTopLeft.X, boxTopLeft.Y + boxSize.Y), BOX_SELECT_RAY_ORIGIN_DIST);
        points[i++] = pivot.GetWorldPos(new Vector2(boxTopLeft.X, boxTopLeft.Y + boxSize.Y), RAY_DIST);
        shape.Points = points;

        param.Shape = shape;
        Godot.Collections.Array<Godot.Collections.Dictionary> results = spaceState.IntersectShape(param);

        foreach (Godot.Collections.Dictionary result in results)
        {
            Node3D node = (Node3D)result["collider"];
            if (!(node.GetParent() is Part))
                continue;

            Part part = (Part)node.GetParent();
            PartInteraction(part, false);
        }
    }

    private void PressClickInteractions()
    {
        transformOnClick = MouseClickRaycast(TRANSFORM_LAYER);
        nothingOnClick = !transformOnClick && !MouseClickRaycast(MAIN_LAYER, false);
    }

    private async void ReleaseClickInteractions()
    {
        if (selectingNewPart)
            PlaceNewPart();
        else if (moving)
        {
            EndMoving();
            Deselect();
        }
        else
        {
            if (nothingOnClick)
                Deselect();
            else
            {
                bool clickedTransform = MouseClickRaycast(TRANSFORM_LAYER, false);
                if (!clickedTransform && !transformOnClick)
                    MouseClickRaycast(MAIN_LAYER);
            }
        }

        if (transforming)
        {
            EndTransforming();
            await CreatePartGroups(selection.GetParts());
        }
    }

    private async void PlaceNewPart()
    {
        if (selection.HasParts() && selectingNewPart)
        {
            transform.Enable();

            List<Part> curParts = new List<Part>(selection.GetParts());
            undoSteps.Add(new UndoStep(Callable.From(() => { RestorePartsDeleted(true, curParts, true); }), false, curParts));
        }

        bool prevSelectingNewPart = selectingNewPart;
        selectingNewPart = false;

        currentNewPart = null;

        if (prevSelectingNewPart)
            await CreateAndMoveNewPart(currentPartOption, currentPartParameters);
        else
        {
            currentPartOption = null;
            currentPartParameters = null;
        }
    }

    private void HandleMoveInput()
    {
        if (ui.IsSettingsOpen())
            return;

        if (moving)
            EndMoving();
        else if (selection.HasParts())
            StartMoving();
    }

    private void HandleMoveRotateInput()
    {
        if (ui.IsSettingsOpen())
            return;

        if (moving && rotating)
            EndMoving();
        else if (moving && selectedHoleBody != null)
            StartMoveRotating();
    }

    private async void HandleCancelInput()
    {
        if (ui.IsSettingsOpen())
            return;

        if (selectingNewPart)
            await Delete(false);
        selectingNewPart = false;
    }

    private void HandleFullScreenInput()
    {
        if (DisplayServer.WindowGetMode() != DisplayServer.WindowMode.Fullscreen)
        {
            prevWindowMode = DisplayServer.WindowGetMode();
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
            DisplayServer.WindowSetMode(prevWindowMode);
    }

    private void HandleHotBoxPressed()
    {
        if (ui.IsSettingsOpen())
            return;

        ui.StartHotBoxing(mousePos);
    }

    private void HandleHotBoxReleased()
    {
        if (ui.IsSettingsOpen())
            return;

        ui.EndHotBoxing();
    }

    private void HandleOrthographicPressed()
    {
        if (ui.IsSettingsOpen())
            return;

        grid.HideGrid();
        pivot.SetOrthogonal(true);
    }

    private void HandlePerspectivePressed()
    {
        if (ui.IsSettingsOpen())
            return;

        grid.ShowGrid();
        pivot.SetOrthogonal(false);
    }

    public void HandleNewInput()
    {
        if (ui.IsSettingsOpen())
            return;

        Deselect(false);
        ResetNewPart();
        parts.Clear();
        SaveManager.SetCurrentPath(null, ui);
    }

    public void HandleSaveInput()
    {
        if (ui.IsSettingsOpen())
            return;

        ClearHistory();

        if (SaveManager.GetCurrentPath() == null)
            ui.ShowSaveDialog();
        else
            SaveManager.Save(SaveManager.GetCurrentPath(), ui, parts);
    }

    public void HandleSaveAsInput()
    {
        if (ui.IsSettingsOpen())
            return;

        ClearHistory();

        ui.ShowSaveDialog();
    }

    public void HandleLoadInput()
    {
        if (ui.IsSettingsOpen())
            return;

        ClearHistory();

        Deselect(false);
        ResetNewPart();
        ui.ShowLoadDialog();
    }

    public void HandleImportInput()
    {
        if (ui.IsSettingsOpen())
            return;

        ClearHistory();

        Deselect(false);
        ResetNewPart();
        ui.ShowImportDialog();
    }

    public void HandleExitInput()
    {
        GetTree().Quit();
    }

    public void HandleUndoInput()
    {
        if (ui.IsSettingsOpen())
            return;

        if (undoSteps.Count == 0 || (moving && !selectingNewPart) || transforming)
            return;

        Callable callable = undoSteps[undoSteps.Count - 1].GetCallable();
        undoSteps.RemoveAt(undoSteps.Count - 1);

        callable.Call();
    }

    public void HandleRedoInput()
    {
        if (ui.IsSettingsOpen())
            return;

        if (redoSteps.Count == 0 || (moving && !selectingNewPart) || transforming)
            return;

        Callable callable = redoSteps[redoSteps.Count - 1].GetCallable();
        redoSteps.RemoveAt(redoSteps.Count - 1);

        callable.Call();
    }

    public async void HandleDeleteInput()
    {
        if (ui.IsSettingsOpen())
            return;

        await Delete();

        await CreatePartGroups(selection.GetParts());
    }

    public void HandleSettingsInput()
    {
        if (ui.IsSettingsOpen())
            return;

        ui.ShowSettings();
    }
    
    public void HandleRequiredPartsInput()
    {
        if (ui.IsSettingsOpen())
            return;

        ui.ShowRequiredParts();
    }

    public void HandleFocusInput()
    {
        if (ui.IsSettingsOpen())
            return;

        if (selectedHoleBody != null)
            pivot.GlobalPosition = selectedHoleBody.GetPos();
        else if (selection.HasParts())
            pivot.GlobalPosition = selection.GetCenter();
    }

    public void HandleSelectAllInput()
    {
        if (ui.IsSettingsOpen())
            return;

        foreach (Part part in parts.GetAllParts())
            PartInteraction(part, false);
    }

    public void HandleDeselectAllInput()
    {
        if (ui.IsSettingsOpen())
            return;

        Deselect(false);
    }

    public void HandleGroupInput()
    {
        if (ui.IsSettingsOpen())
            return;

        List<Part> selectedParts = actualSelection.GetParts();
        parts.CreateManualPartGroup(selectedParts, true);
    }

    public void HandleDuplicateInput()
    {
        if (ui.IsSettingsOpen())
            return;

        foreach (Part part in selection.GetParts())
            parts.DuplicatePart(part);
    }

    private void LimitHistory(int steps)
    {
        while (undoSteps.Count > steps)
        {
            UndoStep undoStep = undoSteps[0];

            if (undoStep.IsDeleting())
            {
                foreach (Part part in undoStep.GetParts())
                    parts.DeletePart(part);
            }

            undoSteps.RemoveAt(0);
        }
    }

    private void CleanHistory()
    {
        LimitHistory(MAX_UNDO_STEPS);
        redoSteps.Clear();
    }

    private void ClearHistory()
    {
        LimitHistory(0);
        redoSteps.Clear();
    }

    private void StoreCurrent(UndoStep undoStep, bool undo)
    {
        if (undo)
            redoSteps.Add(undoStep);
        else
            undoSteps.Add(undoStep);
    }

    private void RestoreTransformMove(Vector3 pos, List<Part> parts, bool undo)
    {
        TempSelectParts(parts);
        Vector3 curPos = tempSelection.GetCenter();
        StoreCurrent(new UndoStep(Callable.From(() => { RestoreTransformMove(curPos, parts, !undo); })), undo);

        tempSelection.MoveTo(pos);
        transform.GlobalPosition = pos;
        ClearTempSelection();
    }

    private void RestoreTransformRotate(Vector3 axis, double angle, List<Part> parts, bool undo)
    {
        StoreCurrent(new UndoStep(Callable.From(() => { RestoreTransformRotate(axis, -angle, parts, !undo); })), undo);

        TempSelectParts(parts);
        tempSelection.RotateCenter(axis, (float)angle);
        ClearTempSelection();
    }

    private void RestoreMoveRotate(Vector3 axis, double angle, Vector3 pos, List<Part> parts, bool undo)
    {
        StoreCurrent(new UndoStep(Callable.From(() => { RestoreMoveRotate(axis, -angle, pos, parts, !undo); })), undo);

        TempSelectParts(parts);
        tempSelection.RotatePos(axis, (float)angle, pos);
        transform.GlobalPosition = tempSelection.GetCenter();
        ClearTempSelection();
    }

    private void RestorePartsDeleted(bool delete, List<Part> parts, bool undo)
    {
        StoreCurrent(new UndoStep(Callable.From(() => { RestorePartsDeleted(!delete, parts, !undo); }), delete, parts), undo);

        foreach (Part part in parts)
        {
            if (delete)
                PretendDeletePart(part);
            else
                UndeletePart(part);
        }
    }

    private void UpdateMousePosition()
    {
        prevMousePos = mousePos;
        mousePos = GetViewport().GetMousePosition();
    }

    public void UpdateSize()
    {
        transform.UpdateSize(pivot);
    }

    private Godot.Collections.Dictionary MouseRaycast(int collisionMask)
    {
        Vector3 rayStart = pivot.GetRayOrigin(mousePos);
        Vector3 rayNormal = pivot.GetRayNormal(mousePos);
        Vector3 rayEnd = rayStart + rayNormal * RAY_DIST;

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        PhysicsRayQueryParameters3D rayParameters = new PhysicsRayQueryParameters3D();
        rayParameters.From = rayStart;
        rayParameters.To = rayEnd;
        rayParameters.CollisionMask = (uint)collisionMask;
        rayParameters.CollideWithAreas = true;

        Godot.Collections.Dictionary result = spaceState.IntersectRay(rayParameters);
        return result;
    }

    private bool MouseHoverRaycast(int collisionMask)
    {
        Godot.Collections.Dictionary result = MouseRaycast(collisionMask);

        if (result.Count == 0)
            return false;

        Node3D other = (Node3D)result["collider"];

        bool isPart = other.IsInGroup("PART_COLLIDER");
        bool isHandle = other.IsInGroup("HANDLE_COLLIDER");
        bool isHole = other.IsInGroup("HOLE_COLLIDER");
        bool isInsert = other.IsInGroup("INSERT_COLLIDER");
        bool isSelectable = isHandle || isHole || isPart;
        if (isSelectable)
        {
            Selectable selectable;
            if (isHole)
            {
                HoleBody holeBody = GetBodyOrOpposingBody((HoleBody)other.GetParent());
                selectable = (Selectable)holeBody;

                if (holeBody.IsMotorHoleBody())
                {
                    selectable = holeBody.GetHole().GetPart();
                    isPart = true;
                    isHole = false;
                }
                else
                    SnapToHole(holeBody);
            }
            else
                selectable = (Selectable)other.GetParent();

            if (selectable == null)
                return true;

            bool movingSelectable = isPart && moving && selectable != null && selection.GetParts().Contains((Part)selectable);
            if (isPart && !movingSelectable)
            {
                Part part = (Part)selectable;

                if (!boxSelecting && !transforming)
                    part.EnableColliders();

                if (part.IsMotor())
                    SnapToMotor(part);
            }

            selectable.Hover();
            hovering = selectable;
        }
        else if (isInsert)
        {
            Insert insert = (Insert)other;
            Vector3 position = (Vector3)result["position"];
            SnapToInsert(insert, position);

            hovering = other;
        }

        return true;
    }

    private bool MouseClickRaycast(int collisionMask, bool interact = true)
    {
        Godot.Collections.Dictionary result = MouseRaycast(collisionMask);

        if (!interact)
            return result.Count != 0;

        if (result.Count == 0)
            return false;

        Node3D other = (Node3D)result["collider"];

        bool isPart = other.IsInGroup("PART_COLLIDER");
        bool isInsert = other.IsInGroup("INSERT_COLLIDER");
        bool isHandle = other.IsInGroup("HANDLE_COLLIDER");
        bool isHole = other.IsInGroup("HOLE_COLLIDER");

        if (isPart)
            PartInteraction((Node3D)other.GetParent());
        else if (isInsert)
        {
            Insert insert = (Insert)other;
            Part part = insert.GetPart();
            PartInteraction(part);
        }
        else if (isHandle)
            StartTransforming(other);
        else if (isHole)
        {
            HoleBody holeBody = GetBodyOrOpposingBody((HoleBody)other.GetParent());
            SelectHole(holeBody);
        }
        else
            Deselect();

        return true;
    }

    private Vector3? IntersectMouse(Moveable other)
    {
        Vector3 start = pivot.GetRayOrigin(mousePos);
        Vector3 normal = pivot.GetRayNormal(mousePos);

        Plane intersectionPlane = GetIntersectionPlane(other);
        Vector3? pos = intersectionPlane.IntersectsRay(start, normal);

        return pos;
    }

    private Plane GetIntersectionPlane(Moveable other)
    {
        Vector3 planeNormal = GetIntersectionPlaneNormal();
        Vector3 planePos = GetIntersectionPlanePos(other);

        return new Plane(planeNormal, planePos);
    }

    private Vector3 GetIntersectionPlaneNormal()
    {
        if (moving && !rotating)
            return axis;
        else
            return pivot.GetCamNormal() * (Vector3.One - axis);
    }

    private Vector3 GetIntersectionPlanePos(Moveable other)
    {
        if (moving)
            return pivot.GlobalPosition;
        else
            return transform.GlobalPosition - offset;
    }

    private double RotationToIntersection(Vector3 intersection, bool snapping)
    {
        Vector3 rotationPivot = moving && rotating ? selectedHoleBody.GetPos() : transform.GlobalPosition;
        Vector3 relative = intersection - rotationPivot;

        Vector3 perpendicularVector = Vector3.Forward;
        if (axis != Vector3.Up)
            perpendicularVector = Vector3.Up.Cross(axis).Normalized();

        double angleToIntersection = perpendicularVector.SignedAngleTo(relative.Normalized(), axis);

        if (snapping)
            angleToIntersection = Math.Round(angleToIntersection / DEFAULT_ROTATE_SNAP) * DEFAULT_ROTATE_SNAP;

        return angleToIntersection;
    }

    private double RotationToMouse(Vector3 point, bool relativeToAngle)
    {
        Vector2 pointOnScreen = pivot.GetScreenPos(point);
        Vector2 relative = mousePos - pointOnScreen;

        double angleToPoint = Math.Atan2(relative.Y, relative.X);

        if (relativeToAngle)
        {
            Vector3 camNormal = pivot.GetCamNormal();
            double dot = camNormal.Dot(axis);

            if (dot >= 0)
                angleToPoint *= -1;
        }

        return angleToPoint;
    }

    private double TransformRotation()
    {
        return RotationToMouse(transform.GlobalPosition, true);
    }

    private double MoveRotation()
    {
        return RotationToMouse(selectedHoleBody.GetPos(), true);
    }

    private double ChangeInAngle(double angle, double snapping)
    {
        double relativeAngle = angle - originalAngle;

        if (snapping != 0)
            relativeAngle = Math.Round(relativeAngle / snapping) * snapping;

        angle = relativeAngle + originalAngle;
        double changeInAngle = angle - prevAngle;

        prevAngle = angle;
        return changeInAngle;
    }

    private void PartInteraction(Node3D other, bool deselect = true)
    {
        if (deselect)
            Deselect(true);

        Part part = (Part)other;

        ActuallySelect(part);
        Select(part);

        if (!selectingNewPart)
        {
            transform.Enable();
            transform.GlobalPosition = selection.GetCenter();
        }
    }

    private void TransformInteraction()
    {
        if (rotating)
        {
            TransformRotateInteraction();
            return;
        }

        Vector3? pos = IntersectMouse(selection);
        if (pos == null || snapping)
            return;

        Vector3 intersection = (Vector3)pos;
        TransformMoveInteraction(intersection);
    }

    private void TransformRotateInteraction()
    {
        double angle = ChangeInAngle(TransformRotation(), Input.IsActionPressed("snapping") ? DEFAULT_ROTATE_SNAP : 0.0);

        Vector3 rotationAxis = axis.Normalized();
        selection.RotateCenter(rotationAxis, (float)angle, true);
    }

    private void TransformMoveInteraction(Vector3 intersection)
    {
        transform.GlobalPosition = transform.GlobalPosition * (Vector3.One - axis) + (intersection + offset) * axis;
        selection.MoveTo(transform.GlobalPosition, true);
    }

    private void MoveInteraction()
    {
        axis = pivot.GetCamNormal();

        Vector3? pos = IntersectMouse(selection);
        if (pos == null || snapping)
            return;

        Vector3 intersection = (Vector3)pos;

        transform.GlobalPosition = intersection + offset;
        selection.MoveTo(transform.GlobalPosition, true);

        if (selection.GetParts().Count != 1)
            return;
        Part lonePart = selection.GetParts()[0];
        if (selectedHoleBody == null && !hadDefaultHoleBody && lonePart.HasDefaultHole())
        {
            selectedHoleBody = lonePart.GetDefaultHoleBody();
            StartMoving(false);
        }
    }

    private void MoveRotateInteraction()
    {
        if (!IsInstanceValid(selectedHoleBody))
            return;

        double angle = ChangeInAngle(MoveRotation(), Input.IsActionPressed("snapping") ? QUARTER_ROTATE_SNAP : DEFAULT_ROTATE_SNAP);

        Vector3 rotationAxis = axis.Normalized();
        selection.RotatePos(rotationAxis, (float)angle, selectedHoleBody.GetPos(), false);
    }

    private HoleBody GetBodyOrOpposingBody(HoleBody holeBody)
    {
        if (Input.IsActionPressed("opposing_collider"))
            return holeBody.GetHole().GetOpposingBody(holeBody);
        else
            return holeBody;
    }

    private void RotateToDirection(Vector3 direction)
    {
        if (selectedHoleBody == null || !IsInstanceValid(selectedHoleBody) || !selectedHoleBody.IsInsideTree())
            return;

        selection.UpdateCollisionTransform();

        Vector3 selectedHoleDirection = selectedHoleBody.GlobalTransform.Basis.Z;

        Vector3 rotationAxis = selectedHoleDirection.Cross(direction).Normalized();
        float angleToDirection = selectedHoleDirection.AngleTo(direction);

        if (rotationAxis == Vector3.Zero)
            rotationAxis = Vector3.Up;

        selection.RotateCenter(rotationAxis, angleToDirection, true);

        StartMoving(false);
    }

    private void SnapToHole(HoleBody holeBody)
    {
        if (!moving || rotating || selectedHoleBody == null || !IsInstanceValid(holeBody) || !IsInstanceValid(selectedHoleBody) || !selectedHoleBody.IsInsideTree())
            return;

        Part part = holeBody.GetHole().GetPart();
        bool isPartSelected = selection.GetParts().Contains(part);
        if (isPartSelected)
            return;

        foreach (Part selectionPart in selection.GetParts())
            if (selectionPart.HasInsert())
                parts.RequireUpdate(selectionPart);

        snapping = true;

        selection.UpdateCollisionTransform();

        Vector3 holeDirection = holeBody.GlobalTransform.Basis.Z;
        RotateToDirection(-holeDirection);

        float rotation = selectedHoleBody.GlobalTransform.Basis.X.SignedAngleTo(holeBody.GlobalTransform.Basis.X, holeDirection);
        selection.RotateCenter(holeDirection, rotation, true);

        Vector3 pos = holeBody.GetPos() + offset;
        selection.MoveTo(pos, true);
    }

    private void SnapToMotor(Part motor)
    {
        bool movingAnInsert = moving && selection.GetParts().Count == 1 && selection.GetParts()[0].HasInsert();
        if (!movingAnInsert)
            return;

        parts.RequireUpdate(selection.GetParts()[0]);

        selection.UpdateCollisionTransform();

        Vector3 motorDirection = motor.GlobalTransform.Basis.Z;
        RotateToDirection(motorDirection);

        snapping = true;
        Part insert = selection.GetParts()[0];
        insert.MoveToMotor(motor, true);
    }

    private void SnapToInsert(Insert insert, Vector3 position)
    {
        if (!moving || rotating || selectedHoleBody == null)
            return;

        parts.RequireUpdate(insert.GetPart());

        Part part = insert.GetPart();
        bool isPartSelected = selection.GetParts().Contains(part);
        if (isPartSelected)
            return;

        snapping = true;

        selection.UpdateCollisionTransform();

        Vector3 centerPos = insert.GetPoint(selectedHoleBody, position);
        HoleBody snappingHoleBody = null;
        if (!Input.IsActionPressed("snapping"))
            snappingHoleBody = insert.GetSnappingHoleBody(centerPos);

        Vector3 direction;
        if (snappingHoleBody != null)
        {
            direction = -snappingHoleBody.GlobalTransform.Basis.Z;
            centerPos = snappingHoleBody.GetPos();
        }
        else
            direction = -insert.GlobalTransform.Basis.Z;
        RotateToDirection(direction);

        Vector3 pos = centerPos + offset;
        selection.MoveTo(pos, true);
    }

    private void StartTransforming(Node3D other)
    {
        Handle handle = (Handle)other.GetParent();
        axis = handle.GetAxis();

        rotating = handle.GetRotating();
        transforming = true;

        if (!rotating)
        {
            Vector3 curPos = selection.GetCenter();
            List<Part> selectionParts = new List<Part>(selection.GetParts());
            undoSteps.Add(new UndoStep(Callable.From(() => { RestoreTransformMove(curPos, selectionParts, true); })));
            CleanHistory();
        }

        selection.DisableMeshCollider();
        selection.DisableColliders(true);

        Vector3? intersectionNullable = IntersectMouse(selection);
        if (intersectionNullable == null)
            return;
        Vector3 intersection = (Vector3)intersectionNullable;

        offset = transform.GlobalPosition - intersection;
        prevAngle = TransformRotation();
        originalAngle = prevAngle;
    }

    private void EndTransforming(bool undoable = true, bool setRotating = true)
    {
        if (undoable && selection.GetParts().Count > 0 && rotating)
        {
            Vector3 curAxis = axis;
            double totalAngleChange = prevAngle - originalAngle;
            List<Part> selectionParts = new List<Part>(selection.GetParts());
            undoSteps.Add(new UndoStep(Callable.From(() => { RestoreTransformRotate(curAxis, -totalAngleChange, selectionParts, true); })));
            CleanHistory();
        }

        selection.EnableMeshCollider();
        selection.UpdateCollisionTransform();

        transforming = false;
        if (setRotating)
            rotating = false;
    }

    private void StartMoving(bool undoable = true)
    {
        if (!selectingNewPart && undoable)
        {
            Vector3 curPos = selection.GetCenter();
            List<Part> selectionParts = new List<Part>(selection.GetParts());
            undoSteps.Add(new UndoStep(Callable.From(() => { RestoreTransformMove(curPos, selectionParts, true); })));
            CleanHistory();
        }

        if (selectedHoleBody != null)
            offset = selection.GetCenter() - selectedHoleBody.GetPos();
        else
            offset = Vector3.Zero;

        EndTransforming();
        selection.DisableMeshCollider();
        selection.DisableColliders(true);
        transform.Disable();

        moving = true;

        if (!snapping && hovering is HoleBody)
            SnapToHole((HoleBody)hovering);
    }

    private async void EndMoving(bool setRotating = true)
    {
        if (!moving)
            return;
        
        selection.EnableMeshCollider();

        transform.GlobalPosition = selection.GetCenter();

        moving = false;

        selection.UpdateCollisionTransform();

        if (rotating)
            EndMoveRotating();

        if (setRotating)
            rotating = false;
        await CreatePartGroups(selection.GetParts());
    }

    private void StartMoveRotating()
    {
        selection.UpdateCollisionTransform();

        axis = selectedHoleBody.GlobalTransform.Basis.Z;

        moving = true;
        rotating = true;

        prevAngle = MoveRotation();
        originalAngle = prevAngle;
    }

    private void EndMoveRotating()
    {
        if (!IsInstanceValid(prevSelectedHoleBody))
            return;

        Vector3 curAxis = axis;
        double totalAngleChange = prevAngle - originalAngle;
        Vector3 pos = prevSelectedHoleBody.GetPos();
        List<Part> selectionParts = new List<Part>(selection.GetParts());
        undoSteps.Add(new UndoStep(Callable.From(() => { RestoreMoveRotate(curAxis, -totalAngleChange, pos, selectionParts, true); })));
        CleanHistory();
    }

    public async Task CreatePartGroups(List<Part> partsList)
    {
        foreach (Part part in parts.GetAllParts())
        {
            if (part.RequiresUpdate())
            {
                partsList.Add(part);
                partsList.AddRange(part.GetPotentiallyCollidingParts());
            }
        }

        List<Part> partsCollidingWithInsert = new List<Part>();
        foreach (Part part in partsList)
            part.IncreaseInsertWidth();
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        foreach (Part part in partsList)
            partsCollidingWithInsert.AddRange(part.GetCurCollidingParts());
        foreach (Part part in partsList)
            part.ResetInsertWidth();

        List<Part> previouslyDisabled = parts.EnableColliders(partsCollidingWithInsert);
        parts.OverlappingFix();

        // Awaiting a couple physics frames seems to be required for proper collision
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        parts.CreatePartGroups();

        parts.DisableColliders(previouslyDisabled, false);
        parts.DisableColliders(queuedToDisable, false);
        queuedToDisable.Clear();

        creatingPartGroups = false;
    }

    private void Select(Moveable moveable)
    {
        bool hasParentGroup = moveable is Part && ((Part)moveable).GetPartGroup() != null;
        if (hasParentGroup)
        {
            Select(((Part)moveable).GetPartGroup());
            return;
        }

        moveable.Select();

        if (moveable is Part)
            Parts.AdoptChild(selection, (Part)moveable, false);
        else
            Parts.AdoptChildren(selection, moveable.GetParts(), false);

        if (moveable.HasDefaultHole())
            SelectHole(moveable.GetDefaultHoleBody());
        hadDefaultHoleBody = moveable.HasDefaultHole();
    }

    private void ActuallySelect(Part part)
    {
        actualSelection.AddPart(part, false);
        part.ActuallySelect();
    }

    private void Deselect(bool canMultiSelect = true)
    {
        DeselectHole();

        if (Input.IsActionPressed("multiselect") && canMultiSelect)
            return;

        parts.DeselectAll();

        EndTransforming(false, false);
        EndMoving(false);
        rotating = false;

        Parts.AbductChildren(selection, false);
        Parts.AbductChildren(actualSelection, false);

        transform.Disable();
    }

    private void ResetNewPart()
    {
        currentNewPart = null;
        currentPartOption = null;
        currentPartParameters = null;
    }

    private void SelectHole(HoleBody holeBody)
    {
        if (!IsInstanceValid(holeBody) || !IsInstanceValid(holeBody.GetHole()) || !IsInstanceValid(holeBody.GetHole().GetPart()))
            return;

        Part part = holeBody.GetHole().GetPart();
        bool isPartSelected = selection.GetParts().Contains(part);
        if (!isPartSelected)
            PartInteraction(part);

        if (selectedHoleBody != null)
            selectedHoleBody.Deselect();
        selectedHoleBody = holeBody;

        holeBody.Select();
    }

    private void DeselectHole()
    {
        if (selectedHoleBody != null)
            selectedHoleBody.Deselect();
        prevSelectedHoleBody = selectedHoleBody;
        selectedHoleBody = null;

        snapping = false;
    }

    private async Task Delete(bool undoable = true)
    {
        if (!actualSelection.HasParts())
            return;

        deleting = true;
        selection.PrepareToDelete();

        // Allow parts to prepare themselves for deletion
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        List<Part> partsList = actualSelection.GetParts(), deletedParts = new List<Part>(partsList);
        while (partsList.Count > 0)
        {
            DeletePart(partsList[partsList.Count - 1], undoable);
            partsList = actualSelection.GetParts();
        }

        if (undoable)
            undoSteps.Add(new UndoStep(Callable.From(() => { RestorePartsDeleted(false, deletedParts, true); }), true, deletedParts));

        // Selection contains freed parts until next physics frame
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        parts.RequireUpdate(selection);
        Parts.AbductChildren(selection, true);

        Deselect();
        ResetNewPart();

        deleting = false;
    }

    private void DeletePart(Part part, bool undoable)
    {
        selection.RemovePart(part, false);
        actualSelection.RemovePart(part, false);

        if (part.GetPartGroup() != null)
            Parts.AbductChild((PartGroup)part.GetPartGroup(), part, true);

        if (undoable)
            PretendDeletePart(part);
        else
            parts.DeletePart(part);
    }

    private void PretendDeletePart(Part part)
    {
        part.PretendDelete();

        if (selection.GetParts().Contains(part))
            Deselect(false);
    }

    private void UndeletePart(Part part)
    {
        part.Undelete();
    }

    public async Task CreateAndMoveNewPart(PartOption partOption, Dictionary<String, Variant> parameters)
    {
        if (partOption == null || parameters == null)
            return;

        Deselect(false);

        PartObject partObject = partOption.GetPartObject(parameters);

        Part newPart = currentNewPart;
        if (currentNewPart == null || partOption != currentPartOption || partObject != currentPartObject)
        {
            if (currentNewPart != null && (partOption != currentPartOption || partObject != currentPartObject))
                DeletePart(currentNewPart, false);

            List<Hole> holeList = partOption.GetHoles(partObject);

            newPart = parts.CreatePart(partObject, holeList, false);
            newPart.Hide();
            currentNewPart = newPart;
        }

        partOption.Setup(newPart, parameters);

        currentPartOption = partOption;
        currentPartObject = partObject;
        currentPartParameters = parameters;

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        newPart.Initialize();
        selectingNewPart = true;
        newPart.Show();

        EndMoving();
        Deselect();
        PartInteraction(newPart);
        StartMoving();
    }

    private void TempSelectParts(List<Part> parts)
    {
        Parts.AdoptChildren(tempSelection, parts, false);
    }

    private void ClearTempSelection()
    {
        Parts.AbductChildren(tempSelection, false);
    }
}
