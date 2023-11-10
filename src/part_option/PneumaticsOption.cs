using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PneumaticsOption : PartOption
{
    private readonly String[] TYPES = {
		"Air Tank", "Valve Stem",
		"Regulator", "Regulator Bracket",
		"Fitting", "Piston", 
		"Shut Off Valve", "Solenoid"
    };
	
	private readonly String[] FITTING_TYPES = {
		"Straight Male Fitting", "Elbow Fitting",
		"Air Flow Valve Fitting", "Tee Fitting"
	};
	
	private readonly String[] PISTON_TYPES = {
		"25mm", "50mm", "75mm"
	};

	private PartObject airTankPartObject, valveStemPartObject;
	private PartObject regulatorPartObject, regulatorBracketPartObject;
	private PartObject straightMaleFittingPartObject, elbowFittingPartObject;
	private PartObject airFlowValveFittingPartObject, teeFittingPartObject;
	private PartObject piston25mmPartObject, piston50mmPartObject, piston75mmPartObject;
	private PartObject shutOffValvePartObject, solenoidPartobject;

    [ExportGroup("Part Objects")]
	[Export]
	private PackedScene airTankPartScene, valveStemPartScene;
	[Export]
	private PackedScene regulatorPartScene, regulatorBracketPartScene;
	[Export]
	private PackedScene straightMaleFittingPartScene, elbowFittingPartScene;
	[Export]
	private PackedScene airFlowValveFittingPartScene, teeFittingPartScene;
	[Export]
	private PackedScene piston25mmPartScene, piston50mmPartScene, piston75mmPartScene;
	[Export]
	private PackedScene shutOffValvePartScene, solenoidPartScene;

    public override void _Ready()
    {
        base._Ready();

        SetPartObjects();
    }

    private void SetPartObjects()
    {
		airTankPartObject = new PartObject(airTankPartScene);
		valveStemPartObject = new PartObject(valveStemPartScene);
		regulatorPartObject = new PartObject(regulatorPartScene);
		regulatorBracketPartObject = new PartObject(regulatorBracketPartScene);
		straightMaleFittingPartObject = new PartObject(straightMaleFittingPartScene);
		elbowFittingPartObject = new PartObject(elbowFittingPartScene);
		airFlowValveFittingPartObject = new PartObject(airFlowValveFittingPartScene);
		teeFittingPartObject = new PartObject(teeFittingPartScene);
		piston25mmPartObject = new PartObject(piston25mmPartScene);
		piston50mmPartObject = new PartObject(piston50mmPartScene);
		piston75mmPartObject = new PartObject(piston75mmPartScene);
		shutOffValvePartObject = new PartObject(shutOffValvePartScene);
		solenoidPartobject = new PartObject(solenoidPartScene);
    }

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
        String type = (String)parameters["Type"];
        switch (type)
        {
			case "Air Tank":
				return airTankPartObject;
			case "Valve Stem":
				return valveStemPartObject;
			case "Regulator":
				return regulatorPartObject;
			case "Regulator Bracket":
				return regulatorBracketPartObject;
			case "Fitting":
				String fittingType = (String)parameters["Fitting Type"];
				switch (fittingType)
				{
					case "Straight Male Fitting":
						return straightMaleFittingPartObject;
					case "Elbow Fitting":
						return elbowFittingPartObject;
					case "Air Flow Valve Fitting":
						return airFlowValveFittingPartObject;
					case "Tee Fitting":
						return teeFittingPartObject;
				}
				break;
			case "Piston":
				String pistonType = (String)parameters["Piston Type"];
				switch (pistonType)
				{
					case "25mm":
						return piston25mmPartObject;
					case "50mm":
						return piston50mmPartObject;
					case "75mm":
						return piston75mmPartObject;
				}
				break;
			case "Shut Off Valve":
				return shutOffValvePartObject;
			case "Solenoid":
				return solenoidPartobject;
			default:
				throw new Exception("Unknown part type");
		}
		
		return null;
    }

    public override void Setup(Part part, Dictionary<String, Variant> parameters)
    {
        base.Setup(part, parameters);
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

        if ((String)parameters["Type"] == "Fitting")
		{
			List<Object> fittingTypesList = new List<String>(FITTING_TYPES).Cast<Object>().ToList();
            parameterTypes.Add(Tuple.Create("Fitting Type", (ParameterType)new DropdownParameter(fittingTypesList)));
		}

        if ((String)parameters["Type"] == "Piston")
		{
			List<Object> pistonTypesList = new List<String>(PISTON_TYPES).Cast<Object>().ToList();
            parameterTypes.Add(Tuple.Create("Piston Type", (ParameterType)new DropdownParameter(pistonTypesList)));
		}

        return parameterTypes;
    }

    public override List<PartObject> GetPartObjects()
    {
        List<PartObject> partObjects = new List<PartObject>();

		partObjects.Add(airTankPartObject);
		partObjects.Add(valveStemPartObject);
		partObjects.Add(regulatorPartObject);
		partObjects.Add(regulatorBracketPartObject);
		partObjects.Add(straightMaleFittingPartObject);
		partObjects.Add(elbowFittingPartObject);
		partObjects.Add(airFlowValveFittingPartObject);
		partObjects.Add(teeFittingPartObject);
		partObjects.Add(piston25mmPartObject);
		partObjects.Add(piston50mmPartObject);
		partObjects.Add(piston75mmPartObject);
		partObjects.Add(shutOffValvePartObject);
		partObjects.Add(solenoidPartobject);

        return partObjects;
    }

    public override String GetName()
    {
        return "Pneumatics";
    }

    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
        String text = "Pneumatic " +(String)parameters["Type"];
		String type = (String)parameters["Type"];
		if (type != "Fitting")
			text += type;
		if (type == "Fitting")
			text += (String)parameters["Fitting Type"];
		if (type == "Piston")
			text += (String)parameters["Piston Type"];
        return text;
    }
}
