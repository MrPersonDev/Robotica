using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LinearSlideOption : PartOption
{
    private readonly String[] TYPES = { "Track", "Truck", "Rack Gear" };
    private readonly String[] TRUCK_TYPES = { "Outer", "Inner" };
    private readonly String[] COLORS = { "Red", "Green" };

    private PartObject trackPartObject, innerTruckPartObject, outerTruckPartObject, rackGearPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D metalMaterial, greenPlasticMaterial, redPlasticMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene trackPartScene, innerTruckPartScene, outerTruckPartScene, rackGearPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        trackPartObject = new PartObject(trackPartScene);
        innerTruckPartObject = new PartObject(innerTruckPartScene);
        outerTruckPartObject = new PartObject(outerTruckPartScene);
        rackGearPartObject = new PartObject(rackGearPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        if (type == "Track")
            return trackPartObject;
        else if (type == "Truck")
            return (String)parameters["Truck Type"] == "Outer" ? outerTruckPartObject : innerTruckPartObject;
        else // if (type == "Rack Gear")
            return rackGearPartObject;
    }

    public override async void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        String type = (String)parameters["Type"];
        if (type == "Track")
        {
            part.SetMaterial(metalMaterial);
            float length = (float)parameters["Length"] / 2.0f;
            await part.SetMeshCutterSize(length, MeshCutter.NO_CUT, MeshCutter.NO_CUT);
        }
        else // if (type == "Truck" || type == "Rack Gear")
        {
            String color = (String)parameters["Color"];
            if (color == "Red")
                part.SetMaterial(redPlasticMaterial);
            else // if (color == "Green")
                part.SetMaterial(greenPlasticMaterial);
        }
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = GetSpecificDefaultParameterTypes();

        String type = (String)curParameters["Type"];
        if (type == "Track")
            parameterTypes.Add(Tuple.Create("Length", (ParameterType)new FloatRangeParameter(1.0f, 24.0f, 1.0f, 24.0f)));
        else // if (type == "Truck" || type == "Rack Gear")
        {
            if (type == "Truck")
            {
                List<Object> truckTypesList = new List<String>(TRUCK_TYPES).Cast<Object>().ToList();
                parameterTypes.Add(Tuple.Create("Truck Type", (ParameterType)new DropdownParameter(truckTypesList)));
            }
            List<Object> colorsList = new List<String>(COLORS).Cast<Object>().ToList();
            parameterTypes.Add(Tuple.Create("Color", (ParameterType)new DropdownParameter(colorsList)));
        }

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjectsList = new List<PartObject>();

        partObjectsList.Add(trackPartObject);
        partObjectsList.Add(innerTruckPartObject);
        partObjectsList.Add(outerTruckPartObject);
        partObjectsList.Add(rackGearPartObject);

        return partObjectsList;
    }

    public override String GetName()
    {
        return "Linear Slide";
    }

    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        if ((String)parameters["Type"] == "Track")       
            return $"{(float)parameters["Length"]:0.####}-length Linear Slide Track";
        else if ((String)parameters["Type"] == "Truck")
            return $"{parameters["Truck Type"]} Linear Slide Truck";
        else // if ((String)parameters["Type"] == "Rack Gear")
            return "Rack Gear";
    }
}
