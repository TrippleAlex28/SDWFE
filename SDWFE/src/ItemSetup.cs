using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Engine;
using Engine.Input;
using Engine.Scene;
using Microsoft.Xna.Framework;
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
            }
        },
        {
            MEDKIT, new ItemData
            {
                Name = MEDKIT,
                MaxStackSize = 4,
                IconPath = "Medkit",
                UseActionId = MEDKIT,
            }
        },
        {
            ADRENALINE, new ItemData
            {
                Name = ADRENALINE,
                MaxStackSize = 2,
                IconPath = "Adrenaline",
                UseActionId = ADRENALINE,
            }
        },
        {
            FREEZE, new ItemData
            {
                Name = FREEZE,
                MaxStackSize = 2,
                IconPath = "Freeze",
                UseActionId = FREEZE,
            }
        },
        {
            RAGE, new ItemData
            {
                Name = RAGE,
                MaxStackSize = 2,
                IconPath = "Rage",
                UseActionId = RAGE,
            }
        },
        {
            SLAM, new ItemData
            {
                Name = SLAM,
                MaxStackSize = 2,
                IconPath = "Slam",
                UseActionId = SLAM,
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
                IconPath = "Medkit",
                UseActionId = ACTION_SHOOT,
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
                IconPath = "Medkit",
                UseActionId = ACTION_SHOOT,
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
                IconPath = "Medkit",
                UseActionId = ACTION_SHOOT,
            }
        },
    };

    public static void Initialize()
    {
        ItemActionRegistry.RegisterUse(BANDAGE, (player, data, direction) =>
        {
            player.Stats.CurrentHealth += 100;
        });
        ItemActionRegistry.RegisterUse(MEDKIT, (player, data, direction) =>
        {
            player.Stats.CurrentHealth += 250;
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
            float bestDistance = float.MaxValue;
            foreach (var enemy in enemies)
            {
                if (best == null)
                {
                    best = enemy;
                    return;
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