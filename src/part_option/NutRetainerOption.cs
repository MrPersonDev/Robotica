using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NutRetainerOption : PartOption
{
    private readonly String[] TYPES = { "Four Post", "One Post", "One Post With Bearing" };
    private PartObject onePostNutRetainerPartObject, onePostBearingNutRetainerPartObject, fourPostNutRetainerPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D blackPlasticMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene onePostNutRetainerPartScene, onePostBearingNutRetainerPartScene, fourPostNutRetainerPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        onePostNutRetainerPartObject = new PartObject(onePostNutRetainerPartScene);
        onePostBearingNutRetainerPartObject = new PartObject(onePostBearingNutRetainerPartScene);
        fourPostNutRetainerPartObject = new PartObject(fourPostNutRetainerPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];

        if (type == "Four Post")
            return fourPostNutRetainerPartObject;
        else if (type == "One Post")
            return onePostNutRetainerPartObject;
        else // if (type == "One Post With Bearing")
            return onePostBearingNutRetainerPartObject;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        part.SetMaterial(blackPlasticMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(onePostNutRetainerPartObject);
        partObjects.Add(onePostBearingNutRetainerPartObject);
        partObjects.Add(fourPostNutRetainerPartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Nut Retainer";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        return $"{parameters["Type"]} Nut Retainer";
    }
}
