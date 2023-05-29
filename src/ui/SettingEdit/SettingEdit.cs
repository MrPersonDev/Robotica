using Godot;
using System;

public partial class SettingEdit : HSplitContainer
{
	[ExportGroup("Properties")]
	[Export]
	private String name;

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
	}

	public String GetName()
	{
		return label.Text;
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
