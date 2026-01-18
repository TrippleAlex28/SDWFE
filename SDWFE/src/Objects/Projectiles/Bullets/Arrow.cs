using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Projectiles.Bullets;

public class Arrow : Bullet
{
    public override uint TypeId => (uint)NetObjects.Arrow;

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
        Sprite.Scale = new Vector2(0.5f);
    }
}