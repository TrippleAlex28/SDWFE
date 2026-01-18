using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SDWFE.UI.PlayerData;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public PlayerStats Stats = new PlayerStats();
    public UIStats StatsUI { get; set; }

    public Interactable? ClosestInteractable = null;
    public int ClosestInteractableDist = int.MaxValue;
    
    /// <summary>
    /// Initializes stats saving/loading
    /// </summary>
    private void ConstructStats()
    {
        // Subscribe to stats changes to sync to inventory for saving
        Stats.OnStatsChanged += SyncStatsToInventory;
        
        // Subscribe to inventory load event to restore stats
        Inventory.OnStatsLoaded += OnStatsLoaded;
        
        // Load stats if already loaded from save file
        if (Inventory.SavedCoins > 0 || Inventory.SavedCurrentHealth != 500f)
        {
            OnStatsLoaded();
        }
    }
    
    /// <summary>
    /// Called when stats are loaded from save file
    /// </summary>
    private void OnStatsLoaded()
    {
        Stats.MaxHealth = Inventory.SavedMaxHealth;
        Stats.CurrentHealth = Inventory.SavedCurrentHealth;
        Stats.MaxStamina = Inventory.SavedMaxStamina;
        Stats.CurrentStamina = Inventory.SavedCurrentStamina;
        Stats.Coins = Inventory.SavedCoins;
        Console.WriteLine($"Loaded stats from save: Health={Stats.CurrentHealth}/{Stats.MaxHealth}, Coins={Stats.Coins}");
    }
    
    /// <summary>
    /// Syncs stats to inventory for saving
    /// </summary>
    private void SyncStatsToInventory(StatType changedStat, bool decreased)
    {
        switch (changedStat)
        {
            case StatType.MaxHealth:
                Inventory.SavedMaxHealth = Stats.MaxHealth;
                break;
            case StatType.CurrentHealth:
                Inventory.SavedCurrentHealth = Stats.CurrentHealth;
                break;
            case StatType.MaxStamina:
                Inventory.SavedMaxStamina = Stats.MaxStamina;
                break;
            case StatType.CurrentStamina:
                Inventory.SavedCurrentStamina = Stats.CurrentStamina;
                break;
            case StatType.Coins:
                Inventory.SavedCoins = Stats.Coins;
                break;
        }
        Inventory.SaveToFile();
        Console.WriteLine("Synced stats to inventory for saving.");
        //Inventory.LoadFromFile(); // For debugging purposes
    }
}

public enum StatType
{
    MaxHealth,
    CurrentHealth,
    MaxStamina,
    CurrentStamina,
    Coins
}

public class PlayerStats
{
    public Action? OnDeath;
    
    // Set max values
    private float _maxHealth = 500f;
    public float MaxHealth
    {
        get => _maxHealth;
        set => SetField(ref _maxHealth, value, StatType.MaxHealth);
    }
    
    private float _maxStamina = 100f;
    public float MaxStamina
    {
        get => _maxStamina;
        set => SetField(ref _maxStamina, value, StatType.MaxStamina);
    }

    // Current values
    private float _currentHealth = 500f;
    public float CurrentHealth
    {
        get => _currentHealth;
        set
        {
            SetField(ref _currentHealth, MathHelper.Clamp(value, 0f, MaxHealth), StatType.CurrentHealth);
            if (CurrentHealth <= 0f) OnDeath?.Invoke();
        }
    }

    private float _currentStamina = 100f;
    public float CurrentStamina
    {
        get => _currentStamina;
        set => SetField(ref _currentStamina, MathHelper.Clamp(value, 0f, MaxStamina), StatType.CurrentStamina);
    }

    // Other stats
    private int _coins = 100;
    public int Coins
    {
        get => _coins;
        set => SetField(ref _coins, value, StatType.Coins);
    }

    public event Action<StatType, bool>? OnStatsChanged;

    private void SetField<T>(ref T field, T value, StatType statType)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        bool decreased = Comparer<T>.Default.Compare(field, value) > 0;
        field = value;
        OnStatsChanged?.Invoke(statType, decreased);
    }
    
    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        Coins = 0;
    }
}
