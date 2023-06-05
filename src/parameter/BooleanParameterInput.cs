using Godot;
using System;

public partial class BooleanParameterInput : ParameterInput
{
    [ExportGroup("Node Paths")]
    [Export]
    private NodePath checkBoxPath;

    private CheckBox checkBox;

    public override void Setup(ParameterType parameterType)
    {
        base.Setup();
        checkBox = (CheckBox)GetNode(checkBoxPath);

        SetupCheckBox((BooleanParameter)parameterType);
    }

    private void SetupCheckBox(BooleanParameter booleanParameter)
    {
        checkBox.ButtonPressed = booleanParameter.defaultValue;
    }

    public override void ConnectEdited(Action<Variant> editAction)
    {
        checkBox.Connect(OptionButton.SignalName.Toggled, Callable.From(editAction));
    }

    public override void SetValue(Variant value)
    {
        checkBox.ButtonPressed = (bool)value;
    }

    public override Variant GetValue()
    {
        return checkBox.ButtonPressed;
    }
}
