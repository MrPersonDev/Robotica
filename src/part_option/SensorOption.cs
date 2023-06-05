using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SensorOption : PartOption
{
    private readonly String[] TYPES = {
        "Bumper Switch",
        "Distance Sensor", "GPS Sensor",
        "Inertial Sensor", "Light Sensor",
        "Limit Switch", "Line Tracker",
        "Optical Sensor", "Potentiometer",
        "Rotation Sensor", "Shaft Encoder",
        "Ultrasonic Range Finder", "Vision Sensor"
    };

    private PartObject bumperSwitchPartObject, distanceSensorPartObject;
    private PartObject GPSSensorPartObject, InertialSensorPartObject;
    private PartObject LightSensorPartObject, limitSwitchPartObject;
    private PartObject lineTrackerPartObject, opticalSensorPartObject;
    private PartObject potentiometerPartObject, rotationSensorPartObject;
    private PartObject convertedRotationSensorPartObject, shaftEncoderPartObject;
    private PartObject ultrasonicRangeFinderPartObject, visionSensorPartObject;

    [ExportGroup("Properties")]
    [Export]
    private StandardMaterial3D metalMaterial;

    [ExportGroup("Part Objects")]
    [Export]
    private PackedScene bumperSwitchPartScene, distanceSensorPartScene;
    [Export]
    private PackedScene GPSSensorPartScene, InertialSensorPartScene;
    [Export]
    private PackedScene LightSensorPartScene, limitSwitchPartScene;
    [Export]
    private PackedScene lineTrackerPartScene, opticalSensorPartScene;
    [Export]
    private PackedScene potentiometerPartScene, rotationSensorPartScene;
    [Export]
    private PackedScene convertedRotationSensorPartScene, shaftEncoderPartScene;
    [Export]
    private PackedScene ultrasonicRangeFinderPartScene, visionSensorPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
        bumperSwitchPartObject = new PartObject(bumperSwitchPartScene);
        distanceSensorPartObject = new PartObject(distanceSensorPartScene);
        GPSSensorPartObject = new PartObject(GPSSensorPartScene);
        InertialSensorPartObject = new PartObject(InertialSensorPartScene);
        LightSensorPartObject = new PartObject(LightSensorPartScene);
        limitSwitchPartObject = new PartObject(limitSwitchPartScene);
        lineTrackerPartObject = new PartObject(lineTrackerPartScene);
        opticalSensorPartObject = new PartObject(opticalSensorPartScene);
        potentiometerPartObject = new PartObject(potentiometerPartScene);
        rotationSensorPartObject = new PartObject(rotationSensorPartScene);
        convertedRotationSensorPartObject = new PartObject(convertedRotationSensorPartScene);
        shaftEncoderPartObject = new PartObject(shaftEncoderPartScene);
        ultrasonicRangeFinderPartObject = new PartObject(ultrasonicRangeFinderPartScene);
        visionSensorPartObject = new PartObject(visionSensorPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        switch (type)
        {
            case "Bumper Switch":
                return bumperSwitchPartObject;
            case "Distance Sensor":
                return distanceSensorPartObject;
            case "GPS Sensor":
                return GPSSensorPartObject;
            case "Inertial Sensor":
                return InertialSensorPartObject;
            case "Light Sensor":
                return LightSensorPartObject;
            case "Limit Switch":
                return limitSwitchPartObject;
            case "Line Tracker":
                return lineTrackerPartObject;
            case "Optical Sensor":
                return opticalSensorPartObject;
            case "Potentiometer":
                return potentiometerPartObject;
            case "Rotation Sensor":
                return (bool)parameters["High Strength Converter"] ? convertedRotationSensorPartObject : rotationSensorPartObject;
            case "Shaft Encoder":
                return shaftEncoderPartObject;
            case "Ultrasonic Range Finder":
                return ultrasonicRangeFinderPartObject;
            case "Vision Sensor":
                return visionSensorPartObject;
            default:
                throw new ArgumentException("Invalid part object name: " + type);
        }
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);

        if ((String)parameters["Type"] == "Rotation Sensor" && (bool)parameters["High Strength Converter"])
            part.SetAdditionalMeshMaterial(metalMaterial);
    }

    public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
    {
        List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

        List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();

        parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));

        return parameterTypes;
    }

    public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> parameters)
    {
        List<Tuple<String, ParameterType>> parameterTypes = GetSpecificDefaultParameterTypes();

        if ((String)parameters["Type"] == "Rotation Sensor")
            parameterTypes.Add(Tuple.Create("High Strength Converter", (ParameterType)new BooleanParameter(true)));

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

        partObjects.Add(bumperSwitchPartObject);
        partObjects.Add(distanceSensorPartObject);
        partObjects.Add(GPSSensorPartObject);
        partObjects.Add(InertialSensorPartObject);
        partObjects.Add(LightSensorPartObject);
        partObjects.Add(limitSwitchPartObject);
        partObjects.Add(lineTrackerPartObject);
        partObjects.Add(opticalSensorPartObject);
        partObjects.Add(potentiometerPartObject);
        partObjects.Add(rotationSensorPartObject);
        partObjects.Add(convertedRotationSensorPartObject);
        partObjects.Add(shaftEncoderPartObject);
        partObjects.Add(ultrasonicRangeFinderPartObject);
        partObjects.Add(visionSensorPartObject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Sensor";
    }

    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        String text = (String)parameters["Type"];
        if (parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"])
            text += " with insert";
        return text;
    }
}
