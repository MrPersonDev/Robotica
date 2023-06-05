using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RailOption : PartOption
{
    private readonly int[] BASE_SIZES = { 25, 35 };
    private readonly String[] MATERIALS = { "Steel", "Alluminum" };

    private PartObject rail25PartObject, rail35PartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D steelMaterial, alluminumMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene rail25PartScene, rail35PartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        rail25PartObject = new PartObject(rail25PartScene);
        rail35PartObject = new PartObject(rail35PartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        if ((int)parameters["Base Length"] == 25)
            return rail25PartObject;
        else // if ((int)parameters["Base Length"] == 35)
            return rail35PartObject;
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

        List<Object> baseLengths = new List<int>(BASE_SIZES).Cast<Object>().ToList();
        List<Object> materialsList = new List<String>(MATERIALS).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Base Length", (ParameterType)new DropdownParameter(baseLengths)));
        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new FloatRangeParameter(1.0f, 35.0f, 1.0f, 25.0f)));
        parameterTypes.Add(Tuple.Create("Material", (ParameterType)new DropdownParameter(materialsList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> baseLengths = new List<int>(BASE_SIZES).Cast<Object>().ToList();
        List<Object> materialsList = new List<String>(MATERIALS).Cast<Object>().ToList();

        float baseLength = (float)curParameters["Base Length"];

        parameterTypes.Add(Tuple.Create("Base Length", (ParameterType)new DropdownParameter(baseLengths)));
        parameterTypes.Add(Tuple.Create("Length", (ParameterType)new FloatRangeParameter(1.0f, baseLength, 1.0f, baseLength)));
        parameterTypes.Add(Tuple.Create("Material", (ParameterType)new DropdownParameter(materialsList)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(rail25PartObject);
        partObjects.Add(rail35PartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Rail";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        return $"{parameters["Material"]} {(float)parameters["Length"]:0.#}-length ({parameters["Base Length"]:0.#}-base-length) Rail";
    }
}
