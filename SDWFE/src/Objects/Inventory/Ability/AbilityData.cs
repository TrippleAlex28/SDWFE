using System;
using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;

#nullable enable

namespace SDWFE.Objects.Inventory.Ability;

/// <summary>
/// Defines an ability's static data
/// </summary>
public class AbilityData
{
    public string Name { get; set; } = "Ability";
    public string Description { get; set; } = "";
    public string IconPath { get; set; } = "Heal";
    public float Cooldown { get; set; } = 5f;
    public int Price { get; set; } = 100;
    
    private Texture2D? _icon;
    public Texture2D Icon => _icon ??= ExtendedGame.AssetManager.LoadTexture(IconPath, "Items/");
}

/// <summary>
/// Runtime instance of an ability owned by a player
/// </summary>
public class AbilityInstance
{
    public AbilityData Data { get; }
    public float RemainingCooldown { get; private set; }
    public bool IsOnCooldown => RemainingCooldown > 0;
    public float CooldownProgress => Data.Cooldown > 0 ? RemainingCooldown / Data.Cooldown : 0f;
    
    public AbilityInstance(AbilityData data)
    {
        Data = data;
        RemainingCooldown = 0f;
    }
    
    public void Update(float deltaTime)
    {
        if (RemainingCooldown > 0)
        {
            RemainingCooldown -= deltaTime;
            if (RemainingCooldown < 0)
                RemainingCooldown = 0;
        }
    }
    
    public bool TryUse(Player player)
    {
        if (IsOnCooldown)
            return false;
            
        var action = AbilityRegistry.GetAction(Data.Name);
        if (action == null)
            return false;
            
        action(player);
        RemainingCooldown = Data.Cooldown;
        player.OnUseItem();
        return true;
    }
}

/// <summary>
/// Registry for all available abilities
/// </summary>
public static class AbilityRegistry
{
    public const string HEAL = "Heal";
    public const string ADRENALINE = "Adrenaline";
    public const string FREEZE = "Freeze";
    public const string RAGE = "Rage";
    public const string SLAM = "Slam";
    
    private static readonly Dictionary<string, AbilityData> _abilities = new();
    private static readonly Dictionary<string, Action<Player>> _actions = new();
    
    public static IReadOnlyDictionary<string, AbilityData> Abilities => _abilities;
    
    /// <summary>
    /// List of abilities in shop order
    /// </summary>
    public static readonly string[] ShopAbilities = { HEAL, ADRENALINE, FREEZE, RAGE, SLAM };
    
    public static void Initialize()
    {
        // Register all abilities
        RegisterAbility(new AbilityData
        {
            Name = HEAL,
            Description = "Restore 25 health",
            IconPath = "Heal",
            Cooldown = 8f,
            Price = 100
        });
        
        RegisterAbility(new AbilityData
        {
            Name = ADRENALINE,
            Description = "Gain speed boost for 5 seconds",
            IconPath = "Adrenaline",
            Cooldown = 15f,
            Price = 150
        });
        
        RegisterAbility(new AbilityData
        {
            Name = FREEZE,
            Description = "Freeze nearby enemies for 3 seconds",
            IconPath = "Freeze",
            Cooldown = 20f,
            Price = 200
        });
        
        RegisterAbility(new AbilityData
        {
            Name = RAGE,
            Description = "Deal double damage for 5 seconds",
            IconPath = "Rage",
            Cooldown = 25f,
            Price = 250
        });
        
        RegisterAbility(new AbilityData
        {
            Name = SLAM,
            Description = "Damage all nearby enemies",
            IconPath = "Slam",
            Cooldown = 12f,
            Price = 300
        });
        
        // Register ability actions
        RegisterAction(HEAL, (player) =>
        {
            player.Stats.CurrentHealth = Math.Min(
                player.Stats.CurrentHealth + 25f,
                player.Stats.MaxHealth
            );
            Console.WriteLine($"Player {player.OwningClientId} used Heal! Health: {player.Stats.CurrentHealth}");
        });
        
        RegisterAction(ADRENALINE, (player) =>
        {
            // Speed boost effect - would need a buff system, for now just log
            Console.WriteLine($"Player {player.OwningClientId} used Adrenaline! Speed boosted!");
            // TODO: Implement speed buff
        });
        
        RegisterAction(FREEZE, (player) =>
        {
            Console.WriteLine($"Player {player.OwningClientId} used Freeze! Enemies frozen!");
            // TODO: Implement freeze effect on nearby enemies
        });
        
        RegisterAction(RAGE, (player) =>
        {
            Console.WriteLine($"Player {player.OwningClientId} used Rage! Damage doubled!");
            // TODO: Implement damage buff
        });
        
        RegisterAction(SLAM, (player) =>
        {
            Console.WriteLine($"Player {player.OwningClientId} used Slam! Area damage!");
            // TODO: Implement area damage
        });
    }
    
    public static void RegisterAbility(AbilityData data)
    {
        _abilities[data.Name] = data;
    }
    
    public static void RegisterAction(string name, Action<Player> action)
    {
        _actions[name] = action;
    }
    
    public static AbilityData? GetAbility(string name)
    {
        return _abilities.TryGetValue(name, out var data) ? data : null;
    }
    
    public static Action<Player>? GetAction(string name)
    {
        return _actions.TryGetValue(name, out var action) ? action : null;
    }
}
