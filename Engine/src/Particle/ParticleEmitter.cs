using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Particle;

public class ParticleEmitter
{
    private ParticleEmitterConfig config;
    private Particle[] particles;
    private Texture2D texture;
    private Random random;
    private float emissionAccumulator;
    private float age;
    private bool isEmitting = true;

    public Vector2 Position { get; set; }
    public bool IsActive { get; private set; } = true;

    public ParticleEmitter(Texture2D texture, ParticleEmitterConfig config)
    {
        this.texture = texture;
        this.config = config;
        this.particles = new Particle[config.MaxParticles];
        this.random = new Random();

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new Particle { IsActive = false };
        }
    }

    public void Update(float deltaTime)
    {
        if (!IsActive) return;

        age += deltaTime;

        if (config.Duration > 0 && age >= config.Duration)
        {
            isEmitting = false;
        }

        if (isEmitting)
        {
            emissionAccumulator += config.EmissionRate * deltaTime;
            int particlesToEmit = (int)emissionAccumulator;
            emissionAccumulator -= particlesToEmit;

            for (int i = 0; i < particlesToEmit; i++)
            {
                EmitParticle();
            }
        }

        bool anyActive = false;
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].IsActive)
            {
                UpdateParticle(ref particles[i], deltaTime);
                anyActive = true;
            }
        }

        if (!isEmitting && !anyActive)
        {
            IsActive = false;
        }
    }

    private void EmitParticle()
    {
        int index = FindInactiveParticle();
        if (index == -1) return;

        ref Particle p = ref particles[index];
        p.IsActive = true;
        p.Age = 0f;
        p.Lifetime = Lerp(config.LifetimeMin, config.LifetimeMax, (float)random.NextDouble());

        float angle = (float)(random.NextDouble() * Math.PI * 2);
        float distance = (float)random.NextDouble() * config.SpawnRadius;
        p.Position = Position + config.SpawnOffset + new Vector2(
            (float)Math.Cos(angle) * distance,
            (float)Math.Sin(angle) * distance
        );

        p.Velocity = new Vector2(
            Lerp(config.VelocityMin.X, config.VelocityMax.X, (float)random.NextDouble()),
            Lerp(config.VelocityMin.Y, config.VelocityMax.Y, (float)random.NextDouble())
        );

        p.Rotation = Lerp(config.RotationMin, config.RotationMax, (float)random.NextDouble());
        p.RotationVelocity = Lerp(config.RotationVelocityMin, config.RotationVelocityMax, (float)random.NextDouble());

        p.Color = config.StartColor;
        p.Scale = 1f;
    }

    private void UpdateParticle(ref Particle p, float deltaTime)
    {
        p.Age += deltaTime;

        if (p.Age >= p.Lifetime)
        {
            p.IsActive = false;
            return;
        }

        p.Velocity += config.Gravity * deltaTime;
        p.Position += p.Velocity * deltaTime;
        p.Rotation += p.RotationVelocity * deltaTime;

        float normalizedAge = p.NormalizedAge;
        p.Scale = config.ScaleOverLifetime.Evaluate(normalizedAge);
        float alpha = config.AlphaOverLifetime.Evaluate(normalizedAge);

        p.Color = Color.Lerp(config.StartColor, config.EndColor, normalizedAge);
        p.Color *= alpha;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsActive && !HasActiveParticles()) return;

        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].IsActive)
            {
                spriteBatch.Draw(
                    texture,
                    particles[i].Position,
                    null,
                    particles[i].Color,
                    particles[i].Rotation,
                    origin,
                    particles[i].Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }

    private int FindInactiveParticle()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (!particles[i].IsActive)
                return i;
        }
        return -1;
    }

    private bool HasActiveParticles()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i].IsActive)
                return true;
        }
        return false;
    }

    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public void Stop()
    {
        isEmitting = false;
    }

    public void Restart()
    {
        isEmitting = true;
        age = 0f;
        IsActive = true;
    }
}