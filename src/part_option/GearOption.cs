using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GearOption : PartOption
{
    private readonly String[] TYPES = { "Normal", "High Strength", "High Strength V2" };
    private readonly int[] NORMAL_TEETH = { 12, 36, 60, 84 };
    private readonly int[] HS_TEETH = { 12, 36, 60, 84 };
    private readonly int[] HS2_TEETH = { 24, 36, 48, 60, 72, 84 };
    private readonly String[] COLORS = { "Red", "Green" };

    private Dictionary<int, PartObject> normalPartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> hsPartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> hs2PartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> hsConvertedPartObjects = new Dictionary<int, PartObject>();
    private Dictionary<int, PartObject> hs2ConvertedPartObjects = new Dictionary<int, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D metalMaterial;
    [Export]
    private StandardMaterial3D redPlasticMaterial;
    [Export]
    private StandardMaterial3D greenPlasticMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> normalPartObjectScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> hsPartObjectScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> hs2PartObjectScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> hsConvertedPartObjectScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> hs2ConvertedPartObjectScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        SetPartObjects(normalPartObjectScenes, normalPartObjects, NORMAL_TEETH);
        SetPartObjects(hsPartObjectScenes, hsPartObjects, HS_TEETH);
        SetPartObjects(hs2PartObjectScenes, hs2PartObjects, HS2_TEETH);
        SetPartObjects(hsConvertedPartObjectScenes, hsConvertedPartObjects, HS_TEETH);
        SetPartObjects(hs2ConvertedPartObjectScenes, hs2ConvertedPartObjects, HS2_TEETH);
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
        int teeth = (int)parameters["Teeth"];
        bool converted = parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"];

        if ((String)parameters["Type"] == "Normal")
            return normalPartObjects[teeth];
        else if ((String)parameters["Type"] == "High Strength")
            return converted ? hsConvertedPartObjects[teeth] : hsPartObjects[teeth];
        else // if ((String)parameters["Type"] == "High Strength V2")
            return converted ? hs2ConvertedPartObjects[teeth] : hs2PartObjects[teeth];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        if (!parameters.ContainsKey("Color"))
            part.SetMaterial(metalMaterial);
        else if ((String)parameters["Color"] == "Green")
            part.SetMaterial(greenPlasticMaterial);
        else if ((String)parameters["Color"] == "Red")
            part.SetMaterial(redPlasticMaterial);

        bool converted = parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"];
        if (converted)
            part.SetAdditionalMeshMaterial(metalMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> teeth = new List<int>(NORMAL_TEETH).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));
        parameterTypes.Add(Tuple.Create("Teeth", (ParameterType)new DropdownParameter(teeth)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> newParameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();

        int[] teethArray;
        if ((String)curParameters["Type"] == "Normal")
            teethArray = NORMAL_TEETH;
        else if ((String)curParameters["Type"] == "High Strength")
            teethArray = HS_TEETH;
        else
            teethArray = HS2_TEETH;

        List<Object> teeth = new List<int>(teethArray).Cast<Object>().ToList();

        newParameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));
        newParameterTypes.Add(Tuple.Create("Teeth", (ParameterType)new DropdownParameter(teeth)));

        if ((String)curParameters["Type"] == "High Strength" || (String)curParameters["Type"] == "High Strength V2")
            newParameterTypes.Add(Tuple.Create("High Strength Converter", (ParameterType)new BooleanParameter(true)));

        if ((String)curParameters["Type"] == "Normal" || (int)curParameters["Teeth"] > 24)
        {
            List<Object> colors = new List<String>(COLORS).Cast<Object>().ToList();
            newParameterTypes.Add(Tuple.Create("Color", (ParameterType)new DropdownParameter(colors)));
        }

        return newParameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in normalPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in hsPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in hsConvertedPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in hs2PartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in hs2ConvertedPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Gear";
    }
}
