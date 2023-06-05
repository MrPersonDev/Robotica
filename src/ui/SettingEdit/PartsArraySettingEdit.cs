using Godot;
using System;
using System.Collections.Generic;

public partial class PartsArraySettingEdit : SettingEdit
{
    private List<String> partNames;

    [ExportGroup("Properties")]
    [Export]
    private PackedScene partDropdownButton;

    [ExportGroup("Node paths")]
    [Export]
    private NodePath itemsContainerPath, spinboxPath;

    private VBoxContainer itemsContainer;
    private SpinBox spinbox;

    public override void _Ready()
    {
        base._Ready();

        itemsContainer = (VBoxContainer)GetNode(itemsContainerPath);
        spinbox = (SpinBox)GetNode(spinboxPath);

        GetPartNames();
        ConnectSpinbox();
    }

    private void GetPartNames()
    {
        World world = (World)GetTree().CurrentScene;
        partNames = world.GetInterfaceNode().GetPartsListNode().GetPartsNames();
    }

    private void ConnectSpinbox()
    {
        spinbox.Connect(SpinBox.SignalName.ValueChanged, Callable.From((int value) => { SetSize(value); }));
    }

    private void SetSize(int size)
    {
        for (int i = itemsContainer.GetChildCount() - 1; i >= size; i--)
            RemoveItem(i);

        for (int i = itemsContainer.GetChildCount(); i < size; i++)
            AddItem();
    }

    private void AddItem()
    {
        Node child = partDropdownButton.Instantiate();
        itemsContainer.AddChild(child);

        OptionButton optionButton = (OptionButton)child;
        foreach (String partName in partNames)
            optionButton.AddItem(partName);
    }

    private void RemoveItem(int i)
    {
        Node child = itemsContainer.GetChild(i);
        itemsContainer.RemoveChild(child);
        child.QueueFree();
    }

    public override void SetValue(Variant value)
    {
        Godot.Collections.Array<String> array = (Godot.Collections.Array<String>)value;
        spinbox.Value = array.Count;

        for (int i = 0; i < array.Count; i++)
        {
            for (int j = 0; j < partNames.Count; j++)
            {
                if (partNames[j] == array[i])
                {
                    ((OptionButton)itemsContainer.GetChild(i)).Select(j);
                    break;
                }
            }
        }
    }

    public override Variant GetValue()
    {
        Godot.Collections.Array<String> array = new Godot.Collections.Array<String>();
        foreach (OptionButton item in itemsContainer.GetChildren())
        {
            String value = item.GetItemText(item.Selected);
            array.Add(value);
        }

        return array;
    }
}

