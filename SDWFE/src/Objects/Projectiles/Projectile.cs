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
    private readonly ParticleEmitter? _collisionEmitter;
    private readonly Sprite _sprite;

    public Projectile(Vector2 startPos, Vector2 direction, float velocity, GameObject? owner = null, ParticleEmitter? projectileEmitter = null, ParticleEmitter? collisionEmitter = null)
    {
        this.GlobalPosition = startPos;
        this.Direction = direction;
        this.Velocity = velocity;
        this._owner = owner;

        this._projectileEmitter = projectileEmitter;
        this._collisionEmitter = collisionEmitter;
    }
    
    protected override void EnterSelf()
    {
        base.Enter();

        if (_projectileEmitter != null)
            _projectileTrail.AddEmitter(_projectileEmitter);
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        _projectileTrail.Update(gameTime.DeltaSeconds());
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        _projectileTrail.Draw(spriteBatch);
    }

    public virtual void OnCollision(GameObject other)
    {
        RemoveFromParent();
    }
}