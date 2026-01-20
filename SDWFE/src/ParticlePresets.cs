using Engine;
using Engine.Particle;
using Microsoft.Xna.Framework;

namespace SDWFE;

public static class ParticlePresets
{
    public static ParticleEmitter CreateBulletTrail() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
            .WithMaxParticles(512)
            .WithEmissionRate(32f)
            .Continuous()
            .WithSpawnRadius(1.5f)
            .WithLifetime(0.05f, 0.1f)
            .WithVelocity(new Vector2(-5f, -5f), new Vector2(5f, 5f))
            .WithRotationVelocity(-5f, 5f)
            .WithColorRange(Color.Yellow, Color.OrangeRed)
            .WithScaleCurve(c => c
                .AddKey(0f, 0.1f)
                .AddKey(0.4f, 0.2f)
                .AddKey(1f, 0.05f))
            .WithFadeInOut(0.05f, 0.55f)
            .Additive()
            .Build();

    public static ParticleEmitter CreateBulletImpact() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
            .WithMaxParticles(8)
            .WithEmissionRate(256f)
            .WithDuration(0.25f)                // One-shot burst
            .WithSpawnRadius(1.5f)
            .WithLifetime(0.15f, 0.3f)
            .WithVelocity(
                new Vector2(-96f, -96f),
                new Vector2(96f, 96f))
            .WithRotationVelocity(-6f, 6f)
            .WithColorRange(Color.Red, Color.Yellow)
            .WithScaleCurve(curve => curve
                .AddKey(0f, 0.25f)
                .AddKey(1f, 0.05f))
            .WithFadeOut(0.5f)
            .Additive()
            .Build();

    public static ParticleEmitter CreateRocketTrail() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
            .WithMaxParticles(600)
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
    
    public static ParticleEmitter CreateRocketImpact => new ParticleEmitterBuilder(EngineResources.BlankCircle)
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
    
    public static ParticleEmitter CreateOrbTrail() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(512)
        .WithEmissionRate(32f)
        .Continuous()
        .WithSpawnRadius(1.5f)
        .WithLifetime(0.05f, 0.1f)
        .WithVelocity(new Vector2(-5f, -5f), new Vector2(5f, 5f))
        .WithRotationVelocity(-5f, 5f)
        .WithColorRange(Color.Green, Color.Lime)
        .WithScaleCurve(c => c
            .AddKey(0f, 0.1f)
            .AddKey(0.4f, 0.2f)
            .AddKey(1f, 0.05f))
        .WithFadeInOut(0.05f, 0.55f)
        .Additive()
        .Build();

    public static ParticleEmitter CreateFreeze() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(512)
        .WithEmissionRate(64f)
        .Continuous()
        .WithLifetime(1f, 2f)
        .WithVelocity(new Vector2(-80f), new Vector2(80f))
        .WithSpawnRadius(6f)
        .WithColorRange(new Color(150, 220, 255), new Color(200, 240, 255))
        .WithRotationVelocity(-3f, 3f)
        .WithScaleCurve(c => c
            .AddKey(0f, .2f)
            .AddKey(.2f, .8f)
            .AddKey(.8f, 1.2f)
            .AddKey(1f, .3f))
        .WithAlphaCurve(c => c
            .AddKey(0f, 0f)
            .AddKey(.2f, 1f)
            .AddKey(.7f, .8f)
            .AddKey(1f, 0f))
        .Additive()
        .Build();

    public static ParticleEmitter CreateFreezeMist() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(128)
        .WithEmissionRate(32f)
        .Continuous()
        .WithLifetime(1.5f, 2.5f)
        .WithVelocity(new Vector2(-30), new Vector2(30))
        .WithSpawnRadius(8f)
        .WithColorRange(new Color(180, 230, 255, 100), new Color(220, 245, 255, 50))
        .WithRotationVelocity(-1f, 1f)
        .WithScaleCurve(c => c
            .AddKey(0f, .5f)
            .AddKey(.4f, 1.5f)
            .AddKey(1f, 2.5f))
        .WithAlphaCurve(c => c
            .AddKey(0f, 0f)
            .AddKey(.2f, .6f)
            .AddKey(.6f, .4f)
            .AddKey(1f, 0f))
        .Additive()
        .Build();

    public static ParticleEmitter CreateRageBubbles() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(384)
        .WithEmissionRate(96f)
        .Continuous()
        .WithLifetime(.5f, 1.2f)
        .WithVelocity(new Vector2(-100, -180), new Vector2(100, -80))
        .WithSpawnRadius(8f)
        .WithColorRange(new Color(140, 30, 180), new Color(200, 60, 220))
        .WithRotationVelocity(-5f, 5f)
        .WithScaleCurve(c => c
            .AddKey(0f, .2f)    
            .AddKey(.3f, .6f)    
            .AddKey(1f, .05f))
        .WithFadeOut(.7f)
        .AlphaBlend()
        .Build();

    public static ParticleEmitter CreateSlam() => new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .WithMaxParticles(512)
        .WithEmissionRate(1024f)
        .WithLifetime(.1f)
        .WithLifetime(.6f, 1f)
        .WithVelocity(Vector2.Zero)
        .WithSpawnRadius(2f)
        .WithColorRange(new Color(200, 180, 140), new Color(160, 140, 100))
        .WithScaleCurve(c => c
            .AddKey(0f, .5f)
            .AddKey(.1f, 2f)
            .AddKey(.4f, 6f)
            .AddKey(1f, 10f))
        .WithAlphaCurve(c => c
            .AddKey(0f, .8f)
            .AddKey(.3f, .6f)
            .AddKey(.6f, .2f)
            .AddKey(1f, 0f))
        .Build();
}