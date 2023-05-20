using Godot;
using System;

public partial class OneSidedHole : Hole
{
    [ExportGroup("Node Paths")]
    [Export]
    private NodePath bodyPath;
    [Export]
    private bool visible = true;

    private HoleBody body;

    public override void _Ready()
    {
        base._Ready();

        body = (HoleBody)GetNode(bodyPath);

        SetVisibility();
    }

    private void SetVisibility()
    {
        body.Visible = visible;
    }

    public override void EnableColliders()
    {
        if (!IsInstanceValid(body))
            return;

        if (ShouldEnable())
            body.EnableColliders();
    }

    public override void DisableColliders()
    {
        if (!IsInstanceValid(body))
            return;

        body.DisableColliders();
    }

    public override HoleBody GetDefaultHoleBody()
    {
        return body;
    }

    public override HoleBody GetOpposingBody(HoleBody current)
    {
        return current;
    }
}
