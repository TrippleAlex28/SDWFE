using Engine;
using Engine.Particle;
using Microsoft.Xna.Framework;

namespace SDWFE;

public static class ParticlePresets
{
    public static readonly ParticleEmitter BulletTrail = new ParticleEmitterBuilder(EngineResources.BlankCircle)
        .At(0, 0)
        .WithMaxParticles(100)
        .WithEmissionRate(30f)
        .WithSpawnRadius(5f)
        .WithLifetime(.5f, 1.5f)
        .WithVelocityY(25f, 50f)
        .WithVelocityX(-10f, 10f)
        .WithRotationVelocity(-1f, 1f)
        .WithColorRange(Color.Yellow, Color.Red)
        .WithConstantScale(0.1f)
        .WithScaleCurve(curve => curve
            .AddKey(0f, 0.5f)
            .AddKey(0.5f, 0.7f)
            .AddKey(1f, 0.3f))
        .WithFadeInOut(0.1f, 0.6f)
        .AlphaBlend()                // Currently, everything is being rendered using AlphaBlend, because there isn't like a draw layers system or something
        .Build();
}