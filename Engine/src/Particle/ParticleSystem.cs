using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Particle;

public class ParticleSystem
{
    private readonly List<ParticleEmitter> _emitters = [];

    public ParticleEmitter CreateEmitter(Texture2D texture, ParticleEmitterConfig config, Vector2 position)
    {
        var emitter = new ParticleEmitter(texture, config);
        emitter.Position = position;
        _emitters.Add(emitter);
        return emitter;
    }

    public void AddEmitter(ParticleEmitter emitter)
    {
        _emitters.Add(emitter);
    }

    public ParticleEmitterBuilder NewEffect(Texture2D texture)
    {
        return new ParticleEmitterBuilder(texture);
    }

    public void Update(float deltaTime)
    {
        for (int i = _emitters.Count - 1; i >= 0; i--)
        {
            _emitters[i].Update(deltaTime);

            if (!_emitters[i].IsActive)
            {
                _emitters.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var emitter in _emitters)
        {
            emitter.Draw(spriteBatch);
        }
    }

    public void Clear()
    {
        _emitters.Clear();
    }

    public void Stop()
    {
        foreach (var emitter in _emitters)
        {
            emitter.Stop();
        }
    }

    public void Restart()
    {
        foreach (var emitter in _emitters)
        {
            emitter.Restart();
        }
    }
}