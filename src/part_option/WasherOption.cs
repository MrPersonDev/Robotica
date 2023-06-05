using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WasherOption : PartOption
{
    private readonly String[] TYPES = { "Steel", "Teflon" };

    private PartObject teflonPartObject, steelPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D teflonMaterial, steelMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene teflonPartScene, steelPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        teflonPartObject = new PartObject(teflonPartScene);
        steelPartObject = new PartObject(steelPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];

        if (type == "Steel")
            return steelPartObject;
        else // if (type == "Teflon")
            return teflonPartObject;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        String type = (String)parameters["Type"];
        StandardMaterial3D mainMaterial;
        if (type == "Steel")
            mainMaterial = steelMaterial;
        else // if (type == "Teflon")
            mainMaterial = teflonMaterial;

        part.SetMaterial(mainMaterial);
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

        partObjects.Add(teflonPartObject);
        partObjects.Add(steelPartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Washer";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        return $"{parameters["Type"]} Washer";
    }
}
