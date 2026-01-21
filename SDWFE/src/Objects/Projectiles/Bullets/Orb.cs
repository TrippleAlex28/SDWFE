using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;
using SDWFE;
using SDWFE.Objects;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Projectiles.Bullets;

public class Orb : Bullet
{
    public override uint TypeId => (uint)NetObjects.Orb;
    private int _damage;
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
        ParticlePresets.CreateBulletImpact(),
        hitboxManager: hitboxManager
    )
    {
        _damage = (int)damage;
    }

    public override void OnCollision(GameObject other)
    {
        // Only apply damage when on the server
        if (GameState.Instance.SessionManager.IsClient) return;
        
        if (Collided) return;

        if (other is Player player)
        {
            player.Stats.CurrentHealth -= _damage;
            string[] namesofSound = { "CharacterHurt1", "CharacterHurt2", "CharacterHurt3" };
            SoundManager.PlaySound(namesofSound[ExtendedGame.Random.Next(0, namesofSound.Length)], volume: 0.3f);
        }
    }
}