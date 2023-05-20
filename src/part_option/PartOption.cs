using Godot;
using System;
using System.Collections.Generic;

public partial class PartOption : Button
{
	private Parts parts;
	private Dictionary<PartObject, List<Hole>> holesDict = new Dictionary<PartObject, List<Hole>>();
	private List<Tuple<String, ParameterType>> prevParameterTypes = null;

	public override void _Ready()
	{
		parts = ((World)GetTree().CurrentScene).GetPartsNode();

		SetText(GetName());
	}

	public void SetText(String text)
	{
		Text = text;
	}

	public void StoreHoles(PartObject partObject, List<Hole> holeArray)
	{
		holesDict[partObject] = holeArray;
	}

	public List<Hole> GetHoles(PartObject partObject)
	{
		if (!holesDict.ContainsKey(partObject))
			return new List<Hole>();
		return holesDict[partObject];
	}

	public Parts GetPartsNode()
	{
		return parts;
	}

	public virtual void Setup(Part part, Dictionary<String, Variant> parameters) 
	{ 
		part.StorePartOption(this);
		part.StoreParameters(parameters);
	}

	public virtual List<Tuple<String, ParameterType>> GetDefaultParameterTypes()
	{
		if (prevParameterTypes != null)
			return prevParameterTypes;
		
		return GetSpecificDefaultParameterTypes();
	}

	public virtual List<Tuple<String, ParameterType>> GetParameterTypes(Dictionary<String, Variant> parameters)
	{
		List<Tuple<String, ParameterType>> parameterTypes = GetDefaultParameterTypes();
		prevParameterTypes = parameterTypes;

		return parameterTypes;
	}

	public virtual List<Tuple<String, ParameterType>> GetSpecificParameterTypes(Dictionary<String, Variant> parameters)
	{
		return GetDefaultParameterTypes();
	}

	public virtual PartObject GetPartObject(Dictionary<String, Variant> parameters) { throw new NotImplementedException(); }
	public virtual List<Tuple<String, ParameterType>> GetSpecificDefaultParameterTypes() { throw new NotImplementedException(); }
	public virtual List<PartObject> GetPartObjects() { throw new NotImplementedException(); }
	public virtual String GetName() { throw new NotImplementedException(); }
}
