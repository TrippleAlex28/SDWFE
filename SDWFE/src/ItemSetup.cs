using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Engine;
using Engine.Hitbox;
using Engine.Input;
using Engine.Scene;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;
using SDWFE.Objects.Projectiles.Bullets;

#nullable enable

namespace SDWFE;

public static class ItemSetup
{
    public const string BANDAGE = "Bandage";
    public const string MEDKIT = "Medkit";

    public const string ADRENALINE = "Adrenaline";
    public const string FREEZE = "Freeze";
    public const string RAGE = "Rage";
    public const string SLAM = "Slam";
    
    public const string BOW = "Bow";
    public const string PISTOL = "Pistol";
    public const string ASSAULT_RIFLE = "Assault Rifle";
    public const string SHOTGUN = "Shotgun";
    public const string FIREWORK_LAUNCHER = "Firework Launcher";
    public const string MELEE = "Melee";

    public const string ACTION_SHOOT = "Shoot";
    
    
    public static readonly Dictionary<string, ItemData> ItemDataMap = new()
    {
        {
            BANDAGE, new ItemData
            {
                Name = BANDAGE,
                MaxStackSize = 8,
                IconPath = "Heal",
                UseActionId = BANDAGE,
                Price = 30,
            }
        },
        {
            ADRENALINE, new ItemData
            {
                Name = ADRENALINE,
                MaxStackSize = 2,
                IconPath = "Adrenaline",
                UseActionId = ADRENALINE,
                Price = 40,
            }
        },
        {
            FREEZE, new ItemData
            {
                Name = FREEZE,
                MaxStackSize = 2,
                IconPath = "Freeze",
                UseActionId = FREEZE,
                Price = 75,
            }
        },
        {
            RAGE, new ItemData
            {
                Name = RAGE,
                MaxStackSize = 2,
                IconPath = "Rage",
                UseActionId = RAGE,
                Price = 100,
            }
        },
        {
            SLAM, new ItemData
            {
                Name = SLAM,
                MaxStackSize = 2,
                IconPath = "Slam",
                UseActionId = SLAM,
                Price = 150,
            }
        },
        {
            BOW, new WeaponData
            {
                Name = BOW,
                Damage = 25f,
                AttackSpeed = 5f,
                Range = 150f,
                Velocity = 200f,
                IconPath = "32x32 Bow",
                UseActionId = ACTION_SHOOT,
                BulletType = BulletType.Arrow,
                Price = 10,
            }
        },
        {
            PISTOL, new WeaponData
            {
                Name = PISTOL,
                Damage = 50f,
                AttackSpeed = 2f,
                Range = 250f,
                Velocity = 350f,
                IconPath = "Pistol",
                UseActionId = ACTION_SHOOT,
                Price = 20,
            }
        },
        {
            ASSAULT_RIFLE, new WeaponData
            {
                Name = ASSAULT_RIFLE,
                Damage = 50f,
                AttackSpeed = 5f,
                Range = 500f,
                Velocity = 550f,
                IconPath = "AssaultRifle",
                UseActionId = ACTION_SHOOT,
                Price = 50,
            }
        },
        {
            SHOTGUN, new WeaponData
            {
                Name = SHOTGUN,
                Damage = 15f,
                AttackSpeed = 1f,
                Range = 80f,
                Velocity = 450f,
                BulletType = BulletType.Shotgun,
                IconPath = "Shotgun",
                UseActionId = ACTION_SHOOT,
                Price = 50,
            }
        },
        {
            FIREWORK_LAUNCHER, new WeaponData
            {
                Name = FIREWORK_LAUNCHER,
                Damage = 250f,
                AttackSpeed = .5f,
                Range = 1000f,
                Velocity = 200f,
                BulletType = BulletType.FireworkRocket,
                IconPath = "rpg",
                UseActionId = ACTION_SHOOT,
                Price = 100,
            }
        },
        {
            MELEE, new WeaponData
            {
                Name = MELEE,
                Damage = 40f,
                AttackSpeed = 1.5f,
                Range = 50f,
                Velocity = 0f,
                IconPath = "32x32 Sword",
                UseActionId = ACTION_SHOOT,
                Price = 25,
            }
        },
    };

