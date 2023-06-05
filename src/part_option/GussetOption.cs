using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GussetOption : PartOption
{
    private readonly String[] TYPES = { "Bent", "Flat", "Angle", "Coupler" };
    private readonly String[] BENT_DEGREES = { "30", "45", "60", "90" };
    private readonly String[] FLAT_DEGREES = { "30", "45", "60", "90" };
    private readonly String[] ANGLE_TYPES = { "45 Degree", "90 Degree Angle", "90 Degree", "Angle", "Angle Corner", "Pivot", "Plus" };
    private readonly String[] COUPLER_TYPES = { "C-Channel", "Angle" };

    private Dictionary<String, PartObject> bentPartObjects = new Dictionary<String, PartObject>();
    private Dictionary<String, PartObject> flatPartObjects = new Dictionary<String, PartObject>();
    private Dictionary<String, PartObject> anglePartObjects = new Dictionary<String, PartObject>();
    private Dictionary<String, PartObject> couplerPartObjects = new Dictionary<String, PartObject>();

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D alluminumMaterial, steelMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private Godot.Collections.Array<PackedScene> bentPartScenes, flatPartScenes, anglePartScenes, couplerPartScenes;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        SetPartObjects(bentPartScenes, bentPartObjects, BENT_DEGREES);
        SetPartObjects(flatPartScenes, flatPartObjects, FLAT_DEGREES);
        SetPartObjects(anglePartScenes, anglePartObjects, ANGLE_TYPES);
        SetPartObjects(couplerPartScenes, couplerPartObjects, COUPLER_TYPES);
    }

    private void SetPartObjects(Godot.Collections.Array<PackedScene> partObjectScenes,
                                Dictionary<String, PartObject> partObjectsDict, String[] subTypes)
    {
        for (int i = 0; i < partObjectScenes.Count; i++)
        {
            PartObject curPartObject = new PartObject(partObjectScenes[i]);
            partObjectsDict[subTypes[i]] = curPartObject;
        }
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        if (type == "Bent")
            return bentPartObjects[(String)parameters["Angle"]];
        else if (type == "Flat")
            return flatPartObjects[(String)parameters["Angle"]];
        else if (type == "Angle")
            return anglePartObjects[(String)parameters["Subtype"]];
        else // if (type == "Coupler")
            return couplerPartObjects[(String)parameters["Subtype"]];
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        part.SetMaterial(steelMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typeList = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> subtypeList = new List<String>(BENT_DEGREES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typeList)));
        parameterTypes.Add(Tuple.Create("Angle", (ParameterType)new DropdownParameter(subtypeList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typeList = new List<String>(TYPES).Cast<Object>().ToList();
        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typeList)));

        String curType = (String)curParameters["Type"];
        if (curType == "Bent" || curType == "Flat")
        {
            List<Object> subtypeList;
            if (curType == "Bent")
                subtypeList = new List<String>(BENT_DEGREES).Cast<Object>().ToList();
            else // if (curType == "Flat")
                subtypeList = new List<String>(FLAT_DEGREES).Cast<Object>().ToList();

            parameterTypes.Add(Tuple.Create("Angle", (ParameterType)new DropdownParameter(subtypeList)));
        }
        else
        {
            List<Object> subtypeList;
            if (curType == "Angle")
                subtypeList = new List<String>(ANGLE_TYPES).Cast<Object>().ToList();
            else // if (curType == "Coupler")
                subtypeList = new List<String>(COUPLER_TYPES).Cast<Object>().ToList();

            parameterTypes.Add(Tuple.Create("Subtype", (ParameterType)new DropdownParameter(subtypeList)));
        }

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        foreach (PartObject partObject in bentPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in flatPartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in anglePartObjects.Values)
            partObjectsList.Add(partObject);
        foreach (PartObject partObject in couplerPartObjects.Values)
            partObjectsList.Add(partObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Gusset";
    }
    
    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        String text = (String)parameters["Material"];
        
        if ((String)parameters["Type"] == "Bent")
            text += $"{parameters["Angle"]} Bent ";
        else if ((String)parameters["Type"] == "Flat")
            text += $"{parameters["Angle"]} Flat ";
        else if ((String)parameters["Type"] == "Angle" || (String)parameters["Type"] == "Coupler")
            text += $"{parameters["Subtype"]} ";
        
        if ((String)parameters["Type"] == "Coupler")
            text += "coupler";
        else
            text += "gusset";
        
        return text;
    }
}
