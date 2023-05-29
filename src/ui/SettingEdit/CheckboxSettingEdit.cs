using Godot;
using System;

public partial class CheckboxSettingEdit : SettingEdit
{
	[ExportGroup("Node paths")]
	[Export]
	private NodePath checkboxPath;

	private CheckBox checkbox;

	public override void _Ready()
	{
		base._Ready();

		checkbox = (CheckBox)GetNode(checkboxPath);
	}

	public override void SetValue(Variant value)
	{
		checkbox.ButtonPressed = (bool)value;
	}

	public override Variant GetValue()
	{
		return checkbox.ButtonPressed;
	}
}
