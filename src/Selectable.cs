using Godot;
using System;

public abstract partial class Selectable : Node3D
{
    public abstract void Hover();
    public abstract void Unhover();
    public abstract void Select();
    public abstract void Deselect();
}
