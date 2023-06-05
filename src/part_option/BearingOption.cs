using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BearingOption : PartOption
{
    private readonly String[] TYPES = { "Flat", "Pillow Block", "High Strength Flat", "High Strength Pillow Block", "Lock Bar", "Low Profile", "Shaft Collar Retainer" };
    private PartObject pillowBlockBearingPartObject, flatBearingPartObject, lockBarPartObject, metalLockBarPartObject, lowProfileBearingPartObject;
    private PartObject shaftCollarRetainerPartObject, highStrengthFlatBearingPartObject, highStrengthPillowBlockBearingPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D blackPlasticMaterial, metalMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene pillowBlockBearingPartScene, flatBearingPartScene, lockBarPartScene, metalLockBarPartScene, lowProfileBearingPartScene;
    [Export]
    private PackedScene shaftCollarRetainerPartScene, highStrengthFlatBearingPartScene, highStrengthPillowBlockBearingPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        pillowBlockBearingPartObject = new PartObject(pillowBlockBearingPartScene);
        flatBearingPartObject = new PartObject(flatBearingPartScene);
        lockBarPartObject = new PartObject(lockBarPartScene);
        metalLockBarPartObject = new PartObject(metalLockBarPartScene);
        lowProfileBearingPartObject = new PartObject(lowProfileBearingPartScene);
        shaftCollarRetainerPartObject = new PartObject(shaftCollarRetainerPartScene);
        highStrengthFlatBearingPartObject = new PartObject(highStrengthFlatBearingPartScene);
        highStrengthPillowBlockBearingPartObject = new PartObject(highStrengthPillowBlockBearingPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];

        if (type == "Flat")
            return flatBearingPartObject;
        else if (type == "Pillow Block")
            return pillowBlockBearingPartObject;
        else if (type == "Lock Bar")
            return (bool)parameters["Metal"] ? metalLockBarPartObject : lockBarPartObject;
        else if (type == "Low Profile")
            return lowProfileBearingPartObject;
        else if (type == "Shaft Collar Retainer")
            return shaftCollarRetainerPartObject;
        else if (type == "High Strength Flat")
            return highStrengthFlatBearingPartObject;
        else // if (type == "High Strength Pillow Block")
            return highStrengthPillowBlockBearingPartObject;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        if (parameters.ContainsKey("Metal") && (bool)parameters["Metal"])
            part.SetMaterial(metalMaterial);
        else
            part.SetMaterial(blackPlasticMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = GetSpecificDefaultParameterTypes();

        if ((String)curParameters["Type"] == "Lock Bar")
            parameterTypes.Add(Tuple.Create("Metal", (ParameterType)new BooleanParameter(false)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(flatBearingPartObject);
        partObjects.Add(pillowBlockBearingPartObject);
        partObjects.Add(lockBarPartObject);
        partObjects.Add(metalLockBarPartObject);
        partObjects.Add(lowProfileBearingPartObject);
        partObjects.Add(shaftCollarRetainerPartObject);
        partObjects.Add(highStrengthFlatBearingPartObject);
        partObjects.Add(highStrengthPillowBlockBearingPartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Bearing";
    }
}
