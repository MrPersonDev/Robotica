using Godot;
using System;

public abstract partial class Hole : Node3D
{
    public enum Shape
    {
        SQUARE,
        CIRCLE,
        DIAMOND
    }

    private const float HS_SCALE = 1.45f;
    private const float SHAFT_SCALE = 0.8f;
    private const float HOVER_ALPHA = 0.4f;
    private bool shouldEnable = true;

    [ExportGroup("Properties")]
    [Export]
    private float width = 1.0f;
    [Export]
    private bool isDefaultHole = false;
    [Export]
    private bool isFastener = false;
    [Export]
    private bool isNonInteractive = false;
    [Export]
    private bool isMotorHole = false;
    [Export]
    private bool isHighStrength = false;

    [ExportGroup("HoleBody Scenes")]
    private PackedScene squareHoleBodyScene, circleHoleBodyScene, diamondHoleBodyScene;

    public override void _Ready()
    {
        ScaleHole();
    }

    private void ScaleHole()
    {
        Scale = new Vector3(width, width, 1.0f);
    }

    public Part GetPart()
    {
        return (Part)GetParent().GetParent();
    }

    public void SetShouldEnable(bool value)
    {
        shouldEnable = value;
    }

    public bool ShouldEnable()
    {
        return shouldEnable && !isNonInteractive;
    }

    public bool IsDefaultHole()
    {
        return isDefaultHole;
    }

    public bool IsFastener()
    {
        return isFastener;
    }

    public bool IsNonInteractive()
    {
        return isNonInteractive;
    }

    public bool IsMotorHole()
    {
        return isMotorHole;
    }

    public bool IsHighStrength()
    {
        return isHighStrength;
    }

    public abstract void EnableColliders();
    public abstract void DisableColliders();
    public abstract HoleBody GetOpposingBody(HoleBody current);
    public abstract HoleBody GetDefaultHoleBody();
}
