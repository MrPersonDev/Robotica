using Godot;
using System;
using System.Collections.Generic;

public partial class HotBox : Godot.Control
{
    private List<HotBoxOption> children = new List<HotBoxOption>();
    private HotBoxOption currentHovering = null;

    [ExportGroup("Properties")]
    [Export]
    private float dist = 120.0f;
    [Export]
    private Godot.Collections.Array<String> partNames = new Godot.Collections.Array<String>();
    [Export]
    private PackedScene hotBoxOptionScene;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath linePath;

    private Line2D line;

    public override void _Ready()
    {
        line = (Line2D)GetNode(linePath);

        SetHotBoxOptions();
    }

    private void SetHotBoxOptions()
    {
        ClearOptions();

        float angleBetween = (float)((360.0f / partNames.Count) * (Math.PI / 180.0f));
        float angle = 0.0f;
        for (int i = 0; i < partNames.Count; i++)
        {
            float dx = (float)Math.Sin(angle) * dist;
            float dy = (float)Math.Cos(angle) * dist;

            HotBoxOption newHotBoxOption = (HotBoxOption)hotBoxOptionScene.Instantiate();
            AddChild(newHotBoxOption);
            children.Add(newHotBoxOption);
            newHotBoxOption.Setup(partNames[i]);

            newHotBoxOption.Position += new Vector2(dx, dy);

            angle += angleBetween;
        }
    }

    private void ClearOptions()
    {
        foreach (Node child in children)
        {
            RemoveChild(child);
            child.QueueFree();
        }

        children.Clear();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!Visible || !(inputEvent is InputEventMouseMotion))
            return;

        UpdateLine();
        UpdateHovering();
    }

    private void UpdateLine()
    {
        line.Points[1] = GetViewport().GetMousePosition() - GlobalPosition;
        line.SetPointPosition(1, GetViewport().GetMousePosition() - GlobalPosition);
    }

    private void UpdateHovering()
    {
        float d = 360.0f / partNames.Count;
        Vector2 relativeMousePos = (GetViewport().GetMousePosition() - GlobalPosition).Normalized();
        float angle = Mathf.Atan2(-relativeMousePos.Y, relativeMousePos.X) / (float)(Math.PI / 180.0f);
        angle += 90.0f;

        if (angle < 0)
            angle += 360.0f;

        float minDist = float.MaxValue;
        int minIdx = 0;
        for (int i = 0; i < partNames.Count; i++)
        {
            float partNameAngle = i * d;

            float edgeDist = Math.Min(Math.Abs((partNameAngle - 360.0f) - angle), Math.Abs((angle - 360.0f) - partNameAngle));
            float dist = Math.Min(Math.Abs(partNameAngle - angle), edgeDist);

            if (dist < minDist)
            {
                minDist = dist;
                minIdx = i;
            }
        }

        HotBoxOptionHovered(children[minIdx]);
    }

    private void HotBoxOptionHovered(HotBoxOption hotBoxOption)
    {
        if (currentHovering != null)
            currentHovering.HideIndicator();
        currentHovering = hotBoxOption;
        currentHovering.ShowIndicator();
    }

    public void Start()
    {
        Show();
    }

    public void End()
    {
        if (currentHovering != null)
        {
            currentHovering.Select();
            currentHovering.HideIndicator();
        }

        currentHovering = null;
        Hide();
    }

    public void SetItems(Godot.Collections.Array<String> items)
    {
        partNames = items;
        SetHotBoxOptions();
    }

    public void SetDist(float dist)
    {
        this.dist = dist;
        SetHotBoxOptions();
    }
}
