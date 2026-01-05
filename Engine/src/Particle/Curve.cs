using Microsoft.Xna.Framework;

namespace Engine.Particle;

public struct CurveKey
{
    public float Time;
    public float Value;

    public CurveKey(float time, float value)
    {
        Time = time;
        Value = value;
    }
}

public class Curve
{
    private List<CurveKey> keys = new();

    public void AddKey(float time, float value)
    {
        keys.Add(new CurveKey(time, value));
        keys.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

    public float Evaluate(float time)
    {
        if (keys.Count == 0) return 0f;
        if (keys.Count == 1) return keys[0].Value;
        
        if (time <= keys[0].Time) return keys[0].Value;
        if (time >= keys[^1].Time) return keys[^1].Value;

        for (int i = 0; i < keys.Count - 1; i++)
        {
            if (time >= keys[i].Time && time <= keys[i + 1].Time)
            {
                float p = (time - keys[i].Time) / (keys[i + 1].Time - keys[i].Time);
                return MathHelper.Lerp(keys[i].Value, keys[i + 1].Value, p);
            }
        }

        return keys[^1].Value;
    }
    
    public static Curve Linear(float start, float end)
    {
        var curve = new Curve();
        curve.AddKey(0f, start);
        curve.AddKey(1f, end);
        return curve;
    }

    public static Curve Constant(float value)
    {
        var curve = new Curve();
        curve.AddKey(0f, value);
        curve.AddKey(1f, value);
        return curve;
    }
}