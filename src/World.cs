using Godot;
using System;
using System.Collections.Generic;

public partial class World : Node3D
{
    public enum BackgroundColor
    {
        Light,
        Dark
    }

    [ExportGroup("Properties")]
    [Export]
    private Color lightBackground, darkBackground;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath pivotPath, gridPath, controlPath, partsPath, uiPath, worldEnvironmentPath, lightPath;

    private Pivot pivot;
    private Grid grid;
    private Control control;
    private Parts parts;
    private Interface ui;
    private WorldEnvironment worldEnvironment;
    private DirectionalLight3D light;

    public override void _Ready()
    {
        pivot = (Pivot)GetNode(pivotPath);
        grid = (Grid)GetNode(gridPath);
        control = (Control)GetNode(controlPath);
        parts = (Parts)GetNode(partsPath);
        ui = (Interface)GetNode(uiPath);
        worldEnvironment = (WorldEnvironment)GetNode(worldEnvironmentPath);
        light = (DirectionalLight3D)GetNode(lightPath);

        ui.InitializeSettings();
    }

    public override void _Process(double delta)
    {
        grid.UpdatePos(pivot);
        control.UpdateSize();
    }

    public void SetGridEnabled(bool value)
    {
        grid.SetGridEnabled(value);
        pivot.SetGridEnabled(value);

        if (value)
        {
            if (pivot.GetOrthogonal())
                pivot.ShowGrid();
            else
                grid.ShowGrid();
        }
        else
        {
            grid.HideGrid();
            pivot.HideGrid();
        }
    }
    
    public void SetGridColor(Grid.GridColor color)
    {
        Color gridColor = grid.GetGridColor(color);
        StandardMaterial3D gridMaterial = grid.GetGridMaterial();
        StandardMaterial3D orthographicGridMaterial = pivot.GetGridMaterial();
        gridMaterial.AlbedoColor = gridColor;
        orthographicGridMaterial.AlbedoColor = gridColor;
    }
    
    public void SetBackgroundColor(BackgroundColor color)
    {
        switch (color)
        {
            case BackgroundColor.Light:
                worldEnvironment.Environment.BackgroundColor = lightBackground;
                break;
            case BackgroundColor.Dark:
                worldEnvironment.Environment.BackgroundColor = darkBackground;
                break;
        }
    }

    public Parts GetPartsNode()
    {
        return parts;
    }

    public Interface GetInterfaceNode()
    {
        return ui;
    }

    public Pivot GetPivotNode()
    {
        return pivot;
    }

    public Grid GetGridNode()
    {
        return grid;
    }

    public Godot.Environment GetEnvironment()
    {
        return worldEnvironment.Environment;
    }

    public DirectionalLight3D GetLight()
    {
        return light;
    }
}
