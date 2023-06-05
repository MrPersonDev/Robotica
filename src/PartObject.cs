using Godot;
using System;

public class PartObject
{
    public PackedScene Object { get; set; }

    public PartObject(PackedScene scene)
    {
        this.Object = scene;
    }
}
