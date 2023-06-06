using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SpacerOption : PartOption
{
    private readonly float[] PLASTIC_SIZES = { 0.181f, 0.315f };
    private readonly float[] NYLON_SIZES = { 0.125f, 0.25f, 0.375f, 0.5f };
    private readonly float[] HS_SIZES = { 0.063f, 0.125f, 0.25f, 0.5f };
    private readonly String[] TYPES = { "Black Nylon", "White Nylon", "Plastic", "High Strength" };

    private Dictionary<float, PartObject> plasticPartObjects = new Dictionary<float, PartObject>();
    private Dictionary<float, PartObject> blackNylonPartObjects = new Dictionary<float, PartObject>();
    private Dictionary<float, PartObject> whiteNylonPartObjects = new Dictionary<float, PartObject>();
    private Dictionary<float, PartObject> hsPartObjects = new Dictionary<float, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D blackPlasticMaterial, whiteNylonMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> plasticPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> blackNylonPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> whiteNylonPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> hsPartScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        LoadPartObjectDict(plasticPartObjects, plasticPartScenes, PLASTIC_SIZES);
        LoadPartObjectDict(blackNylonPartObjects, blackNylonPartScenes, NYLON_SIZES);
        LoadPartObjectDict(whiteNylonPartObjects, whiteNylonPartScenes, NYLON_SIZES);
        LoadPartObjectDict(hsPartObjects, hsPartScenes, HS_SIZES);
    }

    private void LoadPartObjectDict(Dictionary<float, PartObject> dict, Godot.Collections.Array<PackedScene> scenes, float[] sizes)
    {
        for (int i = 0; i < scenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(scenes[i]);
            dict[sizes[i]] = curPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        float length = float.Parse((String)parameters["Length"]);

        if (type == "Black Nylon")
            return blackNylonPartObjects[length];
        else if (type == "White Nylon")
            return whiteNylonPartObjects[length];
        else if (type == "Plastic")
            return plasticPartObjects[length];
        else // if (type == "High Strength")
            return hsPartObjects[length];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        String type = (String)parameters["Type"];

        StandardMaterial3D mainMaterial;
        if (type == "White Nylon")
            mainMaterial = whiteNylonMaterial;
        else
            mainMaterial = blackPlasticMaterial;

        part.SetMaterial(mainMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> sizesList = new List<float>(NYLON_SIZES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));
        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new DropdownParameter(sizesList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> newParameterTypes = new List<Tuple<string, ParameterType>>();

        String type = (String)curParameters["Type"];
        float[] sizes;
        if (type == "Black Nylon" || type == "White Nylon")
            sizes = NYLON_SIZES;
        else if (type == "Plastic")
            sizes = PLASTIC_SIZES;
        else // if (type == "High Strength")
            sizes = HS_SIZES;

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> sizesList = new List<float>(sizes).Cast<Object>().ToList();

        newParameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));
        newParameterTypes.Add(Tuple.Create("Length", (ParameterType)new DropdownParameter(sizesList)));

        return newParameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in blackNylonPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in whiteNylonPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in plasticPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in hsPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Spacer";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        return $"{(float)parameters["Length"]:0.####}-length {parameters["Type"]} Spacer";
    }
}