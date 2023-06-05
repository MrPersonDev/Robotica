using Godot;
using System;

public partial class DropdownSettingEdit : SettingEdit
{
    [ExportGroup("Properties")]
    [Export]
    private Godot.Collections.Array<String> values = new Godot.Collections.Array<String>();

    [ExportGroup("Node paths")]
    [Export]
    private NodePath optionButtonPath;

    private OptionButton optionButton;

    public override void _Ready()
    {
        base._Ready();

        optionButton = (OptionButton)GetNode(optionButtonPath);

        SetOptionButtonValues();
    }

    private void SetOptionButtonValues()
    {
        foreach (String value in values)
            optionButton.AddItem(value);
    }

    public override void SetValue(Variant value)
    {
        for (int i = 0; i < optionButton.ItemCount; i++)
        {
            if (optionButton.GetItemText(i).Equals((String)value))
            {
                optionButton.Selected = i;
                break;
            }
        }
    }

    public override Variant GetValue()
    {
        return optionButton.GetItemText(optionButton.Selected);
    }
}
