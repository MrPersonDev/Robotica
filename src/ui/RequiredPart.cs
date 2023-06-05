using Godot;
using System;

public partial class RequiredPart : PanelContainer
{
	[ExportGroup("Node Paths")]
	[Export]
	private NodePath labelPath;
	
	private Label label;

	public void Setup()
	{
		label = (Label)GetNode(labelPath);
	}
	
	public void SetText(String value)
	{
		label.Text = value;
	}
}
