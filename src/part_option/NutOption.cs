using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NutOption : PartOption
{
    private readonly String[] TYPES = { "Nylock", "Keps", "Hex", "Low Profile" };
    private PartObject hexPartObject, nylockPartObject, kepsPartObject, lowProfilePartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D metalMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene hexPartScene, nylockPartScene, kepsPartScene, lowProfilePartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        hexPartObject = new PartObject(hexPartScene);
        nylockPartObject = new PartObject(nylockPartScene);
        kepsPartObject = new PartObject(kepsPartScene);
        lowProfilePartObject = new PartObject(lowProfilePartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];

        if (type == "Hex")
            return hexPartObject;
        else if (type == "Nylock")
            return nylockPartObject;
        else if (type == "Keps")
            return kepsPartObject;
        else // if (type == "Low Profile")
            return lowProfilePartObject;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        String type = (String)parameters["Type"];
        if (type != "Nylock")
            part.SetMaterial(metalMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(hexPartObject);
        partObjects.Add(nylockPartObject);
        partObjects.Add(kepsPartObject);
        partObjects.Add(lowProfilePartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Nut";
    }
}
