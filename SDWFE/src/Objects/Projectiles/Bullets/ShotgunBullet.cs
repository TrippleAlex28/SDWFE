using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Projectiles.Bullets;

public class ShotgunBullet : Bullet
{
    public override uint TypeId => (uint)NetObjects.ShotgunBullet;
    
    /// <summary>
    /// Empty constructor, should ONLY be used for network object instantiation
    /// </summary>
    public ShotgunBullet()
    {
        
    }
    
    public ShotgunBullet(
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
        ExtendedGame.AssetManager.LoadTexture("GenericBullet", "Entities/Projectiles/"), 
        owner, 
        ParticlePresets.BulletTrail, 
        ParticlePresets.BulletImpact,
        hitboxManager: hitboxManager
    )
    {
        Sprite.Scale = new Vector2(0.25f);
    }
}