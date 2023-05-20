using Godot;
using System;
using System.Collections.Generic;

public partial class BooleanParameter : ParameterType
{
	public bool defaultValue;

	public BooleanParameter(bool defaultValue)
	{
		this.defaultValue = defaultValue;		
	}

	public override bool Equals(object obj)
    {
		if (!(obj is BooleanParameter))
			return false;
		BooleanParameter other = (BooleanParameter)obj;

		if (defaultValue != other.defaultValue)
			return false;

		return true;
    }

	public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
