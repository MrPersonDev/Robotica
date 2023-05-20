using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WheelOption : PartOption
{
	private readonly String[] TYPES = {"Omni-Wheel", "Traction", "Mecanum", "Anti-Static Omni-Wheel", "Anti-Static Traction"};
	private readonly float[] omniWheelDiameters = {2.75f, 3.25f, 4.0f};
	private readonly float[] tractionWheelDiameters = {2.75f, 3.25f, 4.0f, 5.0f};
	private readonly float[] convertedTractionWheelDiameters = {2.75f, 3.25f, 4.0f};
	private readonly float[] antiStaticTractionWheelDiameters = {2.75f, 3.25f, 4.0f};

	private Dictionary<float, PartObject> omniWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> convertedOmniWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> antiStaticOmniWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> antiStaticConvertedOmniWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> tractionWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> convertedTractionWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> antiStaticTractionWheelPartObjects = new Dictionary<float, PartObject>();
	private Dictionary<float, PartObject> antiStaticConvertedTractionWheelPartObjects = new Dictionary<float, PartObject>();
	private PartObject leftMecanumWheelPartObject, rightMecanumWheelPartObject;
	private PartObject leftConvertedMecanumWheelPartObject, rightConvertedMecanumWheelPartObject;

	[ExportGroup("Properties")]
	[Export]
	private StandardMaterial3D converterMaterial;

	[ExportGroup("Part Objects")]
	[Export]
	private Godot.Collections.Array<PackedScene> omniWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> convertedOmniWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> antiStaticOmniWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> antiStaticConvertedOmniWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> tractionWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> convertedTractionWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> antiStaticTractionWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private Godot.Collections.Array<PackedScene> antiStaticConvertedTractionWheelPartScenes = new Godot.Collections.Array<PackedScene>();
	[Export]
	private PackedScene leftMecanumWheelPartScene, rightMecanumWheelPartScene;
	[Export]
	private PackedScene leftConvertedMecanumWheelPartScene, rightConvertedMecanumWheelPartScene;

    public override void _Ready()
    {
        base._Ready();

		SetPartObjects();
    }

	private void SetPartObjects()
	{
		SetPartObjects(omniWheelPartScenes, omniWheelPartObjects, omniWheelDiameters);
		SetPartObjects(convertedOmniWheelPartScenes, convertedOmniWheelPartObjects, omniWheelDiameters);
		SetPartObjects(antiStaticOmniWheelPartScenes, antiStaticOmniWheelPartObjects, omniWheelDiameters);
		SetPartObjects(antiStaticConvertedOmniWheelPartScenes, antiStaticConvertedOmniWheelPartObjects, omniWheelDiameters);
		SetPartObjects(tractionWheelPartScenes, tractionWheelPartObjects, tractionWheelDiameters);
		SetPartObjects(convertedTractionWheelPartScenes, convertedTractionWheelPartObjects, convertedTractionWheelDiameters);
		SetPartObjects(antiStaticTractionWheelPartScenes, antiStaticTractionWheelPartObjects, antiStaticTractionWheelDiameters);
		SetPartObjects(antiStaticConvertedTractionWheelPartScenes, antiStaticConvertedTractionWheelPartObjects, antiStaticTractionWheelDiameters);

		leftMecanumWheelPartObject = new PartObject(leftMecanumWheelPartScene);
		rightMecanumWheelPartObject = new PartObject(rightMecanumWheelPartScene);
		leftConvertedMecanumWheelPartObject = new PartObject(leftConvertedMecanumWheelPartScene);
		rightConvertedMecanumWheelPartObject = new PartObject(rightConvertedMecanumWheelPartScene);
	}

	private void SetPartObjects(Godot.Collections.Array<PackedScene> partObjectScenes, 
								Dictionary<float, PartObject> partObjectsDict, float[] diameters)
	{
		for (int i = 0; i < partObjectScenes.Count; i++)
		{
			PartObject curPartObject = new PartObject(partObjectScenes[i]);
			partObjectsDict[diameters[i]] = curPartObject;
		}
	}

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
		String type = (String)parameters["Type"];
		bool converted = parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"];

		if (type == "Mecanum")
		{
			bool flipped = (bool)parameters["Flipped"];
			if (flipped)
				return converted ? leftConvertedMecanumWheelPartObject : leftMecanumWheelPartObject;
			else
				return converted ? rightConvertedMecanumWheelPartObject : rightMecanumWheelPartObject;
		}

		float diameter = (float)parameters["Diameter"];
		if (type == "Omni-Wheel")
			return converted ? convertedOmniWheelPartObjects[diameter] : omniWheelPartObjects[diameter];
		else if (type == "Traction")
			return converted ? convertedTractionWheelPartObjects[diameter] : tractionWheelPartObjects[diameter];
		else if (type == "Anti-Static Omni-Wheel")
			return converted ? antiStaticConvertedOmniWheelPartObjects[diameter] : antiStaticOmniWheelPartObjects[diameter];
		else // if (type == "Anti-Static Traction)
			return converted ? antiStaticConvertedTractionWheelPartObjects[diameter] : antiStaticTractionWheelPartObjects[diameter];
    }

	public override void Setup(Part part, Dictionary<String, Variant> parameters)
	{
		base.Setup(part, parameters);

		bool converted = parameters.ContainsKey("High Strength Converter") && (bool)parameters["High Strength Converter"];
		if (converted)
			part.SetAdditionalMeshMaterial(converterMaterial);
	}

	public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
		List<Object> diameters = new List<float>(omniWheelDiameters).Cast<Object>().ToList();

		parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));
		parameterTypes.Add(Tuple.Create("Diameter", (ParameterType)new DropdownParameter(diameters)));
		parameterTypes.Add(Tuple.Create("High Strength Converter", (ParameterType)new BooleanParameter(true)));

		return parameterTypes;
	}

	public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
		parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));

		String type = (String)curParameters["Type"];
		if (type == "Mecanum")
		{
			parameterTypes.Add(Tuple.Create("Flipped", (ParameterType)new BooleanParameter(true)));
		}
		else
		{
			float[] diameters;
			
			if (type == "Omni-Wheel" || type == "Anti-Static Omni-Wheel")
				diameters = omniWheelDiameters;
			else if (type == "Traction")
				diameters = tractionWheelDiameters;
			else // if (type == "Anti-Static Traction")
				diameters = antiStaticTractionWheelDiameters;

			List<Object> diametersList = new List<float>(diameters).Cast<Object>().ToList();
			parameterTypes.Add(Tuple.Create("Diameter", (ParameterType)new DropdownParameter(diametersList)));
		}

		if (!(type == "Traction" && (float)curParameters["Diameter"] == 5.0f))
			parameterTypes.Add(Tuple.Create("High Strength Converter", (ParameterType)new BooleanParameter(true)));

		return parameterTypes;
	}

	public override List<PartObject> GetPartObjects()
	{
		List<PartObject> partObjectsList = new List<PartObject>();

		foreach (PartObject partObject in omniWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in convertedOmniWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in antiStaticOmniWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in antiStaticConvertedOmniWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in tractionWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in convertedTractionWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in antiStaticTractionWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		foreach (PartObject partObject in antiStaticConvertedTractionWheelPartObjects.Values)
			partObjectsList.Add(partObject);
		partObjectsList.Add(leftMecanumWheelPartObject);
		partObjectsList.Add(rightMecanumWheelPartObject);
		partObjectsList.Add(leftConvertedMecanumWheelPartObject);
		partObjectsList.Add(rightConvertedMecanumWheelPartObject);

		return partObjectsList;
	}

	public override String GetName()
	{
		return "Wheel";
	}
}
