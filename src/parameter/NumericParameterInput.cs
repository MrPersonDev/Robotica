using Godot;
using System;

public partial class NumericParameterInput : ParameterInput
{
    [ExportGroup("Node Paths")]
	[Export]
	private NodePath spinBoxPath;

	private SpinBox spinBox;

	public override void Setup(ParameterType floatRangeParameter)
	{
		base.Setup();
		spinBox = (SpinBox)GetNode(spinBoxPath);

		SetupSpinBox((FloatRangeParameter)floatRangeParameter);
	}

	private void SetupSpinBox(FloatRangeParameter floatRangeParameter)
	{
		spinBox.MinValue = floatRangeParameter.GetMin();
		spinBox.MaxValue = floatRangeParameter.GetMax();
		spinBox.Step = floatRangeParameter.GetStep();
		spinBox.Value = floatRangeParameter.GetDefaultValue();
	}

    public override void ConnectEdited(Action<Variant> editAction)
    {
		spinBox.Connect(SpinBox.SignalName.ValueChanged, Callable.From(editAction));
    }

	public override void SetValue(Variant value)
	{
		spinBox.Value = (double)value;
	}

    public override Variant GetValue()
    {
		return spinBox.Value;
    }
}
