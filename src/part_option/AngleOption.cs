using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AngleOption : PartOption
{
    private readonly int[] WIDTHS = { 2, 3 };
    private readonly String[] MATERIALS = { "Steel", "Alluminum" };

    private Dictionary<int, PartObject> partObjects = new Dictionary<int, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D steelMaterial, alluminumMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> partScenes = new Godot.Collections.Array<PackedScene>();

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        SetPartObjects(partScenes, partObjects, WIDTHS);
    }

    private void SetPartObjects(Godot.Collections.Array<PackedScene> partObjectScenes,
                                Dictionary<int, PartObject> partObjectsDict, int[] widths)
    {
        for (int i = 0; i < partObjectScenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(partObjectScenes[i]);
            partObjectsDict[widths[i]] = curPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        int width = (int)parameters["Width"];

        return partObjects[width];
    }

    public override async void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        float length = (float)parameters["Length"] / 2.0f;

        StandardMaterial3D mainMaterial;
        if ((String)parameters["Material"] == "Steel")
            mainMaterial = steelMaterial;
        else // if (String)parameters["Material"] == "Alluminum"
            mainMaterial = alluminumMaterial;

        await part.SetMeshCutterSize(length, MeshCutter.NO_CUT, MeshCutter.NO_CUT);
        part.SetMaterial(mainMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();
        List<Object> widthsList = new List<int>(WIDTHS).Cast<Object>().ToList();
        List<Object> materialsList = new List<String>(MATERIALS).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Width", (ParameterType)new DropdownParameter(widthsList)));
        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new FloatRangeParameter(1.0f, 35.0f, 1.0f, 15.0f)));
        parameterTypes.Add(Tuple.Create("Material", (ParameterType)new DropdownParameter(materialsList)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in partObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Angle";
    }
}
