using Godot;
using System;

public partial class SettingEdit : HSplitContainer
{
    [ExportGroup("Properties")]
    [Export]
    private String name;
    [Export]
    private bool requiresRestart = false;

    [ExportGroup("Node paths")]
    [Export]
    private NodePath labelPath;

    private Label label;

    public override void _Ready()
    {
        label = (Label)GetNode(labelPath);

        SetName();
    }

    private void SetName()
    {
        label.Text = name;
        if (requiresRestart)
            label.Text += "*";
    }

    public String GetName()
    {
        return name;
    }

    public virtual void SetValue(Variant value)
    {
        throw new NotImplementedException();
    }

    public virtual Variant GetValue()
    {
        throw new NotImplementedException();
    }
}
