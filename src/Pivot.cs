using Godot;
using System;

public partial class Pivot : Node3D
{
    private const float CAM_MAX_ANGLE = 90.0f;
    private const float CAM_MIN_ANGLE = 90.0f;
    private const float ORTHOGONAL_DIST_MULTIPLIER = 2.0f;

    private float zoomSpeed = 0.2f;
    private float xOrbitSpeed = 0.01f;
    private float yOrbitSpeed = 0.005f;
    private float panSpeed = 0.01f;
    private bool invertXOrbit = false;
    private bool invertYOrbit = false;
    private bool invertXPan = false;
    private bool invertYPan = false;

    private bool orbiting = false;
    private bool panning = false;
    private bool canEnableGrid = true;

    [Export]
    private bool orthogonal = false;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath camPosPath, mainCamPath, outlineCamPath, outlineContainerPath, orthographicGridPath;

    private Node3D camPos;
    private Camera3D backgroundCam, mainCam, outlineCam;
    private SubViewportContainer outlineContainer;
    private MeshInstance3D orthographicGrid;

    public override void _Ready()
    {
        camPos = (Node3D)GetNode(camPosPath);
        mainCam = (Camera3D)GetNode(mainCamPath);
        outlineCam = (Camera3D)GetNode(outlineCamPath);
        outlineContainer = (SubViewportContainer)GetNode(outlineContainerPath);
        orthographicGrid = (MeshInstance3D)GetNode(orthographicGridPath);
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        bool panningAltInputPressed = inputEvent.IsActionPressed("pan_alt_main") && inputEvent.IsActionPressed("pan_alt_modifier");
        bool panningInputPressed = inputEvent.IsActionPressed("pan") || panningAltInputPressed;

        bool panningAltInputReleased = inputEvent.IsActionReleased("pan_alt_main") || inputEvent.IsActionReleased("pan_alt_modifier");
        bool panningInputReleased = inputEvent.IsActionReleased("pan") || panningAltInputReleased;

        if (!orbiting && panningInputPressed)
            panning = true;
        else if (panningInputReleased)
            panning = false;

        if (!panning && inputEvent.IsActionPressed("orbit"))
            orbiting = true;
        else if (inputEvent.IsActionReleased("orbit"))
            orbiting = false;

        if (Input.IsActionJustPressed("zoom_in"))
            Scale *= 1.0f / (1.0f + zoomSpeed);
        else if (Input.IsActionJustPressed("zoom_out"))
            Scale *= 1.0f + zoomSpeed;

        if (inputEvent is InputEventMouseMotion)
        {
            InputEventMouseMotion mouseEvent = (InputEventMouseMotion)inputEvent;
            if (orbiting)
                OrbitingUpdate(mouseEvent);
            else if (panning)
                PanningUpdate(mouseEvent);
        }

        if (Input.IsActionJustPressed("snap_camera"))// && orthogonal)
            SnapCamera();
    }

    public override void _Process(double delta)
    {
        UpdateCameraPositions();
        UpdateCameraZoom();
    }

    private void OrbitingUpdate(InputEventMouseMotion mouseEvent)
    {
        Rotate(Vector3.Up, -mouseEvent.Relative.X * xOrbitSpeed * (invertXOrbit ? -1 : 1));
        Rotate(Transform.Basis.X.Normalized(), -mouseEvent.Relative.Y * yOrbitSpeed * (invertYOrbit ? -1 : 1));

        if (RotationDegrees.X < -CAM_MIN_ANGLE || RotationDegrees.X > CAM_MAX_ANGLE)
            ClampCameraAngle();
    }

    private void ClampCameraAngle()
    {
        Vector3 rotation = RotationDegrees;
        rotation.X = (float)Math.Clamp(RotationDegrees.X, -CAM_MIN_ANGLE, CAM_MAX_ANGLE);
        RotationDegrees = rotation;
    }

    private void PanningUpdate(InputEventMouseMotion mouseEvent)
    {
        float curPanSpeed = orthogonal ? panSpeed / ORTHOGONAL_DIST_MULTIPLIER : panSpeed;
        Translate(Vector3.Up * mouseEvent.Relative.Y * curPanSpeed * (invertYPan ? -1 : 1));
        Translate(Vector3.Left * mouseEvent.Relative.X * curPanSpeed * (invertXPan ? -1 : 1));
    }

    private void UpdateCameraPositions()
    {
        mainCam.GlobalTransform = camPos.GlobalTransform;
        outlineCam.GlobalTransform = camPos.GlobalTransform;
    }

    private void UpdateCameraZoom()
    {
        if (orthogonal)
            SetCameraSize(camPos.GlobalPosition.DistanceTo(GlobalPosition) / ORTHOGONAL_DIST_MULTIPLIER);
        else
            SetCameraSize(1.0f);
    }

