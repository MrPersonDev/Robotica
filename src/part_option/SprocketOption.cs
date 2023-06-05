using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SprocketOption : PartOption
{
    private readonly String[] TYPES = { "3P", "6P", "9P" };
    private readonly int[] THREE_TEETH = { 8, 16, 24, 32, 40 };
    private readonly int[] SIX_TEETH = { 6, 12, 18, 24, 30 };
    private readonly int[] NINE_TEETH = { 6, 12, 18, 24, 30 };
    private readonly String[] COLORS = { "Red", "Green" };

    private Dictionary<int, PartObject> threePartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> sixPartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> sixConvertedPartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> ninePartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> nineConvertedPartObjects = new Dictionary<int, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D redPlasticMaterial, greenPlasticMaterial, metalMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> threePartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> sixPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> sixConvertedPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> ninePartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> nineConvertedPartScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        SetPartObjects(threePartScenes, threePartObjects, THREE_TEETH);
        SetPartObjects(sixPartScenes, sixPartObjects, SIX_TEETH);
        SetPartObjects(sixConvertedPartScenes, sixConvertedPartObjects, SIX_TEETH);
        SetPartObjects(ninePartScenes, ninePartObjects, NINE_TEETH);
        SetPartObjects(nineConvertedPartScenes, nineConvertedPartObjects, NINE_TEETH);
    }

    private void SetPartObjects(Godot.Collections.Array<PackedScene> partObjectScenes,
                                Dictionary<int, PartObject> partObjectsDict, int[] teeth)
    {
        for (int i = 0; i < partObjectScenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(partObjectScenes[i]);
            partObjectsDict[teeth[i]] = curPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        int teeth = (int)parameters["Teeth"];
        bool converted = parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"];

        if (type == "3P")
            return threePartObjects[teeth];
        else if (type == "6P")
            return converted ? sixConvertedPartObjects[teeth] : sixPartObjects[teeth];
        else // if (type == "9P")
            return converted ? nineConvertedPartObjects[teeth] : ninePartObjects[teeth];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        String color = (String)parameters["Color"];
        bool converted = parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"];

        if (color == "Green")
            part.SetMaterial(greenPlasticMaterial);
        else if (color == "Red")
            part.SetMaterial(redPlasticMaterial);

        if (converted)
            part.SetAdditionalMeshMaterial(metalMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> teeth = new List<int>(THREE_TEETH).Cast<Object>().ToList();
        List<Object> colors = new List<String>(COLORS).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));
        parameterTypes.Add(Tuple.Create("Teeth", (ParameterType)new DropdownParameter(teeth)));
        parameterTypes.Add(Tuple.Create("Color", (ParameterType)new DropdownParameter(colors)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> newParameterTypes = new List<Tuple<String, ParameterType>>();

        String type = (String)curParameters["Type"];
        int[] teethArray;
        if (type == "3P")
            teethArray = THREE_TEETH;
        else if (type == "6P")
            teethArray = SIX_TEETH;
        else // if (type == "9P")
            teethArray = NINE_TEETH;

        List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> teeth = new List<int>(teethArray).Cast<Object>().ToList();
        List<Object> colors = new List<String>(COLORS).Cast<Object>().ToList();

        newParameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));
        newParameterTypes.Add(Tuple.Create("Teeth", (ParameterType)new DropdownParameter(teeth)));

        if (type != "3P")
            newParameterTypes.Add(Tuple.Create("High Strength Converter", (ParameterType)new BooleanParameter(true)));

        newParameterTypes.Add(Tuple.Create("Color", (ParameterType)new DropdownParameter(colors)));

        return newParameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in threePartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in sixPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in sixConvertedPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in ninePartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in nineConvertedPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Sprocket";
    }
}
