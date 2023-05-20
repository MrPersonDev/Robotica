using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShaftCollarOption : PartOption
{
	private readonly String[] TYPES = {"Metal", "Rubber", "Clamping"};
	private readonly String[] CLAMPING_TYPES = {"Normal", "High Strength", "Low Profile"};

	private PartObject metalShaftCollarPartObject, metalShaftCollarWithSetScrewPartObject;
	private PartObject rubberShaftCollarPartObject;
	private PartObject clampingNormalPartObject, clampingHSPartObject, clampingLowProfilePartObject;

	[ExportGroup("Properties")]
	[Export]
	private StandardMaterial3D metalMaterial, rubberMaterial, blackPlasticMaterial;

	[ExportGroup("Part Objects")]
	[Export]
	private PackedScene metalShaftCollarPartScene, metalShaftCollarWithSetScrewPartScene;
	[Export]
	private PackedScene rubberShaftCollarPartScene;
	[Export]
	private PackedScene clampingNormalPartScene, clampingHSPartScene, clampingLowProfilePartScene;

    public override void _Ready()
    {
        base._Ready();

		SetPartObjects();
    }

	private void SetPartObjects()
	{
		metalShaftCollarPartObject = new PartObject(metalShaftCollarPartScene);
		metalShaftCollarWithSetScrewPartObject = new PartObject(metalShaftCollarWithSetScrewPartScene);
		rubberShaftCollarPartObject = new PartObject(rubberShaftCollarPartScene);
		clampingNormalPartObject = new PartObject(clampingNormalPartScene);
		clampingHSPartObject = new PartObject(clampingHSPartScene);
		clampingLowProfilePartObject = new PartObject(clampingLowProfilePartScene);
	}

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
		String type = (String)parameters["Type"];
		if (type == "Metal")
			return (bool)parameters["Set Screw"] ? metalShaftCollarWithSetScrewPartObject : metalShaftCollarPartObject;
		else if (type == "Rubber")
			return rubberShaftCollarPartObject;
		
		String clampingType = (String)parameters["Clamping Type"];
		if (clampingType == "Normal")
			return clampingNormalPartObject;
		else if (clampingType == "High Strength")
			return clampingHSPartObject;
		else // if (clampingType == "Low Profile")
			return clampingLowProfilePartObject;
    }

	public override void Setup(Part part, Dictionary<String, Variant> parameters)
	{
		base.Setup(part, parameters);

		StandardMaterial3D mainMaterial;
		String type = (String)parameters["Type"];
		if (type == "Metal")
			mainMaterial = metalMaterial;
		else if (type == "Rubber")
			mainMaterial = rubberMaterial;
		else
			mainMaterial = blackPlasticMaterial;
		
		part.SetMaterial(mainMaterial);

		if (type == "Metal" && (bool)parameters["Set Screw"])
			part.SetAdditionalMeshMaterial(metalMaterial);
	}

	public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> types = new List<String>(TYPES).Cast<Object>().ToList();

		parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(types)));

		return parameterTypes;
	}

	public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
	{
		List<Tuple<String, ParameterType>> parameterTypes = GetSpecificDefaultParameterTypes();
		
		String type = (String)curParameters["Type"];
		if (type == "Metal")
			parameterTypes.Add(Tuple.Create("Set Screw", (ParameterType)new BooleanParameter(true)));
		else if (type == "Clamping")
		{
			List<Object> clampingTypes = new List<String>(CLAMPING_TYPES).Cast<Object>().ToList();
			parameterTypes.Add(Tuple.Create("Clamping Type", (ParameterType)new DropdownParameter(clampingTypes)));
		}
	
		return parameterTypes;
	}

	public override List<PartObject> GetPartObjects()
	{
		List<PartObject> partObjectsList = new List<PartObject>();

		partObjectsList.Add(metalShaftCollarPartObject);
		partObjectsList.Add(metalShaftCollarWithSetScrewPartObject);
		partObjectsList.Add(rubberShaftCollarPartObject);
		partObjectsList.Add(clampingNormalPartObject);
		partObjectsList.Add(clampingHSPartObject);
		partObjectsList.Add(clampingLowProfilePartObject);

		return partObjectsList;
	}

	public override String GetName()
	{
		return "Shaft Collar";
	}
}