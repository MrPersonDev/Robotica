using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ConnectorPinOption : PartOption
{
    private readonly String[] TYPES = { "Pin", "Mounting Flange" };
    private readonly String[] PIN_TYPES = { "Capped", "1x1" };
    private readonly String[] FLANGE_TYPES = { "Long", "Short" };

    private PartObject cappedPartObject, pinPartObject, flangeShortPartObject, flangeLongPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D defaultMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene cappedPartScene, pinPartScene, flangeShortPartScene, flangeLongPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        cappedPartObject = new PartObject(cappedPartScene);
        pinPartObject = new PartObject(pinPartScene);
        flangeShortPartObject = new PartObject(flangeShortPartScene);
        flangeLongPartObject = new PartObject(flangeLongPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        if ((String)parameters["Type"] == "Pin")
            return (String)parameters["Pin Type"] == "Capped" ? cappedPartObject : pinPartObject;
        else
            return (String)parameters["Flange Type"] == "Short" ? flangeShortPartObject : flangeLongPartObject;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        part.SetMaterial(defaultMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
        List<Object> pinTypesList = new List<String>(PIN_TYPES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));
        parameterTypes.Add(Tuple.Create("Pin Type", (ParameterType)new DropdownParameter(pinTypesList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));

        if ((String)curParameters["Type"] == "Pin")
        {
            List<Object> pinTypesList = new List<String>(PIN_TYPES).Cast<Object>().ToList();
            parameterTypes.Add(Tuple.Create("Pin Type", (ParameterType)new DropdownParameter(pinTypesList)));
        }
        else
        {
            List<Object> flangeTypesList = new List<String>(FLANGE_TYPES).Cast<Object>().ToList();
            parameterTypes.Add(Tuple.Create("Flange Type", (ParameterType)new DropdownParameter(flangeTypesList)));
        }

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(cappedPartObject);
        partObjects.Add(pinPartObject);
        partObjects.Add(flangeShortPartObject);
        partObjects.Add(flangeLongPartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Connector Pin";
    }
}
