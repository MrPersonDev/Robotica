using Godot;
using System;

public partial class HoleBody : Selectable
{
    private const float HOVER_ALPHA = 0.4f;

    private bool isNonInteractive;
    private bool isMotorHole;
    private bool selected = false;
    private float posOffset;
    private Color defaultColor;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D mat;
    [Export]
    private Color selectedColor;
    [Export]
    private bool visible = true;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath meshPath, colliderPath, characterBodyPath;

    private MeshInstance3D mesh;
    private CollisionShape3D collider;
    private CharacterBody3D characterBody;

    public override void _Ready()
    {
        mesh = (MeshInstance3D)GetNode(meshPath);
        collider = (CollisionShape3D)GetNode(colliderPath);
        characterBody = (CharacterBody3D)GetNode(characterBodyPath);

        SetHoleType();
        SetMaterial();
        SetMesh();
        SetDefaultColor();
    }

    private void SetHoleType()
    {
        Hole parent = (Hole)GetParent();
        isNonInteractive = parent.IsNonInteractive();
        isMotorHole = parent.IsMotorHole();
    }

    private void SetMaterial()
    {
        mat = (StandardMaterial3D)mat.Duplicate();
        GeometryInstance3D meshGeometry = (GeometryInstance3D)mesh;
        meshGeometry.MaterialOverride = mat;
    }

    private void SetMesh()
    {
        float size = 0.0f;
        if (mesh.Mesh is BoxMesh)
            size = ((BoxMesh)mesh.Mesh).Size.Z;
        else if (mesh.Mesh is CylinderMesh)
            size = ((CylinderMesh)mesh.Mesh).Height;
        posOffset = size / 2;
    }

    private void SetDefaultColor()
    {
        defaultColor = mat.AlbedoColor;
    }

    public void EnableColliders()
    {
        collider.Disabled = false;
    }

    public void DisableColliders()
    {
        collider.Disabled = true;
    }

    public override void Hover()
    {
        if (isNonInteractive || isMotorHole)
            return;    

        if (!selected)
            mat.AlbedoColor = new Color(mat.AlbedoColor.R, mat.AlbedoColor.G, mat.AlbedoColor.B, HOVER_ALPHA);
    }

    public override void Unhover()
    {
        if (!selected)
            mat.AlbedoColor = new Color(defaultColor.R, defaultColor.G, defaultColor.B, 0.0f);
    }

    public override void Select()
    {
        if (isNonInteractive || isMotorHole)
            return;

        mat.AlbedoColor = selectedColor;
        selected = true;
    }

    public override void Deselect()
    {
        selected = false;
        Unhover();
    }

    public bool IsMotorHoleBody()
    {
        return isMotorHole;
    }

    public Vector3 GetPos()
    {
        if (!IsInsideTree())
            return Vector3.Zero;
        return GlobalPosition + posOffset * GlobalTransform.Basis.Z;
    }

    public Hole GetHole()
    {
        if (!IsInstanceValid(this))
            return null;
        return (Hole)GetParent();
    }
}
