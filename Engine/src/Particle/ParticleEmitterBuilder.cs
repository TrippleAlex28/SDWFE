using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Particle;

public class ParticleEmitterBuilder
{
    private ParticleEmitterConfig config;
    private Texture2D texture;
    private Vector2 position;

    public ParticleEmitterBuilder(Texture2D texture)
    {
        this.texture = texture;
        this.config = new ParticleEmitterConfig();
        this.position = Vector2.Zero;
    }

    // Position
    public ParticleEmitterBuilder At(Vector2 position)
    {
        this.position = position;
        return this;
    }

    public ParticleEmitterBuilder At(float x, float y)
    {
        this.position = new Vector2(x, y);
        return this;
    }

    // Emission
    public ParticleEmitterBuilder WithMaxParticles(int maxParticles)
    {
        config.MaxParticles = maxParticles;
        return this;
    }

    public ParticleEmitterBuilder WithEmissionRate(float rate)
    {
        config.EmissionRate = rate;
        return this;
    }

    public ParticleEmitterBuilder WithDuration(float duration)
    {
        config.Duration = duration;
        return this;
    }

    public ParticleEmitterBuilder Continuous()
    {
        config.Duration = -1f;
        return this;
    }

    // Lifetime
    public ParticleEmitterBuilder WithLifetime(float min, float max)
    {
        config.LifetimeMin = min;
        config.LifetimeMax = max;
        return this;
    }

    public ParticleEmitterBuilder WithLifetime(float lifetime)
    {
        config.LifetimeMin = lifetime;
        config.LifetimeMax = lifetime;
        return this;
    }

    // Spawn shape
    public ParticleEmitterBuilder WithSpawnOffset(Vector2 offset)
    {
        config.SpawnOffset = offset;
        return this;
    }

    public ParticleEmitterBuilder WithSpawnOffset(float x, float y)
    {
        config.SpawnOffset = new Vector2(x, y);
        return this;
    }

    public ParticleEmitterBuilder WithSpawnRadius(float radius)
    {
        config.SpawnRadius = radius;
        return this;
    }

    // Velocity
    public ParticleEmitterBuilder WithVelocity(Vector2 min, Vector2 max)
    {
        config.VelocityMin = min;
        config.VelocityMax = max;
        return this;
    }

    public ParticleEmitterBuilder WithVelocity(Vector2 velocity)
    {
        config.VelocityMin = velocity;
        config.VelocityMax = velocity;
        return this;
    }

    public ParticleEmitterBuilder WithVelocityX(float min, float max)
    {
        config.VelocityMin.X = min;
        config.VelocityMax.X = max;
        return this;
    }

    public ParticleEmitterBuilder WithVelocityY(float min, float max)
    {
        config.VelocityMin.Y = min;
        config.VelocityMax.Y = max;
        return this;
    }

    public ParticleEmitterBuilder WithGravity(Vector2 gravity)
    {
        config.Gravity = gravity;
        return this;
    }

    public ParticleEmitterBuilder WithGravity(float x, float y)
    {
        config.Gravity = new Vector2(x, y);
        return this;
    }

    // Rotation
    public ParticleEmitterBuilder WithRotation(float min, float max)
    {
        config.RotationMin = min;
        config.RotationMax = max;
        return this;
    }

    public ParticleEmitterBuilder WithRotation(float rotation)
    {
        config.RotationMin = rotation;
        config.RotationMax = rotation;
        return this;
    }

    public ParticleEmitterBuilder WithRotationVelocity(float min, float max)
    {
        config.RotationVelocityMin = min;
        config.RotationVelocityMax = max;
        return this;
    }

    public ParticleEmitterBuilder WithRotationVelocity(float velocity)
    {
        config.RotationVelocityMin = velocity;
        config.RotationVelocityMax = velocity;
        return this;
    }

    // Color
    public ParticleEmitterBuilder WithColor(Color color)
    {
        config.StartColor = color;
        config.EndColor = color;
        return this;
    }

    public ParticleEmitterBuilder WithColorRange(Color start, Color end)
    {
        config.StartColor = start;
        config.EndColor = end;
        return this;
    }

    public ParticleEmitterBuilder WithStartColor(Color color)
    {
        config.StartColor = color;
        return this;
    }

    public ParticleEmitterBuilder WithEndColor(Color color)
    {
        config.EndColor = color;
        return this;
    }

    // Curves
    public ParticleEmitterBuilder WithScaleCurve(Curve curve)
    {
        config.ScaleOverLifetime = curve;
        return this;
    }

    public ParticleEmitterBuilder WithScaleCurve(Action<CurveBuilder> builder)
    {
        var curveBuilder = new CurveBuilder();
        builder(curveBuilder);
        config.ScaleOverLifetime = curveBuilder.Build();
        return this;
    }

    public ParticleEmitterBuilder WithLinearScale(float start, float end)
    {
        config.ScaleOverLifetime = Curve.Linear(start, end);
        return this;
    }

    public ParticleEmitterBuilder WithConstantScale(float scale)
    {
        config.ScaleOverLifetime = Curve.Constant(scale);
        return this;
    }

    public ParticleEmitterBuilder WithAlphaCurve(Curve curve)
    {
        config.AlphaOverLifetime = curve;
        return this;
    }

    public ParticleEmitterBuilder WithAlphaCurve(Action<CurveBuilder> builder)
    {
        var curveBuilder = new CurveBuilder();
        builder(curveBuilder);
        config.AlphaOverLifetime = curveBuilder.Build();
        return this;
    }

    public ParticleEmitterBuilder WithFadeIn(float duration = 0.2f)
    {
        var curve = new Curve();
        curve.AddKey(0f, 0f);
        curve.AddKey(duration, 1f);
        curve.AddKey(1f, 1f);
        config.AlphaOverLifetime = curve;
        return this;
    }

    public ParticleEmitterBuilder WithFadeOut(float startTime = 0.7f)
    {
        var curve = new Curve();
        curve.AddKey(0f, 1f);
        curve.AddKey(startTime, 1f);
        curve.AddKey(1f, 0f);
        config.AlphaOverLifetime = curve;
        return this;
    }

    public ParticleEmitterBuilder WithFadeInOut(float fadeInDuration = 0.2f, float fadeOutStart = 0.7f)
    {
        var curve = new Curve();
        curve.AddKey(0f, 0f);
        curve.AddKey(fadeInDuration, 1f);
        curve.AddKey(fadeOutStart, 1f);
        curve.AddKey(1f, 0f);
        config.AlphaOverLifetime = curve;
        return this;
    }

    // Blend mode
    public ParticleEmitterBuilder WithBlendMode(BlendState blendState)
    {
        config.BlendState = blendState;
        return this;
    }

    public ParticleEmitterBuilder Additive()
    {
        config.BlendState = BlendState.Additive;
        return this;
    }

    public ParticleEmitterBuilder AlphaBlend()
    {
        config.BlendState = BlendState.AlphaBlend;
        return this;
    }

    // Build
    public ParticleEmitter Build()
    {
        var emitter = new ParticleEmitter(texture, config);
        emitter.Position = position;
        return emitter;
    }

    public ParticleEmitter BuildAndAdd(ParticleSystem particleSystem)
    {
        var emitter = Build();
        particleSystem.AddEmitter(emitter);
        return emitter;
    }
}