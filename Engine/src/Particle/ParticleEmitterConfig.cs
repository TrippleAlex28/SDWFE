using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Particle;

public class ParticleEmitterConfig
{
    // Emission
    public int MaxParticles = 1000;
    public float EmissionRate = 50f; // particles per second
    public float Duration = -1f; // -1 = infinite

    // Lifetime
    public float LifetimeMin = 1f;
    public float LifetimeMax = 2f;

    // Spawn shape
    public Vector2 SpawnOffset = Vector2.Zero;
    public float SpawnRadius = 0f;

    // Velocity
    public Vector2 VelocityMin = new Vector2(-50, -50);
    public Vector2 VelocityMax = new Vector2(50, 50);
    public Vector2 Gravity = Vector2.Zero;

    // Rotation
    public float RotationMin = 0f;
    public float RotationMax = 0f;
    public float RotationVelocityMin = 0f;
    public float RotationVelocityMax = 0f;

    // Color
    public Color StartColor = Color.White;
    public Color EndColor = Color.White;

    // Scale curves
    public Curve ScaleOverLifetime = Curve.Constant(1f);
    public Curve AlphaOverLifetime = Curve.Constant(1f);

    // Blend mode
    public BlendState BlendState = BlendState.AlphaBlend;
}

