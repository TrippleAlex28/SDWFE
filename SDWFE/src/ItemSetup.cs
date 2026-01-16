using System;
using System.Collections.Generic;
using Engine;
using Engine.Scene;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;
using SDWFE.Objects.Projectiles.Bullets;

namespace SDWFE;

public static class ItemSetup
{
    public const string WOOD = "Wood";
    public const string IRON = "Iron";
    public const string BANDAGE = "Bandage";
    public const string MEDKIT = "Medkit";

    public const string PISTOL = "Pistol";
    public const string ASSAULT_RIFLE = "Assault Rifle";
    public const string SHOTGUN = "Shotgun";
    public const string FIREWORK_LAUNCHER = "Firework Launcher";

    public const string ACTION_HEAL = "Heal";
    public const string ACTION_HEAL_SUPERIOR = "HealSuperior";
    public const string ACTION_SHOOT = "Shoot";
    
    public static readonly Dictionary<string, ItemData> ItemDataMap = new()
    {
        {
            WOOD, new ItemData
            {
                Name = WOOD,
                MaxStackSize = 64,
                IconPath = "Medkit",
            }
        },
        {
            IRON, new ItemData
            {
                Name = IRON,
                MaxStackSize = 16,
                IconPath = "Medkit",
            }
        },
        {
            BANDAGE, new ItemData
            {
                Name = BANDAGE,
                MaxStackSize = 8,
                IconPath = "Medkit",
                UseActionId = ACTION_HEAL,
            }
        },
        {
            MEDKIT, new ItemData
            {
                Name = MEDKIT,
                MaxStackSize = 4,
                IconPath = "Medkit",
                UseActionId = ACTION_HEAL_SUPERIOR,
            }
        },
        {
            PISTOL, new WeaponData
            {
                Name = PISTOL,
                Damage = 50f,
                AttackSpeed = 5f,
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
                Damage = 30f,
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
                Damage = 500f,
                AttackSpeed = 1f,
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
        ItemActionRegistry.RegisterUse(ACTION_HEAL, (player, data, direction) =>
        {
            Console.WriteLine($"Heal Player{player.OwningClientId}: {direction}");
        });
        ItemActionRegistry.RegisterUse(ACTION_HEAL_SUPERIOR, (player, data, direction) =>
        {
            Console.WriteLine($"Heal Superior Player{player.OwningClientId}: {direction}");
        });
        ItemActionRegistry.RegisterUse(ACTION_SHOOT, (player, data, direction) =>
        {
            var weaponData = data as WeaponData;
            
            Scene scene = GameState.Instance.CurrentScene;
            if (scene == null) return;
            
            switch (weaponData!.BulletType)
            {
                case BulletType.Generic:
                    scene.AddObject(new GenericBullet(player.GlobalPosition + player.CameraOffset, direction, weaponData.Velocity, weaponData.Range, weaponData.Damage, player, scene.HitboxManager));
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
                            weaponData.Damage, 
                            player,
                            scene.HitboxManager
                        ));
                    }
                    break;
                case BulletType.FireworkRocket:
                    scene.AddObject(new FireworkRocket(player.GlobalPosition + player.CameraOffset, direction, weaponData.Velocity, weaponData.Range, weaponData.Damage, player, scene.HitboxManager));
                    break;
                default:
                    break;
            }
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