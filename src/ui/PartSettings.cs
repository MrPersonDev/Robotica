using Godot;
using System;
using System.Collections.Generic;

public partial class PartSettings : PanelContainer
{
    private List<Tuple<String, ParameterType>> curParameterTypes;
    private PartOption currentPartOption;
    private Dictionary<PartOption, Dictionary<String, Variant>> prevParameters = new Dictionary<PartOption, Dictionary<String, Variant>>();

    [ExportGroup("Parameter Objects")]
    [Export]
    private PackedScene numericParameterObject, dropdownParameterObject, booleanParameterObject;

    [ExportGroup("Node Paths")]
    [Export]
    private NodePath vBoxPath;

    private VBoxContainer vBox;

    public override void _Ready()
    {
        vBox = (VBoxContainer)GetNode(vBoxPath);
    }

    public void ShowSettings(PartOption partOption)
    {
        Show();
        ClearChildren();

        currentPartOption = partOption;
        List<Tuple<String, ParameterType>> parameterTypes = partOption.GetDefaultParameterTypes();
        this.curParameterTypes = parameterTypes;

        foreach (Tuple<String, ParameterType> parameterType in parameterTypes)
            AddParameter(parameterType.Item1, parameterType.Item2);

        if (prevParameters.ContainsKey(partOption))
            RestoreParameters(prevParameters[partOption], parameterTypes);

        ParameterInputEdited();
    }

    private void RestoreParameters(Dictionary<String, Variant> parameters, List<Tuple<String, ParameterType>> parameterTypes)
    {
        for (int i = 0; i < parameterTypes.Count; i++)
        {
            ParameterInput parameterInput = GetParameter(i);
            String name = parameterInput.GetName();
            Variant value = parameters[name];

            parameterInput.SetValue(value);
        }
    }

    private void UpdateSettings()
    {
        bool updatedParameter = true;
        while (updatedParameter)
        {
            updatedParameter = false;

            Dictionary<String, Variant> curParameters = GetParameters();
            List<Tuple<String, ParameterType>> newParameterTypes = currentPartOption.GetSpecificParameterTypes(curParameters);

            for (int i = 0; i < curParameterTypes.Count && i < newParameterTypes.Count; i++)
            {
                if (curParameterTypes[i].Equals(newParameterTypes[i]))
                    continue;

                RemoveParameter(i);
                ParameterInput parameterInput = AddParameter(newParameterTypes[i].Item1, newParameterTypes[i].Item2);
                vBox.MoveChild(parameterInput, i);
                updatedParameter = true;
            }

            for (int i = newParameterTypes.Count; i < curParameterTypes.Count; i++)
            {
                RemoveParameter(i);
                updatedParameter = true;
            }

            for (int i = curParameterTypes.Count; i < newParameterTypes.Count; i++)
            {
                AddParameter(newParameterTypes[i].Item1, newParameterTypes[i].Item2);
                updatedParameter = true;
            }

            curParameterTypes = newParameterTypes;
        }
    }

    private ParameterInput GetParameter(int index)
    {
        return (ParameterInput)vBox.GetChild(index);
    }

    private void ClearChildren()
    {
        foreach (Node child in vBox.GetChildren())
        {
            vBox.RemoveChild(child);
            child.QueueFree();
        }
    }

    private ParameterInput AddParameter(String label, ParameterType parameterType)
    {
        PackedScene parameterInputObject = null;
        if (parameterType is FloatRangeParameter)
            parameterInputObject = numericParameterObject;
        else if (parameterType is DropdownParameter)
            parameterInputObject = dropdownParameterObject;
        else if (parameterType is BooleanParameter)
            parameterInputObject = booleanParameterObject;

        ParameterInput newParameterInput = (ParameterInput)parameterInputObject.Instantiate();

        newParameterInput.Setup(parameterType);

        Action<Variant> editAction = (Variant value) => { ParameterInputEdited(); };
        newParameterInput.ConnectEdited(editAction);
        newParameterInput.SetLabel(label);

        vBox.AddChild(newParameterInput);

        return newParameterInput;
    }

    private void RemoveParameter(int index)
    {
        Node child = vBox.GetChildren()[index];
        vBox.RemoveChild(child);
        child.QueueFree();
    }

    private void ParameterInputEdited()
    {
        UpdateSettings();
        Dictionary<String, Variant> parameters = GetParameters();

        prevParameters[currentPartOption] = parameters;

        Interface ui = (Interface)GetParent();
        ui.CreatePart(currentPartOption, parameters);
    }

    public Dictionary<String, Variant> GetParameters()
    {
        Dictionary<String, Variant> parameters = new Dictionary<String, Variant>();

        foreach (Node child in vBox.GetChildren())
        {
            if (!(child is ParameterInput))
                continue;
            ParameterInput parameterInput = (ParameterInput)child;

            parameters.Add(parameterInput.GetName(), parameterInput.GetValue());
        }

        return parameters;
    }
}