    private void SetCameraSize(float size)
    {
        mainCam.Size = size;
        outlineCam.Size = size;
    }

    public void SetOrthogonal(bool value)
    {
        if (value != orthogonal)
        {
            if (value)
                Scale *= ORTHOGONAL_DIST_MULTIPLIER;
            else
                Scale /= ORTHOGONAL_DIST_MULTIPLIER;
        }

        orthogonal = value;
        if (value)
            ShowGrid();
        else
            HideGrid();

        if (value)
            SetCameraProjection(Camera3D.ProjectionType.Orthogonal);
        else
            SetCameraProjection(Camera3D.ProjectionType.Perspective);
    }

    private void SnapCamera()
    {
        double minDistance = double.MaxValue;
        Vector3 closestSnapDirection = GlobalTransform.Basis.Z;
        for (int position = 0; position < 3; position++)
        {
            for (int value = -1; value <= 1; value += 2)
            {
                Vector3 snapDirection = new Vector3(0.0f, 0.0f, 0.0f);
                snapDirection[position] = value;

                double distance = snapDirection.AngleTo(GlobalTransform.Basis.Z);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSnapDirection = snapDirection;
                }
            }
        }

        Vector3 prevPosition = GlobalPosition;
        if (GlobalTransform.Basis.Z.Normalized().Dot(closestSnapDirection) < 0.9999)
        {
            if (Vector3.Up.Cross(closestSnapDirection).IsZeroApprox())
                LookAtFromPosition(Vector3.Zero, -closestSnapDirection, -GlobalTransform.Basis.Z);
            else
                LookAtFromPosition(Vector3.Zero, -closestSnapDirection);
        }
        GlobalPosition = prevPosition;

        RotationDegrees = SnapRotationVector(RotationDegrees);
    }

    private float RoundToNearest(float value, float other)
    {
        return (float)Math.Round(value / other) * other;
    }

    private Vector3 SnapRotationVector(Vector3 vector)
    {
        return new Vector3(vector.X, RoundToNearest(vector.Y, 90.0f), vector.Z);
    }

    public void ShowGrid()
    {
        if (canEnableGrid)
            orthographicGrid.Show();
    }

    public void HideGrid()
    {
        orthographicGrid.Hide();
    }

    public void SetGridEnabled(bool value)
    {
        canEnableGrid = value;
    }
    
    public StandardMaterial3D GetGridMaterial()
    {
        GeometryInstance3D gridGeometry = (GeometryInstance3D)orthographicGrid;
        return (StandardMaterial3D)gridGeometry.MaterialOverride;
    }

    private void SetCameraProjection(Camera3D.ProjectionType projectionType)
    {
        mainCam.Projection = projectionType;
        outlineCam.Projection = projectionType;
    }

    public void SetOutlinesEnabled(bool value)
    {
        outlineContainer.Visible = value;
    }

    public void SetFieldOfView(float value)
    {
        mainCam.Fov = value;
        outlineCam.Fov = value;
    }

    public void SetZoomSpeed(float value)
    {
        zoomSpeed = value;
    }

    public void SetXOrbitSpeed(float value)
    {
        xOrbitSpeed = value;
    }

    public void SetYOrbitSpeed(float value)
    {
        yOrbitSpeed = value;
    }

    public void SetPanSpeed(float value)
    {
        panSpeed = value;
    }

    public void SetXOrbitInverted(bool value)
    {
        invertXOrbit = value;
    }

    public void SetYOrbitInverted(bool value)
    {
        invertYOrbit = value;
    }

    public void SetXPanInverted(bool value)
    {
        invertXPan = value;
    }

    public void SetYPanInverted(bool value)
    {
        invertYPan = value;
    }

    public bool GetOrthogonal()
    {
        return orthogonal;
    }

    public Vector3 GetWorldPos(Vector2 screenPos, float dist)
    {
        return mainCam.ProjectPosition(screenPos, dist);
    }

    public Vector2 GetScreenPos(Vector3 pos)
    {
        return mainCam.UnprojectPosition(pos);
    }

    public Vector3 GetCamPos()
    {
        return mainCam.GlobalPosition / (orthogonal ? ORTHOGONAL_DIST_MULTIPLIER : 1);
    }

    public float GetCamDist(Vector3 pos)
    {
        if (orthogonal)
            return mainCam.Size;

        return GetCamPos().DistanceTo(pos);
    }

    public Vector3 GetCamNormal()
    {
        return mainCam.GlobalTransform.Basis.Z;
    }

    public Transform3D GetCamTransform()
    {
        return mainCam.GlobalTransform;
    }

    public Vector3 GetRayOrigin(Vector2 screenPos)
    {
        return mainCam.ProjectRayOrigin(screenPos);
    }

    public Vector3 GetRayNormal(Vector2 screenPos)
    {
        return mainCam.ProjectRayNormal(screenPos);
    }
}
