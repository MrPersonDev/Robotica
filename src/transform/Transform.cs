using Godot;
using System;

public enum TransformMode
{
    TRANSLATE,
    ROTATE,
    BOTH
}

public partial class Transform : Node3D
{
    [ExportGroup("Parameters")]
    [Export]
    private TransformMode transformMode;

    public override void _Ready()
    {
        Disable();
    }

    public void Enable()
    {
        Visible = true;
        foreach (Handle handle in GetChildren())
        {
            bool showRotation = transformMode == TransformMode.ROTATE;
            bool showHandle = handle.GetRotating() == showRotation || transformMode == TransformMode.BOTH;
            if (showHandle)
                handle.Enable();
        }
    }

    public void Disable()
    {
        Visible = false;
        foreach (Handle handle in GetChildren())
            handle.Disable();
    }

    public void UpdateSize(Pivot pivot)
    {
        foreach (Handle handle in GetChildren())
            handle.UpdateSize(pivot);
    }

    public void SetTransformMode(TransformMode mode)
    {
        this.transformMode = mode;

        bool prevVisible = Visible;
        Disable();

        if (prevVisible)
            Enable();
    }
}
