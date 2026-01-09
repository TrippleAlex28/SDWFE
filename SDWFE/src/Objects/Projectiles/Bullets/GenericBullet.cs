using Engine;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Projectiles.Bullets;

public class GenericBullet : Bullet
{
    public GenericBullet(
        Vector2 startPos, 
        Vector2 direction, 
        float velocity, 
        float range, 
        float damage,
        GameObject? owner = null
    ) : base(startPos, direction, velocity, range, damage, ExtendedGame.AssetManager.LoadTexture("GenericBullet", "Entities/Projectiles/"), owner, ParticlePresets.BulletTrail)
    {
        Sprite.Scale = new Vector2(0.5f);
    }
}