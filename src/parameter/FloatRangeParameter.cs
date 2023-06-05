public class FloatRangeParameter : ParameterType
{
    private float min, max, step, defaultValue;

    public FloatRangeParameter(float min, float max, float step, float defaultValue)
    {
        this.min = min;
        this.max = max;
        this.step = step;
        this.defaultValue = defaultValue;
    }

    public float GetMin()
    {
        return min;
    }

    public float GetMax()
    {
        return max;
    }

    public float GetStep()
    {
        return step;
    }

    public float GetDefaultValue()
    {
        return defaultValue;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is FloatRangeParameter))
            return false;
        FloatRangeParameter other = (FloatRangeParameter)obj;
        return min == other.min && max == other.max && step == other.step && defaultValue == other.defaultValue;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
