using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FlexWheelOption : PartOption
{
    private readonly String[] SIZES = { "1.625", "2", "3", "4" };
    private readonly String[] DUROMETERS = { "30A", "45A", "60A" };

    private Dictionary<String, PartObject> nonePartObjects = new Dictionary<String, PartObject>();
    private Dictionary<String, PartObject> hsPartObjects = new Dictionary<String, PartObject>();
    private Dictionary<String, PartObject> convertedPartObjects = new Dictionary<String, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D rubber30A, rubber45A, rubber60A, insertsMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> nonePartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> hsPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> convertedPartScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        SetPartObjects(nonePartScenes, nonePartObjects);
        SetPartObjects(hsPartScenes, hsPartObjects);
        SetPartObjects(convertedPartScenes, convertedPartObjects);
    }

    private void SetPartObjects(Godot.Collections.Array<PackedScene> partObjectScenes,
                                Dictionary<String, PartObject> partObjectsDict)
    {
        for (int i = 0; i < partObjectScenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(partObjectScenes[i]);
            partObjectsDict[SIZES[i]] = curPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String size = (String)parameters["Diameter"];

        if ((bool)parameters["Adapter"])
        {
            if ((bool)parameters["High Strength Converter"])
                return convertedPartObjects[size];
            else
                return hsPartObjects[size];
        }
        else
            return nonePartObjects[size];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        if ((bool)parameters["Adapter"])
            part.SetAdditionalMeshMaterial(insertsMaterial);

        StandardMaterial3D baseMaterial;
        String durometer = (String)parameters["Durometer"];
        if (durometer == "30A")
            baseMaterial = rubber30A;
        else if (durometer == "45A")
            baseMaterial = rubber45A;
        else // if (durometer == "60A")
            baseMaterial = rubber60A;

        part.SetMaterial(baseMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> sizesList = new List<String>(SIZES).Cast<Object>().ToList();
        List<Object> durometersList = new List<String>(DUROMETERS).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Diameter", (ParameterType)new DropdownParameter(sizesList)));
        parameterTypes.Add(Tuple.Create("Durometer", (ParameterType)new DropdownParameter(durometersList)));
        parameterTypes.Add(Tuple.Create("Adapter", (ParameterType)new BooleanParameter(true)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = GetSpecificDefaultParameterTypes();
        if ((bool)curParameters["Adapter"])
            parameterTypes.Add(Tuple.Create("High Strength Converter", (ParameterType)new BooleanParameter(true)));
        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in nonePartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in hsPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in convertedPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Flex Wheel";
    }
}
