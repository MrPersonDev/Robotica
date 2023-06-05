using Godot;
using System;
using System.Collections.Generic;

public partial class ShaftOption : PartOption
{
    private PartObject shaftObject, highStrengthShaftObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D defaultMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene shaftScene, highStrengthShaftScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        shaftObject = new PartObject(shaftScene);
        highStrengthShaftObject = new PartObject(highStrengthShaftScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        if ((bool)parameters["High Strength"])
            return highStrengthShaftObject;
        else
            return shaftObject;
    }

    public override async void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        float length = (float)parameters["Length"];

        await part.SetMeshCutterSize(length, MeshCutter.NO_CUT, MeshCutter.NO_CUT);
        part.SetInsertSize(length);
        part.SetMaterial(defaultMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new FloatRangeParameter(0.1f, 12.0f, 0.1f, 6.0f)));
        parameterTypes.Add(Tuple.Create("High Strength", (ParameterType)new BooleanParameter(false)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(shaftObject);
        partObjects.Add(highStrengthShaftObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Shaft";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        String text = "";
        if ((bool)parameters["High Strength"])   
            text += "High Strength ";
        text += $"{(float)parameters["Length"]:0.#}-length Shaft";
        return text;
    }
}
