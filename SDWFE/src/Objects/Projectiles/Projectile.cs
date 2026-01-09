using System;
using System.Threading.Tasks;
using Engine;
using Engine.Particle;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.Objects.Projectiles;

public abstract class Projectile : GameObject
{
    // Store owner to ignore with collision
    private readonly GameObject? _owner;

    private readonly ParticleSystem _projectileTrail = new();
    private readonly ParticleEmitter? _projectileEmitter;
    private readonly ParticleSystem _collisionEffect = new();
    private readonly ParticleEmitter? _collisionEmitter;
    protected readonly Sprite Sprite;

    protected bool Collided = false;
    
    public Projectile(
        Vector2 startPos, 
        Vector2 direction, 
        float velocity, 
        Texture2D texture, 
        GameObject? owner = null, 
        ParticleEmitter? projectileEmitter = null, 
        ParticleEmitter? collisionEmitter = null
    )
    {
        this.GlobalPosition = startPos;
        this.Direction = direction;
        this.Velocity = velocity;

        Sprite = new Sprite(texture);
        AddChild(Sprite);
        
        this._owner = owner;
        this._projectileEmitter = projectileEmitter;
        this._collisionEmitter = collisionEmitter;
    }
    
    protected override void EnterSelf()
    {
        base.EnterSelf();

        if (_projectileEmitter != null)
            _projectileTrail.AddEmitter(_projectileEmitter);
        
        if (_collisionEmitter != null)
            _collisionEffect.AddEmitter(_collisionEmitter);
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        _projectileTrail.Update(gameTime.DeltaSeconds());
        
        if (Collided)
            _collisionEffect.Update(gameTime.DeltaSeconds());
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        _projectileTrail.Draw(spriteBatch);
        
        if (Collided)
            _collisionEffect.Draw(spriteBatch);
    }

    public virtual void OnCollision(GameObject other)
    {
        Collided = true;
        Velocity = 0f;

        if (_collisionEmitter != null)
        {
            Task.Run(async () =>
            {
                bool isInfinite = _collisionEmitter.Config.Duration.IsApproximatelyEqual(-1f);
                if (isInfinite)
                {
                    Console.WriteLine("Projectile Collision Effect shouldn't have an infinite duration!!!");
                }
                await Task.Delay(isInfinite ? 0 : (int)_collisionEmitter.Config.Duration * 1000);
                RemoveFromParent();
            });
        }
        else
        {
            RemoveFromParent();
        }
        
    }
}