using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BatteryClipOption : PartOption
{
	private PartObject defaultPartObject;

	[ExportGroup("Properties")]
	[Export]
	private StandardMaterial3D blackPlasticMaterial;

	[ExportGroup("Part Objects")]
	[Export]
	private PackedScene defaultPartScene;

    public override void _Ready()
    {
        base._Ready();

		SetPartObjects();
    }

	private void SetPartObjects()
	{
		defaultPartObject = new PartObject(defaultPartScene);
	}

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
		return defaultPartObject;
    }

	public override void Setup(Part part, Dictionary<String, Variant> parameters)
	{
		base.Setup(part, parameters);

		part.SetMaterial(blackPlasticMaterial);
	}

	public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		return parameterTypes;
	}

	public override List<PartObject> GetPartObjects()
	{
		List<PartObject> partObjects = new List<PartObject>();

		partObjects.Add(defaultPartObject);

		return partObjects;
	}

	public override String GetName()
	{
		return "Battery Clip";
	}
}
