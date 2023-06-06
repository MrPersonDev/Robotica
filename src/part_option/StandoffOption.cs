using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class StandoffOption : PartOption
{
    private readonly float[] STANDOFF_SIZES = { 0.25f, 0.5f, 0.75f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f, 5.0f, 6.0f };
    private readonly float[] COUPLER_SIZES = { 0.5f, 1.0f };

    private Dictionary<float, PartObject> standoffPartObjects = new Dictionary<float, PartObject>();
    private Dictionary<float, PartObject> couplerPartObjects = new Dictionary<float, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D metalMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> standoffPartScenes = new Godot.Collections.Array<PackedScene>();
    [Export]
    private Godot.Collections.Array<PackedScene> couplerPartScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        SetPartObjects(standoffPartScenes, standoffPartObjects, STANDOFF_SIZES);
        SetPartObjects(couplerPartScenes, couplerPartObjects, COUPLER_SIZES);
    }

    private void SetPartObjects(Godot.Collections.Array<PackedScene> partScenes,
                                Dictionary<float, PartObject> partObjectsDict, float[] sizes)
    {
        for (int i = 0; i < partScenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(partScenes[i]);
            partObjectsDict[sizes[i]] = curPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        float length = (float)parameters["Length"];
        bool coupler = (bool)parameters["Coupler"];

        if (coupler)
            return couplerPartObjects[length];
        else
            return standoffPartObjects[length];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> sizesList = new List<float>(STANDOFF_SIZES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Coupler", (ParameterType)new BooleanParameter(false)));
        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new DropdownParameter(sizesList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        parameterTypes.Add(Tuple.Create("Coupler", (ParameterType)new BooleanParameter(false)));

        bool coupler = (bool)curParameters["Coupler"];
        float[] sizes;
        if (coupler)
            sizes = COUPLER_SIZES;
        else
            sizes = STANDOFF_SIZES;

        List<Object> sizesList = new List<float>(sizes).Cast<Object>().ToList();
        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new DropdownParameter(sizesList)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in standoffPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in couplerPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Standoff";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        if ((bool)parameters["Coupler"])
            return $"{(float)parameters["Length"]:0.#}\" Standoff Coupler";
        return $"{(float)parameters["Length"]:0.#}\" Standoff";
    }
}
