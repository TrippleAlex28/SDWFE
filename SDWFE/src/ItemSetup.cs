using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE;

public static class ItemSetup
{
    public const string WOOD = "Wood";
    public const string IRON = "Iron";
    public const string HEALTH_POTION = "Health Potion";
    public const string SUPERIOR_HEALTH_POTION = "Superior Health Potion";

    public const string ASSAULT_RIFLE = "Assault Rifle";
    public const string SHOTGUN = "Shotgun";

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
            }
        },
        {
            IRON, new ItemData
            {
                Name = IRON,
                MaxStackSize = 16,
            }
        },
        {
            HEALTH_POTION, new ItemData
            {
                Name = HEALTH_POTION,
                MaxStackSize = 8,
                UseActionId = ACTION_HEAL,
            }
        },
        {
            SUPERIOR_HEALTH_POTION, new ItemData
            {
                Name = SUPERIOR_HEALTH_POTION,
                MaxStackSize = 4,
                UseActionId = ACTION_HEAL_SUPERIOR,
            }
        },
        {
            ASSAULT_RIFLE, new WeaponData
            {
                Name = ASSAULT_RIFLE,
                Damage = 15f,
                AttackSpeed = 5f,
                Range = 80f,
                UseActionId = ACTION_SHOOT,
            }
        },
        {
            SHOTGUN, new WeaponData
            {
                Name = SHOTGUN,
                Damage = 5f,
                AttackSpeed = 1f,
                Range = 20f,
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
            Console.WriteLine($"Shoot Player{player.OwningClientId}: {direction}");
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