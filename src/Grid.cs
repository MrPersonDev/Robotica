using Godot;
using System;

public partial class Grid : Node3D
{
    public enum GridColor
    {
        LightTheme,
        DarkTheme
    }

    private const float AXIS_WIDTH_SCALE = 0.2f;
    private const float AXIS_LENGTH_SCALE = 1000.0f;
    private const float MIN_AXIS_SCALE = 0.75f;

    private bool canEnableGrid = true;

    [ExportGroup("Properties")]
    [Export]
    private Color gridLightThemeColor, gridDarkThemeColor;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath gridLinesPath, xAxisPath, zAxisPath;

    private MeshInstance3D gridLines, xAxis, zAxis;

    public override void _Ready()
    {
        gridLines = (MeshInstance3D)GetNode(gridLinesPath);
        xAxis = (MeshInstance3D)GetNode(xAxisPath);
        zAxis = (MeshInstance3D)GetNode(zAxisPath);
    }

    public void UpdatePos(Pivot pivot)
    {
        Vector3 camPos = pivot.GetCamPos();
        MoveGridLines(camPos);
        ScaleAxisLines(camPos, pivot);
    }

    private void MoveGridLines(Vector3 camPos)
    {
        Vector3 newGridLinesPosition = gridLines.GlobalPosition;
        newGridLinesPosition.X = (float)Math.Floor(camPos.X);
        newGridLinesPosition.Z = (float)Math.Floor(camPos.Z);
        gridLines.GlobalPosition = newGridLinesPosition;
    }

    private void ScaleAxisLines(Vector3 camPos, Pivot pivot)
    {
        float xAxisCamDist = pivot.GetCamDist(new Vector3(camPos.X, 0.0f, 0.0f));
        float zAxisCamDist = pivot.GetCamDist(new Vector3(0.0f, 0.0f, camPos.Z));

        float xAxisWidth = Math.Max(xAxisCamDist * AXIS_WIDTH_SCALE, MIN_AXIS_SCALE);
        float zAxisWidth = Math.Max(zAxisCamDist * AXIS_WIDTH_SCALE, MIN_AXIS_SCALE);

        float lineLength = camPos.Length() * AXIS_LENGTH_SCALE;
        xAxis.Scale = new Vector3(xAxisWidth, lineLength, xAxisWidth);
        zAxis.Scale = new Vector3(zAxisWidth, lineLength, zAxisWidth);
    }
    
    public Color GetGridColor(GridColor color)
    {
        switch (color)
        {
            case GridColor.LightTheme:
                return gridLightThemeColor;
            case GridColor.DarkTheme:
                return gridDarkThemeColor;
            default:
                throw new Exception("Invalid grid color");
        }
    }
    
    public StandardMaterial3D GetGridMaterial()
    {
        GeometryInstance3D gridGeometry = (GeometryInstance3D)gridLines;
        return (StandardMaterial3D)gridGeometry.MaterialOverride;
    }

    public void ShowGrid()
    {
        if (canEnableGrid)
            Show();
    }

    public void HideGrid()
    {
        Hide();
    }

    public void SetGridEnabled(bool value)
    {
        canEnableGrid = value;
    }
}
