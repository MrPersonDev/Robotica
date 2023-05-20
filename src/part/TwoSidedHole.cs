using Godot;
using System;

public partial class TwoSidedHole : Hole
{
    [ExportGroup("Properties")]
    [Export]
    private float length = 0.263f;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath topBodyPath, bottomBodyPath;

    private HoleBody topBody, bottomBody;

    public override void _Ready()
    {
        base._Ready();

        topBody = (HoleBody)GetNode(topBodyPath);
        bottomBody = (HoleBody)GetNode(bottomBodyPath);

        SetHoleBodyOrientations();
    }

    private void SetHoleBodyOrientations()
    {
        topBody.Position = new Vector3(0.0f, 0.0f, length / 2);
        bottomBody.Position = new Vector3(0.0f, 0.0f, -length / 2);
        bottomBody.RotationDegrees = new Vector3(0.0f, -180.0f, 0.0f);
    }

    public override void EnableColliders()
    {
        if (!IsInstanceValid(topBody) || !IsInstanceValid(bottomBody))
            return;

        if (ShouldEnable())
        {
            topBody.EnableColliders();
            bottomBody.EnableColliders();
        }
    }

    public override void DisableColliders()
    {
        if (!IsInstanceValid(topBody) || !IsInstanceValid(bottomBody))
            return;

        topBody.DisableColliders();
        bottomBody.DisableColliders();
    }

    public override HoleBody GetDefaultHoleBody()
    {
        return topBody;
    }

    public override HoleBody GetOpposingBody(HoleBody current)
    {
        if (current == topBody)
            return bottomBody;
        else if (current == bottomBody)
            return topBody;
        else
            return null;
    }
}
