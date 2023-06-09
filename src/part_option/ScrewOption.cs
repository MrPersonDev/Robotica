using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ScrewOption : PartOption
{
    private readonly float[] SIZES = { 0.25f, 0.375f, 0.5f, 0.625f, 0.75f, 0.875f, 1.0f, 1.25f, 1.5f, 1.75f, 2.0f, 2.25f, 2.5f };

    private Dictionary<float, PartObject> partObjects = new Dictionary<float, PartObject>();
    private Dictionary<float, PartObject> coloredPartObjects = new Dictionary<float, PartObject>();

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> partScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> coloredPartScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        for (int i = 0; i < partScenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(partScenes[i]);
            partObjects[SIZES[i]] = curPartObject;
            
            PartObject curColoredPartObject = new PartObject(coloredPartScenes[i]);
            coloredPartObjects[SIZES[i]] = curColoredPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        float length = float.Parse((String)parameters["Length"]);
        
        if ((bool)parameters["Colored"])
            return coloredPartObjects[length];
        else
            return partObjects[length];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<float> lengths = new List<float>(SIZES);
        List<Object> lengthsObjectList = lengths.Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new DropdownParameter(lengthsObjectList)));
        parameterTypes.Add(Tuple.Create("Colored", (ParameterType)new BooleanParameter(false)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in partObjects.Values)
            partObjectsList.Add(partObject);
        
        foreach (PartObject partObject in coloredPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Screw";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        return $"{(float)parameters["Length"]:0.####}\" Screw";
    }
}