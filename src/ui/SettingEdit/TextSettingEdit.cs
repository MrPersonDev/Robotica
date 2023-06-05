using Godot;
using System;

public partial class TextSettingEdit : SettingEdit
{
    [ExportGroup("Node paths")]
    [Export]
    private NodePath textEditPath;

    private TextEdit textEdit;

    public override void _Ready()
    {
        base._Ready();

        textEdit = (TextEdit)GetNode(textEditPath);
    }

    public override void SetValue(Variant value)
    {
        textEdit.Text = (String)value;
    }

    public override Variant GetValue()
    {
        return textEdit.Text;
    }
}
