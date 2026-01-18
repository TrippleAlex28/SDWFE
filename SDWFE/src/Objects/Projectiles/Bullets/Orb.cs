using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;
using SDWFE;
using SDWFE.Objects;
using SDWFE.Objects.Projectiles.Bullets;

public class Orb : Bullet
{
    public override uint TypeId => (uint)NetObjects.Orb;
    
    /// <summary>
    /// Empty constructor, should ONLY be used for network object instantiation
    /// </summary>
    public Orb()
    {
        
    }
    
    public Orb(
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
        ExtendedGame.AssetManager.LoadTexture("16x16 Wizard Projectile", "Entities/Projectiles/"), 
        owner, 
        ParticlePresets.CreateOrbTrail(), 
        ParticlePresets.CreateRocketImpact,
        hitboxManager: hitboxManager
    )
    {
        
    }
}