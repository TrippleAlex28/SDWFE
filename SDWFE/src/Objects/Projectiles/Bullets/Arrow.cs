using System;
using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Objects.Projectiles.Bullets;

public class Arrow : Bullet
{
    public override uint TypeId => (uint)NetObjects.Arrow;
    private GameObject? _owner;
    /// <summary>
    /// Empty constructor, should ONLY be used for network object instantiation
    /// </summary>
    public Arrow()
    {
        
    }
    
    public Arrow(
        Vector2 startPos, 
        Vector2 direction, 
        float velocity, 
        float range, 
        float damage,
        GameObject? owner = null,
        HitboxManager? hitboxManager = null
    ) : base(
        startPos, 
        direction, 
        velocity, 
        range, 
        damage, 
        ExtendedGame.AssetManager.LoadTexture("16x16 Arrow", "Entities/Projectiles/"), 
        owner, 
        ParticlePresets.CreateBulletTrail(), 
        ParticlePresets.CreateBulletImpact(),    
        hitboxManager: hitboxManager,
        new Vector2(4, 4)
    )
    {
        _owner = owner;
        Sprite.Scale = new Vector2(0.5f);
    }

    public override void OnCollision(GameObject other)
    {
        base.OnCollision(other);
        if (other is Enemy && _owner is Player player)
        {
            player.Stats.CurrentStamina += 5f;
        }
        //SoundEffect hitSound = ExtendedGame.AssetManager.LoadSoundEffect("ArrowHit", "SFX/");
        //hitSound.Play(volume: 0.2f, pitch: 0.0f, pan: 0.0f);
    }
}