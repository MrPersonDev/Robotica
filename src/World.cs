using Godot;
using System;
using System.Collections.Generic;

public partial class World : Node3D
{
    [ExportGroup("Node Paths")]
    [Export]
    private NodePath pivotPath, gridPath, controlPath, partsPath, uiPath;

    private Pivot pivot;
    private Grid grid;
    private Control control;
    private Parts parts;
    private Interface ui;

    public override void _Ready()
    {
        pivot = (Pivot)GetNode(pivotPath);
        grid = (Grid)GetNode(gridPath);
        control = (Control)GetNode(controlPath);
        parts = (Parts)GetNode(partsPath);
        ui = (Interface)GetNode(uiPath);
    }

    public override void _Process(double delta)
    {
        grid.UpdatePos(pivot);
        control.UpdateSize();
    }

    public Parts GetPartsNode()
    {
        return parts;
    }

    public Interface GetInterfaceNode()
    {
        return ui;
    }
}
