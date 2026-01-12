using Engine;
using Engine.Particle;
using Microsoft.Xna.Framework;

namespace SDWFE;

public static class ParticlePresets
{
    public static readonly ParticleEmitter BulletTrail = new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(120)
        .WithEmissionRate(100f)
        .Continuous()
        .WithSpawnRadius(2f)
        .WithLifetime(0.12f, 0.28f)
        .WithVelocity(new Vector2(5f, -5f), new Vector2(10f, 5f))
        .WithRotationVelocity(-2f, 2f)
        .WithColorRange(Color.Yellow, Color.OrangeRed)
        .WithScaleCurve(c => c
            .AddKey(0f, 0.18f)
            .AddKey(0.4f, 0.26f)
            .AddKey(1f, 0.05f))
        .WithFadeInOut(0.05f, 0.55f)
        .Additive()
        .Build();
    
    public static readonly ParticleEmitter BulletImpact = new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(40)
        .WithEmissionRate(200f)
        .WithDuration(0.05f)                // One-shot burst
        .WithSpawnRadius(2f)
        .WithLifetime(0.15f, 0.35f)
        .WithVelocity(
            new Vector2(-120f, -120f),
            new Vector2(120f, 120f))
        .WithRotationVelocity(-6f, 6f)
        .WithColorRange(Color.Orange, Color.Yellow)
        .WithScaleCurve(curve => curve
            .AddKey(0f, 0.25f)
            .AddKey(1f, 0.05f))
        .WithFadeOut(0.6f)
        .Additive()
        .Build();
    
    public static readonly ParticleEmitter RocketTrail = new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(300)
        .WithEmissionRate(60f)
        .Continuous()
        .WithSpawnRadius(4f)
        .WithLifetime(0.8f, 1.6f)
        .WithVelocity(
            new Vector2(-20f, 30f),
            new Vector2(20f, 80f))
        .WithGravity(0f, 20f)
        .WithRotationVelocity(-1f, 1f)
        .WithColorRange(
            new Color(255, 140, 40),
            new Color(100, 100, 100))
        .WithScaleCurve(curve => curve
            .AddKey(0f, 0.2f)
            .AddKey(1f, 1.0f))
        .WithFadeInOut(0.1f, 0.7f)
        .AlphaBlend()
        .Build();
    
    public static readonly ParticleEmitter RocketImpact = new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(200)
        .WithEmissionRate(400f)
        .WithDuration(0.15f)                 // Burst explosion
        .WithSpawnRadius(12f)
        .WithLifetime(0.6f, 1.4f)
        .WithVelocity(
            new Vector2(-180f, -180f),
            new Vector2(180f, 180f))
        .WithGravity(0f, 60f)                // Smoke falls slightly
        .WithRotationVelocity(-3f, 3f)
        .WithColorRange(
            new Color(255, 180, 80),
            new Color(80, 80, 80))           // Fire → smoke
        .WithScaleCurve(curve => curve
            .AddKey(0f, 0.4f)
            .AddKey(0.3f, 1.2f)
            .AddKey(1f, 1.8f))
        .WithFadeInOut(0.05f, 0.5f)
        .AlphaBlend()
        .Build();
}