using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class Part : Moveable
{
    private const int HOLES_PER_FRAME = 20;
    private const float HOLE_OFFSET = 0.1f;
    private const int MAIN_MESH_LAYER = 1;
    private const int OUTLINE_MESH_LAYER = 2;
    private const float OVERLAY_OPACITY = 0.1f;

    private bool loaded = false;
    private Vector3 overlappingFixOffset = new Vector3(0.0f, 0.0001f, 0.0f);
    private StandardMaterial3D overlayMaterial;
    private Color defaultColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    private Color selectionColor = new Color(0.0f, 0.3f, 0.5f, OVERLAY_OPACITY);
    private Color actualSelectionColor = new Color(0.15f, 0.45f, 1.0f, OVERLAY_OPACITY);
    private bool actuallySelected = false;
    private PartGroup partGroup = null;
    private float length = float.MaxValue, height = float.MaxValue, width = float.MaxValue;
    private Hole defaultHole, shaftHole, motorHole, highStrengthMotorHole;
    private bool preparingToDelete = false;
    private bool deleted = false;
    private PartOption partOption;
    private Dictionary<String, Variant> parameters;
    private StaticBody3D partMeshStaticBody;
    private CollisionShape3D partMeshCollider;
    private List<CollisionShape3D> additionalMeshColliders = new List<CollisionShape3D>();
    private bool cutting = false;
    private bool queuedForCutting = false;
    private float queuedLength, queuedHeight, queuedWidth;
    private bool requiresUpdate = false;
    private bool chainFlipped = false;
    // private List<Hole> queuedHoles = null;

    [ExportGroup("Properties")]
    [Export]
    private bool isMotor;
    [Export]
    private bool isChain;
    [Export]
    private bool isHighStrengthShaft;
    [Export]
    private bool enableMeshCutter = false;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath partMeshPath, additionalMeshesPath, centerPath, holesPath, insertsPath, chainRotationPath, chainHolePath;

    private MeshCutter partMesh;
    private Node3D additionalMeshes, center, holes, inserts, otherMeshes, chainRotation, chainHole;

    public void Setup(bool loaded = false)
    {
        partMesh = (MeshCutter)GetNode(partMeshPath);
        additionalMeshes = (Node3D)GetNode(additionalMeshesPath);
        center = (Node3D)GetNode(centerPath);
        holes = (Node3D)GetNode(holesPath);
        inserts = (Node3D)GetNode(insertsPath);
        if (!(chainRotationPath == null))
            chainRotation = (Node3D)GetNode(chainRotationPath);
        if (!(chainHolePath == null))
            chainHole = (Node3D)GetNode(chainHolePath);

        this.loaded = loaded;

        ShowPartMesh();
        SetCollider();
        SetOverlayMaterial();
    }

    public override void Initialize()
    {
        SetCenter();
    }

    public async Task SetMeshCutterSize(float length, float height, float width)
    {
        if (!enableMeshCutter)
            return;

        HidePartMesh();
        await Task.Run(() => SetMeshCutterSizeTask(length, height, width));
        ShowPartMesh();
    }

    private void SetMeshCutterSizeTask(float length, float height, float width)
    {
        if (cutting)
        {
            queuedForCutting = true;
            queuedLength = length;
            queuedHeight = height;
            queuedWidth = width;
            return;
        }

        cutting = true;

        this.length = length;
        this.height = height;
        this.width = width;

        partMesh.SetMeshSize(width, height, length);

        if (length == MeshCutter.NO_CUT)
            this.length = float.MaxValue;
        if (height == MeshCutter.NO_CUT)
            this.height = float.MaxValue;
        if (width == MeshCutter.NO_CUT)
            this.width = float.MaxValue;

        if (preparingToDelete || !IsInstanceValid(this) || !IsInstanceValid(center))
        {
            cutting = false;
            return;
        }

        HideExcludedHoles(this.width, this.height, this.length);

        CallDeferred("SetCollider");
        Vector3 prevCenter = center.Position;
        CallDeferred("SetCenter");
        if (!loaded)
            CallDeferred("MoveToNewCenter", prevCenter);

        cutting = false;
        if (queuedForCutting)
        {
            queuedForCutting = false;
            SetMeshCutterSizeTask(queuedLength, queuedHeight, queuedWidth);
        }
    }

    public void SetInsertSize(float length)
    {
        if (inserts.GetChildCount() != 1)
            throw new Exception("Incorrect number of inserts");

        Insert insert = (Insert)inserts.GetChildren()[0];
        insert.SetLength(length);
        insert.Position = new Vector3(insert.Position.X, insert.Position.Y, -length / 2.0f);

        if (shaftHole != null)
            shaftHole.Position = new Vector3(insert.Position.X, insert.Position.Y, -length / 2.0f);
    }

    private void SetCollider()
    {
        if (!IsInstanceValid(partMesh) || partMesh.Mesh == null || partMesh.Mesh.GetFaces().Length == 0)
            return;

        if (partMeshStaticBody != null)
            RemoveCollider();

        partMesh.CreateTrimeshCollision();

        if (partMesh.GetChildCount() == 0)
            return;

        partMeshStaticBody = (StaticBody3D)partMesh.GetChildren()[partMesh.GetChildCount() - 1];
        partMeshStaticBody.AddToGroup(partMesh.GetGroups()[0]);
        partMesh.RemoveChild(partMeshStaticBody);
        // CallDeferred("add_child", partMeshStaticBody);
        AddChild(partMeshStaticBody);

        partMeshCollider = (CollisionShape3D)partMeshStaticBody.GetChild(0);
    }

    public void SetAdditionalMeshColliders()
    {
        if (!IsInsideTree())
            return;

        foreach (MeshInstance3D additionalMesh in additionalMeshes.GetChildren())
            SetAdditionalMeshCollider(additionalMesh);
    }

    private void SetAdditionalMeshCollider(MeshInstance3D additionalMesh)
    {
        additionalMesh.CreateTrimeshCollision();

        StaticBody3D additionalMeshStaticBody = (StaticBody3D)additionalMesh.GetChildren()[additionalMesh.GetChildCount() - 1];
        additionalMeshStaticBody.AddToGroup(additionalMeshes.GetGroups()[0], true);
        Transform3D prevTransform = additionalMeshStaticBody.GlobalTransform;
        additionalMesh.RemoveChild(additionalMeshStaticBody);
        AddChild(additionalMeshStaticBody);
        additionalMeshStaticBody.GlobalTransform = prevTransform;

        additionalMeshColliders.Add((CollisionShape3D)additionalMeshStaticBody.GetChild(0));
    }

    public void RemoveCollider()
    {
        if (!IsInstanceValid(partMeshStaticBody))
            return;

        if (GetChildren().Contains(partMeshStaticBody))
            RemoveChild(partMeshStaticBody);
        partMeshStaticBody.QueueFree();
    }

    private void HideExcludedHoles(float width, float height, float length)
    {
        float maxX = width - HOLE_OFFSET;
        float maxY = height - HOLE_OFFSET;
        float minZ = -length + HOLE_OFFSET;

        foreach (Hole hole in holes.GetChildren())
        {
            Vector3 holePos = hole.Position;
            bool shouldHideHole = holePos.X > maxX || holePos.Y > maxY || holePos.Z < minZ;
            hole.SetShouldEnable(!shouldHideHole);
        }
    }

    public void SetMaterial(StandardMaterial3D material)
    {
        partMesh.SetMaterial(material);
    }

    public void SetAdditionalMeshMaterial(StandardMaterial3D material)
    {
        foreach (MeshInstance3D additionalMesh in additionalMeshes.GetChildren())
            additionalMesh.MaterialOverride = material;
    }

    private void SetOverlayMaterial()
    {
        overlayMaterial = new StandardMaterial3D();
        overlayMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        overlayMaterial.AlbedoColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        partMesh.MaterialOverlay = overlayMaterial;
    }

    private void SetCenter()
    {
        if (!IsInstanceValid(partMesh) || !IsInstanceValid(center))
            return;

        Aabb boundingBox = partMesh.GetAabb();
        Vector3 centerPos = boundingBox.GetCenter();
        center.Position = centerPos;
    }

    private void MoveToNewCenter(Vector3 prevCenter)
    {
        if (!IsInsideTree())
            return;

        GlobalPosition += (prevCenter - center.Position);
    }

    public override void UpdateCollisionTransform()
    {
        holes.GlobalTransform = GlobalTransform;
        inserts.GlobalTransform = GlobalTransform;
    }

    // public override void _Process(double delta)
    // {
    //     if (queuedHoles != null && queuedHoles.Count > 0)
    //     {
    //         int endIdx = Math.Max(queuedHoles.Count - HOLES_PER_FRAME - 1, 0);
    //         for (int i = queuedHoles.Count - 1; i >= endIdx; i--)
    //         {
    //             if (preparingToDelete || !IsInstanceValid(holes) || !IsInsideTree())
    //                 return;

    //             Hole curHole = (Hole)queuedHoles[i].Duplicate();
    //             AddHole(curHole);

    //             queuedHoles.RemoveAt(i);
    //         }

    //         if (queuedHoles.Count == 0)
    //         {
    //             HideExcludedHoles(width, height, length); 
    //             DisableColliders(false);
    //         }
    //     }
    // }

    public override void MoveTo(Vector3 pos, bool meshOnly = false)
    {
        if (!IsInsideTree())
            return;

        Vector3 offset = GlobalPosition - center.GlobalPosition;
        GlobalPosition = pos + offset;

        if (!meshOnly)
            UpdateCollisionTransform();
    }

    public override void RotateCenter(Vector3 axis, float rotation, bool meshOnly = false)
    {
        if (!IsInsideTree())
            return;

        Vector3 originalPosition = GetCenter();
        Rotate(axis.Normalized(), rotation);
        MoveTo(originalPosition);

        if (!meshOnly)
            UpdateCollisionTransform();
    }

    public override void RotatePos(Vector3 axis, float rotation, Vector3 pos, bool meshOnly = false)
    {
        if (!IsInsideTree())
            return;

        RotateCenter(axis, rotation, meshOnly);
        Vector3 displacementFromCenter = GetCenter() - pos;
        displacementFromCenter = displacementFromCenter.Rotated(axis.Normalized(), rotation);
        MoveTo(pos + displacementFromCenter, meshOnly);

        if (!meshOnly)
            UpdateCollisionTransform();
    }

    public void MoveToMotor(Part motor, bool meshOnly = false)
    {
        if (!IsInsideTree())
            return;

        Hole hole = motor.GetMotorHole(isHighStrengthShaft);
        Vector3 pos = hole.GetDefaultHoleBody().GetPos();

        Vector3 origin = center.GlobalPosition;
        Vector3 direction = GlobalTransform.Basis.Z;
        Vector3 difference = GlobalPosition - origin;
        float dotP = difference.Dot(direction);
        Vector3 closestPointOnAxis = origin + direction * dotP;
        Vector3 centerOffset = GlobalPosition - closestPointOnAxis;

        GlobalPosition = pos + centerOffset;

        if (!meshOnly)
            UpdateCollisionTransform();
    }

    public override void EnableMeshCollider()
    {
        if (deleted || !IsInstanceValid(partMeshCollider) || !IsInstanceValid(additionalMeshes))
            return;

        partMeshCollider.Disabled = false;
        foreach (CollisionShape3D additionalMeshCollider in additionalMeshColliders)
            additionalMeshCollider.Disabled = false;
    }

    public override void DisableMeshCollider()
    {
        if (!IsInstanceValid(partMeshCollider) || !IsInstanceValid(additionalMeshes))
            return;

        partMeshCollider.Disabled = true;
        foreach (CollisionShape3D additionalMeshCollider in additionalMeshColliders)
            additionalMeshCollider.Disabled = true;
    }

    public override void EnableColliders()
    {
        if (deleted || !IsInstanceValid(holes) || !IsInstanceValid(inserts))
            return;

        holes.Show();
        foreach (Hole hole in holes.GetChildren())
            hole.EnableColliders();
        foreach (Insert insert in inserts.GetChildren())
            insert.EnableColliders();
    }

    public override void DisableColliders(bool disableInserts)
    {
        if (!IsInstanceValid(holes) || !IsInstanceValid(inserts))
            return;

        holes.Hide();
        foreach (Hole hole in holes.GetChildren())
            hole.DisableColliders();
        if (disableInserts)
            foreach (Insert insert in inserts.GetChildren())
                insert.DisableColliders();
    }

    public void OverlappingFix()
    {
        /* 
            There's a weird glitch (or weirdly implemented feature) with Area3D 
            in which GetOverlappingBodies() wont find some of the parts unless
            it is moved slightly and thus is forced to update
        */
        MoveTo(GetCenter() + overlappingFixOffset);
        overlappingFixOffset *= -1;
    }

    public void ActuallySelect()
    {
        actuallySelected = true;
        Select();

        ChangeMeshColor(actualSelectionColor);
    }

    public override void Select()
    {
        SetMeshLayer(OUTLINE_MESH_LAYER);

        if (!actuallySelected)
            ChangeMeshColor(selectionColor);
    }

    public override void Deselect()
    {
        SetMeshLayer(MAIN_MESH_LAYER);
        ResetMeshColor();

        actuallySelected = false;
    }

    private void SetMeshLayer(uint layer)
    {
        if (!IsInstanceValid(partMesh) || !IsInstanceValid(additionalMeshes))
            return;

        partMesh.Layers = layer;

        foreach (MeshInstance3D additionalMesh in additionalMeshes.GetChildren())
            additionalMesh.Layers = layer;
    }

    private void ChangeMeshColor(Color additionalColor)
    {
        overlayMaterial.AlbedoColor = additionalColor;
    }

    private void ResetMeshColor()
    {
        overlayMaterial.AlbedoColor = defaultColor;
    }

    public override void Hover() { }
    public override void Unhover() { }

    public void ShowPartMesh()
    {
        if (!IsInstanceValid(partMesh) || !IsInstanceValid(additionalMeshes))
            return;

        partMesh.Show();
        additionalMeshes.Show();
    }

    public void HidePartMesh()
    {
        if (!IsInstanceValid(partMesh) || !IsInstanceValid(additionalMeshes))
            return;

        partMesh.Hide();
        additionalMeshes.Hide();
    }

    private void AddHole(Hole hole)
    {
        holes.CallDeferred("add_child", hole);
        // holes.AddChild(hole);
        if (hole.IsDefaultHole())
            defaultHole = hole;
        if (hole.IsNonInteractive())
            shaftHole = hole;
        if (hole.IsMotorHole())
        {
            if (hole.IsHighStrength())
                highStrengthMotorHole = hole;
            else
                motorHole = hole;
        }
    }

    public async Task CreateHoles(List<Hole> holeList)
    {
        // queuedHoles = new List<Hole>(holeList);

        for (int i = 0; i < holeList.Count; i += HOLES_PER_FRAME)
        {
            int count = Math.Min(HOLES_PER_FRAME, holeList.Count - i);
            for (int j = i; j < i + count; j++)
            {
                if (preparingToDelete || !IsInstanceValid(holes) || !IsInsideTree())
                    return;

                Hole curHole = (Hole)holeList[j].Duplicate();
                AddHole(curHole);
            }

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        HideExcludedHoles(width, height, length);
        DisableColliders(false);
    }

    public void RemoveHoles()
    {
        List<Hole> holeList = GetHoles();
        foreach (Hole hole in holeList)
            holes.RemoveChild(hole);
    }

    public override void PrepareToDelete()
    {
        preparingToDelete = true;
    }

    public void Delete()
    {
        if (!IsInstanceValid(this))
            return;

        GetParent().CallDeferred("remove_child", this);
        QueueFree();
    }

    public void PretendDelete()
    {
        HidePartMesh();
        DisableColliders(true);
        DisableMeshCollider();
        if (partGroup != null)
            partGroup.RemovePart(this, true);

        deleted = true;
    }

    public void Undelete()
    {
        deleted = false;

        ShowPartMesh();
        EnableMeshCollider();
    }

    public void SetPartGroup(PartGroup val)
    {
        partGroup = val;
    }

    public void StorePartOption(PartOption partOption)
    {
        this.partOption = partOption;
    }

    public void StoreParameters(Dictionary<String, Variant> parameters)
    {
        this.parameters = parameters;
    }

    public bool IsDeleted()
    {
        return deleted;
    }

    public bool IsEnabled()
    {
        return holes.Visible;
    }

    public PartGroup GetPartGroup()
    {
        return partGroup;
    }

    public override Vector3 GetCenter()
    {
        if (!IsInstanceValid(center) || !center.IsInsideTree())
            return Vector3.Zero;

        return center.GlobalPosition;
    }

    public override bool HasDefaultHole()
    {
        return defaultHole != null;
    }

    public override HoleBody GetDefaultHoleBody()
    {
        UpdateCollisionTransform();
        return defaultHole.GetDefaultHoleBody();
    }

    public Hole GetMotorHole(bool isHighStrengthShaft)
    {
        if (isHighStrengthShaft)
            return highStrengthMotorHole;
        else
            return motorHole;
    }

    public override bool HasInsert()
    {
        if (!IsInstanceValid(inserts))
            return false;
        return inserts.GetChildCount() > 0;
    }

    public override bool HasFasteners()
    {
        if (!IsInstanceValid(inserts))
            return false;

        foreach (Insert insert in inserts.GetChildren())
        {
            if (insert.HasFasteners())
                return true;
        }

        return false;
    }

    public bool HasFastenerHoles()
    {
        if (!IsInstanceValid(holes))
            return false;

        foreach (Hole hole in holes.GetChildren())
        {
            if (hole.IsFastener())
                return true;
        }

        return false;
    }

    public bool IsMotor()
    {
        return isMotor;
    }
    
    public bool IsChain()
    {
        return isChain;
    }
    
    public bool IsChainFlipped()
    {
        return chainFlipped;
    }
    
    public void SetChainFlipped(bool value)
    {
        chainFlipped = value;
    }
    
    public Vector3 GetChainRotationPosition()
    {
        if (!IsInstanceValid(chainRotation))
            return Vector3.Zero;

        return chainRotation.GlobalPosition;
    }
    
    public Vector3 GetChainHolePosition()
    {
        if (!IsInstanceValid(chainHole))
            return Vector3.Zero;
        
        return chainHole.GlobalPosition;
    }

    public List<Part> GetCollidingFastenedParts()
    {
        List<Part> parts = new List<Part>();

        foreach (Insert insert in inserts.GetChildren())
            parts.AddRange(insert.GetCollidingFastenedParts());

        return parts;
    }

    public override List<Part> GetParts()
    {
        List<Part> parts = new List<Part>();
        parts.Add(this);
        return parts;
    }

    public List<Hole> GetHoles()
    {
        List<Node> children = new List<Node>(holes.GetChildren());
        List<Hole> holeList = new List<Hole>();

        foreach (Node child in children)
            holeList.Add((Hole)child);

        return holeList;
    }

    public PartOption GetPartOption()
    {
        return partOption;
    }

    public Dictionary<String, Variant> GetParameters()
    {
        return parameters;
    }

    public void SetRequiresUpdate(bool val)
    {
        requiresUpdate = val;
    }

    public bool RequiresUpdate()
    {
        return requiresUpdate;
    }

    public void IncreaseInsertWidth()
    {
        if (!IsInstanceValid(inserts))
            return;

        foreach (Insert insert in inserts.GetChildren())
            insert.IncreaseWidth();
    }

    public void ResetInsertWidth()
    {
        if (!IsInstanceValid(inserts))
            return;

        foreach (Insert insert in inserts.GetChildren())
            insert.ResetWidth();
    }

    public List<Part> GetCurCollidingParts()
    {
        List<Part> collidingParts = new List<Part>();

        if (!IsInstanceValid(inserts))
            return collidingParts;

        foreach (Insert insert in inserts.GetChildren())
            collidingParts.AddRange(insert.GetCurCollidingParts());
        return collidingParts;
    }

    public List<Part> GetPotentiallyCollidingParts()
    {
        List<Part> potentiallyCollidingParts = new List<Part>();
        foreach (Insert insert in inserts.GetChildren())
            potentiallyCollidingParts.AddRange(insert.GetPotentiallyCollidingParts());
        return potentiallyCollidingParts;
    }

    public Dictionary<String, Variant> GetSaveInfo()
    {
        String name = Name;
        Transform3D transform = GlobalTransform;
        String partGroup = (GetPartGroup() != null) ? GetPartGroup().ToString() : "";
        NodePath partOptionName = partOption.GetName();
        Godot.Collections.Dictionary<String, Variant> godotDict = new Godot.Collections.Dictionary<String, Variant>(parameters);

        return new Dictionary<String, Variant>() {
            { "Name", name },
            { "X", transform.Basis.X },
            { "Y", transform.Basis.Y },
            { "Z", transform.Basis.Z },
            { "O", transform.Origin },
            { "PartGroup", partGroup },
            { "PartOption", partOptionName },
            { "Parameters", godotDict }
        };
    }
}
