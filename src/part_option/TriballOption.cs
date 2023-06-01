using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TriballOption : PartOption
{
	private readonly String[] COLORS = {"Neutral", "Red", "Blue"};
	private PartObject defaultPartObject;

	[ExportGroup("Properties")]
	[Export]
	private StandardMaterial3D greenPlasticMaterial, redPlasticMaterial, bluePlasticMaterial;

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

		String color = (String)parameters["Alliance"];
		if (color == "Neutral")
			part.SetMaterial(greenPlasticMaterial);
		else if (color == "Red")
			part.SetMaterial(redPlasticMaterial);
		else // if (color == "Blue")
			part.SetMaterial(bluePlasticMaterial);
	}

	public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> colorList = new List<String>(COLORS).Cast<Object>().ToList();
		parameterTypes.Add(Tuple.Create("Alliance", (ParameterType)new DropdownParameter(colorList)));

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
		return "Triball";
	}
}
