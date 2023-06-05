using Godot;
using System;

public partial class DropdownParameterInput : ParameterInput
{
    [ExportGroup("Node Paths")]
    [Export]
    private NodePath optionButtonPath;

    private OptionButton optionButton;

    public override void Setup(ParameterType parameterType)
    {
        base.Setup();
        optionButton = (OptionButton)GetNode(optionButtonPath);

        SetupOptionButton((DropdownParameter)parameterType);
    }

    private void SetupOptionButton(DropdownParameter dropdownParameter)
    {
        foreach (Object value in dropdownParameter.Values)
            optionButton.AddItem(value.ToString());
    }

    public override void ConnectEdited(Action<Variant> editAction)
    {
        optionButton.Connect(OptionButton.SignalName.ItemSelected, Callable.From(editAction));
    }

    public override void SetValue(Variant value)
    {
        String valueString = (String)value;

        for (int i = 0; i < optionButton.ItemCount; i++)
        {
            String curString = optionButton.GetItemText(i);
            if (curString.Equals(valueString))
            {
                optionButton.Select(i);
                break;
            }
        }
    }

    public override Variant GetValue()
    {
        return optionButton.GetItemText(optionButton.Selected);
    }
}
