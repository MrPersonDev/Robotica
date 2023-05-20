using Godot;
using System;
using System.Collections.Generic;

public partial class DropdownParameter : ParameterType
{
	public List<Object> Values;

	public DropdownParameter()
	{
		this.Values = new List<Object>();
	}

	public DropdownParameter(List<Object> values)
	{
		this.Values = values;
	}

	public override bool Equals(object obj)
    {
		if (!(obj is DropdownParameter))
			return false;
		DropdownParameter other = (DropdownParameter)obj;

		if (Values.Count != other.Values.Count)
			return false;

		for (int i = 0; i < Values.Count; i++)
		{
			if (!Values[i].Equals(other.Values[i]))
				return false;
		}

		return true;
    }

	public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