    public static void Initialize()
    {
        SoundEffect itemUseSound = ExtendedGame.AssetManager.LoadSoundEffect("AbilityUse", "SFX/");
        ItemActionRegistry.RegisterUse(BANDAGE, (player, data, direction) =>
        {
            player.Stats.CurrentHealth += 100;
            itemUseSound.Play(volume: 0.2f, pitch: 0.0f, pan: 0.0f);
        });
        ItemActionRegistry.RegisterUse(ADRENALINE, (player, data, direction) =>
        {
            player.ApplyMovementMultiplier(.75f, 2f);
        });
        ItemActionRegistry.RegisterUse(FREEZE, (player, data, direction) =>
        {
            const float duration = 3f;
            
            Scene? scene = GameState.Instance.CurrentScene;
            if (scene == null) return;

            // Get enemy closest to the pointer position
            var enemies = scene.GetAllTypes<Enemy>();
            if (enemies.Count <= 0) return;

            Vector2 mouse = ExtendedGame.ScreenToWorld(InputManager.Instance.MousePosition.ToVector2());
            Enemy? best = null;
            float bestDistance = 9999f;
            foreach (var enemy in enemies)
            {
                if (best == null)
                {
                    best = enemy;
                    break;
                }

                if (Vector2.Distance(mouse, best.GlobalPosition) < bestDistance)
                    best = enemy;
            }
            
            best!.Freeze(duration);
        });
        ItemActionRegistry.RegisterUse(RAGE, (player, data, direction) =>
        {
            player.ApplyDamageMultiplier(.5f, 2f);
        });
        ItemActionRegistry.RegisterUse(SLAM, (player, data, direction) =>
        {
            const float range = 250f;
            const int damage = 50;
            
            Scene? scene = GameState.Instance.CurrentScene;
            if (scene == null) return;

            // Create the player slam particle effect
            player.StartSlamEffect();
            
            // Get enemy closest to the pointer position
            var enemies = scene.GetAllTypes<Enemy>();
            if (enemies.Count <= 0) return;

            foreach (var enemy in enemies)
            {
                if (Vector2.Distance(player.GlobalPosition, enemy.GlobalPosition) <= range)
                {
                    enemy.TakeDamage(damage);
                }
            }
        });
        ItemActionRegistry.RegisterUse(ACTION_SHOOT, (player, data, direction) =>
        {
            // ===== CHECK COOLDOWN =====
            if (!player.CanShoot) return;
            
            // ===== SHOOT =====
            var weaponData = data as WeaponData;
            
            Scene? scene = GameState.Instance.CurrentScene;
            if (scene == null) return;
            
            if (weaponData!.Name == MELEE)
            {   
                SoundEffect meleeSound = ExtendedGame.AssetManager.LoadSoundEffect("SwordSlash", "SFX/");
                meleeSound.Play(volume: 0.2f, pitch: 0.5f, pan: 0.8f);
                // Melee attack
                const float meleeRange = 48f;
                const float meleeArc = MathF.PI / 3f; // 60 degrees

                Texture2D? meleeAttackTexture = ExtendedGame.AssetManager.LoadTexture("64x48 Sword Swing-Sheet", "Items/");
                AnimationData meleeAttackAnimData = new AnimationData(meleeAttackTexture, 0, 100f, false);
                AnimatedSprite? attackAnim = new AnimatedSprite(meleeAttackTexture, 64, 48);
                attackAnim.PlayOneShot(meleeAttackAnimData);
                attackAnim.AnimationCompleted += () => player.RemoveChild(attackAnim);
                float rotation = MathF.Atan2(direction.Y, direction.X) + MathF.PI * 2.5f;

                if (!direction.IsApproximatelyZero())
                {
                    attackAnim.AngleRadians = rotation;
                }
                attackAnim.OriginType = OriginType.Center;
                player.AddChild(attackAnim);

                Vector2 middlePlayerPos = player.GlobalPosition + new Vector2(8, 16);
                attackAnim.GlobalPosition = player.GlobalPosition + new Vector2(8, 20) + direction * (meleeRange / 2f);
                attackAnim.BaseDrawLayer = 1f;
                float xRect = direction.X >= 0 ? attackAnim.GlobalPosition.X : attackAnim.GlobalPosition.X - meleeRange;
                float yRect = direction.Y >= 0 ? attackAnim.GlobalPosition.Y - meleeRange : attackAnim.GlobalPosition.Y + meleeRange;
                FloatRect collisionBox = new FloatRect(
                    xRect,
                    yRect,
                    meleeRange,
                    meleeRange);

                
                rotation = rotation - MathF.PI * 1.5f;
                if (rotation >= MathF.PI * 1.75f || rotation <= MathF.PI * 0.25f)
                {
                    // left side
                    collisionBox = new FloatRect(
                        middlePlayerPos.X - meleeRange,
                        middlePlayerPos.Y - (meleeRange / 2f),
                        meleeRange,
                        meleeRange * 1.5f);
                }
                else if (rotation > MathF.PI * 0.25f && rotation < MathF.PI * 0.75f)
                {
                    // top side
                    collisionBox = new FloatRect(
                        middlePlayerPos.X - meleeRange / 2f,
                        middlePlayerPos.Y - meleeRange,
                        meleeRange * 1.5f,
                        meleeRange);
                }
                else if (rotation >= MathF.PI * 0.75f && rotation < MathF.PI * 1.25f)
                {
                    collisionBox = new FloatRect(
                        middlePlayerPos.X,
                        middlePlayerPos.Y - meleeRange / 2f,
                        meleeRange,
                        meleeRange * 1.5f);
                } else
                {
                    // bottom side
                    collisionBox = new FloatRect(
                        middlePlayerPos.X - meleeRange / 2f,
                        middlePlayerPos.Y,
                        meleeRange * 1.5f,
                        meleeRange);
                }

                List<StaticHitbox> hitboxes = player.HitboxManager!.GetStaticCollisions(collisionBox, HitboxLayer.Enemy, player);
                
                foreach (var hitbox in hitboxes)
                {
                    if (hitbox.Owner is Enemy enemy)
                    {
                        SoundEffect hitSound = ExtendedGame.AssetManager.LoadSoundEffect("SwordHit", "SFX/");
                        hitSound.Play(volume: 0.3f, pitch: 0.0f, pan: 0.0f);
                        enemy.TakeDamage((int)(weaponData.Damage * player.DamageMultiplier));
                    }
                }
                // Set cooldown
                player.SetShootCooldownFromAttackSpeed(weaponData.AttackSpeed);
                return;
            }
            switch (weaponData!.BulletType)
            {
                case BulletType.Generic:
                    scene.AddObject(new GenericBullet(player.GlobalPosition + player.CameraOffset, direction, weaponData.Velocity, weaponData.Range, weaponData.Damage * player.DamageMultiplier, player, scene.HitboxManager));
                    break;
                case BulletType.Shotgun:
                    const int pelletCount = 12;
                    const float angleSpread = 20f;
                    float angleSpreadRad = MathHelper.ToRadians(angleSpread);
                    float angleStep = angleSpreadRad / (pelletCount - 1);
                    float startAngle = -angleSpreadRad / 2f;
                    
                    for (int i = 0; i < pelletCount; i++)
                    {
                        float jitter = MathF.Sin(i * 12.9898f) * 0.002f; // Deterministic jitter
                        float angleOffset = startAngle + angleStep * i + jitter;
                        Vector2 pelletDirection = Vector2.Rotate(direction, angleOffset);
                        
                        scene.AddObject(new ShotgunBullet(
                            player.GlobalPosition + player.CameraOffset, 
                            pelletDirection, 
                            weaponData.Velocity, 
                            weaponData.Range, 
                            weaponData.Damage * player.DamageMultiplier, 
                            player,
                            scene.HitboxManager
                        ));
                    }
                    break;
                case BulletType.FireworkRocket:
                    scene.AddObject(new FireworkRocket(player.GlobalPosition + player.CameraOffset, direction, weaponData.Velocity, weaponData.Range, weaponData.Damage * player.DamageMultiplier, player, scene.HitboxManager));
                    break;
                case BulletType.Arrow:
                    SoundEffect arrowSound = ExtendedGame.AssetManager.LoadSoundEffect("ArrowShot", "SFX/");
                    arrowSound.Play(volume: 0.1f, pitch: 0.7f, pan: 0.0f);
                    scene.AddObject(new Arrow(player.GlobalPosition + player.CameraOffset, direction, weaponData.Velocity, weaponData.Range, weaponData.Damage * player.DamageMultiplier, player, scene.HitboxManager));
                    break;
                default:
                    break;
            }
            
            // ===== SET COOLDOWN =====
            player.SetShootCooldownFromAttackSpeed(weaponData.AttackSpeed);
        });
    }
}


public static class ItemActionRegistry
{
    private static readonly Dictionary<string, Action<Player, ItemData, Vector2>> _use = new();

    public static void RegisterUse(string id, Action<Player, ItemData, Vector2> action) => _use[id] = action;

    public static Action<Player, ItemData, Vector2> GetUse(string? id) =>
        (id != null && _use.TryGetValue(id, out var a)) ? a : null;
}