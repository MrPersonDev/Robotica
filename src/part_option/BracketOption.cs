using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BracketOption : PartOption
{
    private readonly String[] TYPES = { "Bevel", "Rack", "Worm" };
    private PartObject bevelPartObject, rackPartObject, wormPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D steelMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene bevelPartScene, rackPartScene, wormPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        bevelPartObject = new PartObject(bevelPartScene);
        rackPartObject = new PartObject(rackPartScene);
        wormPartObject = new PartObject(wormPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        if (type == "Bevel")
            return bevelPartObject;
        else if (type == "Rack")
            return rackPartObject;
        else // if (type == "Worm")
            return wormPartObject;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        part.SetMaterial(steelMaterial);
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

        partObjects.Add(bevelPartObject);
        partObjects.Add(rackPartObject);
        partObjects.Add(wormPartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Bracket";
    }
}
