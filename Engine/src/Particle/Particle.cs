using Microsoft.Xna.Framework;

namespace Engine.Particle;

public class Particle
{
    public bool IsActive;
    
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public float RotationVelocity;
    public float Age;
    public float Lifetime;
    public Color Color;
    public float Scale;

    public float NormalizedAge => Age / Lifetime;
}