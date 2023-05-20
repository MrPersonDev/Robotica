using Godot;
using System;

public partial class ParameterInput : HSplitContainer
{
	private String name = "";

    [ExportGroup("Node Paths")]
	[Export]
	private NodePath labelPath;

	private Label label;

	public void Setup()
	{
		label = (Label)GetNode(labelPath);
	}

	public virtual void Setup(ParameterType parameterType) { }
	public virtual void ConnectEdited(Action<Variant> editAction) { }

	public void SetLabel(String text)
	{
		name = text;
		label.Text = text + ':';
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
