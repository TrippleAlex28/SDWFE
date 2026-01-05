namespace Engine.Particle;

public class CurveBuilder
{
    private Curve curve = new();

    public CurveBuilder AddKey(float time, float value)
    {
        curve.AddKey(time, value);
        return this;
    }

    public Curve Build()
    {
        return curve;
    }
}