using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UChannelOption : PartOption
{
	private readonly String[] MATERIALS = {"Steel", "Alluminum"};

	private PartObject defaultPartObject;

	[ExportGroup("Properties")]
	[Export]
	private StandardMaterial3D steelMaterial, alluminumMaterial;

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

	public override async void Setup(Part part, Dictionary<String, Variant> parameters)
	{
		base.Setup(part, parameters);

		float length = (float)parameters["Length"]/2.0f;

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

		List<Object> materialsList = new List<String>(MATERIALS).Cast<Object>().ToList();

		parameterTypes.Add(Tuple.Create("Length", (ParameterType)new FloatRangeParameter(1.0f, 20.0f, 1.0f, 15.0f)));
		parameterTypes.Add(Tuple.Create("Material", (ParameterType)new DropdownParameter(materialsList)));

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
		return "U-Channel";
	}
}
