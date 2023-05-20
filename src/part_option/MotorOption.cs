using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MotorOption : PartOption
{
	private readonly String[] CARTRIDGES = {"Green (200 RPM)", "Blue (600 RPM)", "Red (100 RPM)"};

	private PartObject defaultPartObject;
	private Dictionary<String, StandardMaterial3D> cartridgeMaterials = new Dictionary<String, StandardMaterial3D>();

	[ExportGroup("Properties")]
	[Export]
	private StandardMaterial3D greenCartridgeMaterial, blueCartridgeMaterial, redCartridgeMaterial;
	[Export]
	private StandardMaterial3D blackPlasticMaterial;

	[ExportGroup("Part Objects")]
	[Export]
	private PackedScene defaultPartScene;

    public override void _Ready()
    {
        base._Ready();

		SetPartObjects();
		SetCartridgeMaterials();
    }

	private void SetPartObjects()
	{
		defaultPartObject = new PartObject(defaultPartScene);
	}

	private void SetCartridgeMaterials()
	{
		cartridgeMaterials[CARTRIDGES[0]] = greenCartridgeMaterial;
		cartridgeMaterials[CARTRIDGES[1]] = blueCartridgeMaterial;
		cartridgeMaterials[CARTRIDGES[2]] = redCartridgeMaterial;
	}

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
		return defaultPartObject;
    }

	public override void Setup(Part part, Dictionary<String, Variant> parameters)
	{
		base.Setup(part, parameters);

		String cartridge = (String)parameters["Cartridge"];
		
		part.SetAdditionalMeshMaterial(cartridgeMaterials[cartridge]);
	}

	public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> cartridges = new List<String>(CARTRIDGES).Cast<Object>().ToList();

		parameterTypes.Add(Tuple.Create("Cartridge", (ParameterType)new DropdownParameter(cartridges)));

		return parameterTypes;
	}

	public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
	{
		return GetDefaultParameterTypes();
	}

	public override List<PartObject> GetPartObjects()
	{
		List<PartObject> partObjectsList = new List<PartObject>();

		partObjectsList.Add(defaultPartObject);

		return partObjectsList;
	}

	public override String GetName()
	{
		return "Motor";
	}
}