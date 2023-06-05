using Godot;
using System;

public partial class SliderSettingEdit : SettingEdit
{
    [ExportGroup("Properties")]
    [Export]
    private float min = 0.0f, max = 100.0f, step = 1.0f;

    [ExportGroup("Node paths")]
    [Export]
    private NodePath sliderPath, spinboxPath;

    private Slider slider;
    private SpinBox spinbox;

    public override void _Ready()
    {
        base._Ready();

        slider = (Slider)GetNode(sliderPath);
        spinbox = (SpinBox)GetNode(spinboxPath);

        SetupInputs();
        ConnectInputs();
    }

    private void SetupInputs()
    {
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Step = step;

        spinbox.MinValue = min;
        spinbox.MaxValue = max;
        spinbox.Step = step;
    }

    private void ConnectInputs()
    {
        Callable callable = Callable.From((Variant value) => { SetValue(value); });

        slider.Connect(Slider.SignalName.ValueChanged, callable);
        spinbox.Connect(Slider.SignalName.ValueChanged, callable);
    }

    public override void SetValue(Variant value)
    {
        slider.Value = (float)value;
        spinbox.Value = (float)value;
    }

    public override Variant GetValue()
    {
        return slider.Value;
    }
}
