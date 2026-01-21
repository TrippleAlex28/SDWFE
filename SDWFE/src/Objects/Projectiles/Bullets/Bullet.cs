using System;
using Engine;
using Engine.Hitbox;
using Engine.Network.Shared.Session;
using Engine.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Objects.Projectiles.Bullets;

public abstract class Bullet : Projectile
{
    private float _range;
    private float _damage;
    
    /// <summary>
    /// Empty constructor, should ONLY be used for network object instantiation
    /// </summary>
    public Bullet()
    {
        
    }
    
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
        HitboxManager? hitboxManager = null,
        Vector2? size = null
    ) : base(startPos, direction, velocity, texture, owner, projectileEmitter, collisionEmitter, hitboxManager, size, damage)
    {
        this.ReplicatesOverNetwork = false;
        
        RegisterProperty(
            0,
            nameof(IsVisible),
            () => IsVisible,
            (v) => IsVisible = v
        );
        
        RegisterProperty(
            1,
            nameof(GlobalPosition),
            () => GlobalPosition,
            (v) => GlobalPosition = v
        );
        
        RegisterProperty(
            2,
            nameof(Direction),
            () => Direction,
            (v) => Direction = v
        );
        
        RegisterProperty(
            3,
            nameof(Velocity),
            () => Velocity,
            (v) => Velocity = v
        );
        
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
        // Only apply damage when on the server
        if (GameState.Instance.SessionManager.IsClient) return;
        
        if (Collided) return;
        
        if (other is Enemy enemy)
        {
            enemy.TakeDamage((int)_damage);
        }

        if (other is Player player)
        {
            player.Stats.CurrentHealth -= _damage;
            string[] namesofSound = { "CharacterHurt1", "CharacterHurt2", "CharacterHurt3" };
            SoundManager.PlaySound(namesofSound[ExtendedGame.Random.Next(0, namesofSound.Length)], volume: 0.3f);

        }
        
        base.OnCollision(other);
    }
}
