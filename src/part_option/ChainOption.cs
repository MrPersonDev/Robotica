using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ChainOption : PartOption
{
	private readonly String[] TYPES = {"Link", "Flap", "Traction", "Attachment", "Conveyor"};
	private readonly String[] FLAP_TYPES = {"Short", "Medium", "Tall"};
	private PartObject linkPartObject, tractionPartObject, attachmentPartObject, conveyorPartObject;
	private PartObject flapShortPartObject, flatMediumPartObject, flapTallPartObject;

	[ExportGroup("Part Objects")]
	[Export]
	private PackedScene linkPartScene, tractionPartScene, attachmentPartScene, conveyorPartScene;
	[Export]
	private PackedScene flapShortPartScene, flapMediumPartScene, flapTallPartScene;

    public override void _Ready()
    {
        base._Ready();

		SetPartObjects();
    }

	private void SetPartObjects()
	{
		linkPartObject = new PartObject(linkPartScene);
		tractionPartObject = new PartObject(tractionPartScene);
		attachmentPartObject = new PartObject(attachmentPartScene);
		conveyorPartObject = new PartObject(conveyorPartScene);
		flapShortPartObject = new PartObject(flapShortPartScene);
		flatMediumPartObject = new PartObject(flapMediumPartScene);
		flapTallPartObject = new PartObject(flapTallPartScene);
	}

    public override PartObject GetPartObject(Dictionary<String, Variant> parameters)
    {
		String type = (String)parameters["Type"];
		if (type == "Link")
			return linkPartObject;
		else if (type == "Flap")
		{
			String flapType = (String)parameters["Flap Type"];
			if (flapType == "Short")
				return flapShortPartObject;
			else if (flapType == "Medium")
				return flatMediumPartObject;
			else // if (flapType == "Tall")
				return flapTallPartObject;
		}
		else if (type == "Traction")
			return tractionPartObject;
		else if (type == "Attachment")
			return attachmentPartObject;
		else // if (type == "Conveyor")
			return conveyorPartObject;
    }

	public override void Setup(Part part, Dictionary<String, Variant> parameters)
	{
		base.Setup(part, parameters);
		
		float zRotation = 0.0f;
		if ((bool)parameters["Flipped"])
			zRotation = (float)Math.PI;
		part.SetChainFlipped((bool)parameters["Flipped"]);
		CallDeferred("FlipChain", part, zRotation);
	}

	private void FlipChain(Part part, float zRotation)
	{
		Vector3 originalPosition = part.GetCenter();
		part.GlobalRotation = new Vector3(0.0f, 0.0f, zRotation);
		part.MoveTo(originalPosition);
	}

	public override List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes()
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
		parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));
		parameterTypes.Add(Tuple.Create("Flipped", (ParameterType)new BooleanParameter(false)));

		return parameterTypes;
	}
	
	public override List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> curParameters)
	{
		List<Tuple<String, ParameterType>> parameterTypes = new List<Tuple<String, ParameterType>>();

		List<Object> typesList = new List<String>(TYPES).Cast<Object>().ToList();
		parameterTypes.Add(Tuple.Create("Type", (ParameterType)new DropdownParameter(typesList)));

		String type = (String)curParameters["Type"];
		if (type == "Flap")
		{
			List<Object> flapTypesList = new List<String>(FLAP_TYPES).Cast<Object>().ToList();
			parameterTypes.Add(Tuple.Create("Flap Type", (ParameterType)new DropdownParameter(flapTypesList)));
		}

		parameterTypes.Add(Tuple.Create("Flipped", (ParameterType)new BooleanParameter(false)));

		return parameterTypes;
	}

	public override List<PartObject> GetPartObjects()
	{
		List<PartObject> partObjects = new List<PartObject>();
		
		partObjects.Add(linkPartObject);
		partObjects.Add(tractionPartObject);
		partObjects.Add(attachmentPartObject);
		partObjects.Add(conveyorPartObject);
		partObjects.Add(flapShortPartObject);
		partObjects.Add(flatMediumPartObject);
		partObjects.Add(flapTallPartObject);

		return partObjects;
	}

	public override String GetName()
	{
		return "Chain";
	}

    public override string GetSpecificName(Dictionary<String, Variant> parameters)
    {
		String type = (String)parameters["Type"];
		if (type == "Flap")
		{
			String name = "Chain Flap ";
			String flapType = (String)parameters["Flap Type"];
			name += flapType;
			return name;
		}
		else
			return "Chain " + type;
    }
}
