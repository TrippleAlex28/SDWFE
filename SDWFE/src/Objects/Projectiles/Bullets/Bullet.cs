using System;
using Engine;
using Engine.Hitbox;
using Engine.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.Enemies;

namespace SDWFE.Objects.Projectiles.Bullets;

public abstract class Bullet : Projectile
{
    private float _range;
    private float _damage;
    
    public Bullet(
        Vector2 startPos, 
        Vector2 direction, 
        float velocity, 
        float range, 
        float damage, 
        Texture2D texture,
        GameObject? owner = null, 
        ParticleEmitter? projectileEmitter = null, 
        ParticleEmitter? collisionEmitter = null,
        HitboxManager? hitboxManager = null
    ) : base(startPos, direction, velocity, texture, owner, projectileEmitter, collisionEmitter, hitboxManager)
    {
        _range = range;
        _damage = damage;

        Sprite.OriginType = OriginType.Center;
        Sprite.AngleRadians = (float)Math.Atan2(Direction.Y, Direction.X);
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        _range -= this.Displacement.Length() * gameTime.DeltaSeconds();
        if (_range <= 0f)
            OnCollision(null);
        
        base.UpdateSelf(gameTime);
    }

    public override void OnCollision(GameObject other)
    {
        if (Collided) return;
        
        if (other is Enemy enemy)
        {
            enemy.TakeDamage((int)_damage);
        }
        
        base.OnCollision(other);
    }
}
