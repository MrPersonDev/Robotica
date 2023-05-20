using Godot;
using System;

public partial class Handle : Selectable
{
    private enum TransformType
    {
        MOVE, ROTATE
    }

    private const float DIST_SCALE = 0.3f;
    private const float MIN_SIZE = 0.001f;
    private const float HOVER_BRIGHTNESS = 0.4f;

    private Vector3 colliderOffset;
    private Color normalColor, hoverColor;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D mat;
    [Export]
    private Color color;
    [Export]
    private Vector3 meshRotation;
    [Export]
    private Vector3 normal;
    [Export]
    private TransformType type;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath meshesPath, colliderPath, staticBodyPath;

    private Node3D meshes;
    private CollisionShape3D collider;
    private StaticBody3D staticBody;

    public override void _Ready()
    {
        meshes = (Node3D)GetNode(meshesPath);
        collider = (CollisionShape3D)GetNode(colliderPath);
        staticBody = (StaticBody3D)GetNode(staticBodyPath);

        SetRotation();
        SetMaterial();
        SetColors();
        SetColliderOffset();
    }

    private void SetRotation()
    {
        meshes.GlobalRotationDegrees = meshRotation;
        staticBody.GlobalRotationDegrees = meshRotation;
    }

    private void SetMaterial()
    {
        mat = (StandardMaterial3D)mat.Duplicate();

        foreach (MeshInstance3D mesh in meshes.GetChildren())
        {
            GeometryInstance3D meshGeometry = (GeometryInstance3D)mesh;
            meshGeometry.MaterialOverride = mat;
        }
    }

    private void SetColors()
    {
        normalColor = new Color(color.R, color.G, color.B, mat.AlbedoColor.A);
        hoverColor = normalColor + new Color(1.0f, 1.0f, 1.0f) * HOVER_BRIGHTNESS;

        mat.AlbedoColor = normalColor;
    }

    private void SetColliderOffset()
    {
        colliderOffset = collider.Position;
    }

    public override void Select() { }
    public override void Deselect() { }

    public override void Hover()
    {
        mat.AlbedoColor = hoverColor;
    }

    public override void Unhover()
    {
        mat.AlbedoColor = normalColor;
    }

    public void Enable()
    {
        collider.Disabled = false;
        Visible = true;
    }

    public void Disable()
    {
        collider.Disabled = true;
        Visible = false;
    }

    public void UpdateSize(Pivot pivot)
    {
        float cameraDist = pivot.GetCamDist(meshes.GlobalPosition) * DIST_SCALE;
        float size = Math.Max(cameraDist * DIST_SCALE, MIN_SIZE);
        Vector3 scale = Vector3.One * size;

        meshes.Scale = scale;
        collider.Scale = scale;

        Vector3 colliderPos = Position + colliderOffset * size;
        collider.Position = colliderPos;
    }

    public Vector3 GetAxis()
    {
        return normal;
    }

    public bool GetRotating()
    {
        return type == TransformType.ROTATE;
    }
}
