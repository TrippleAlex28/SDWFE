using Engine;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Projectiles.Bullets;

public class FireworkRocket : Bullet
{
    public FireworkRocket(
        Vector2 startPos, 
        Vector2 direction, 
        float velocity, 
        float range, 
        float damage,
        GameObject? owner = null
    ) : base(startPos, direction, velocity, range, damage, ExtendedGame.AssetManager.LoadTexture("GenericBullet", "Entities/Projectiles/"), owner, ParticlePresets.BulletTrail)
    {
        
    }
}