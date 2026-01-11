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
}

public class PlayerStats
{
    // Set max values
    private float _maxHealth = 500f;
    public float MaxHealth
    {
        get => _maxHealth;
        set => SetField(ref _maxHealth, value);
    }
    private float _maxStamina = 100f;
    public float MaxStamina
    {
        get => _maxStamina;
        set => SetField(ref _maxStamina, value);
    }

    // Current values
    private float _currentHealth = 500f;
    public float CurrentHealth
    {
        get => _currentHealth;
        set => SetField(ref _currentHealth, MathHelper.Clamp(value, 0f, MaxHealth));
    }
    private float _currentStamina = 100f;
    public float CurrentStamina
    {
        get => _currentStamina;
        set => SetField(ref _currentStamina, MathHelper.Clamp(value, 0f, MaxStamina));
    }

    // Other stats
    private int _coins = 0;
    public int Coins
    {
        get => _coins;
        set => SetField(ref _coins, value);
    }

    public event Action? OnStatsChanged;

    private void SetField<T>(ref T field, T value){
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnStatsChanged?.Invoke();
    }
    
    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        Coins = 0;
    }
}
