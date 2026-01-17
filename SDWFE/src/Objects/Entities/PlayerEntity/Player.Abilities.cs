using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Input;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Inventory.Ability;

#nullable enable

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    /// <summary>
    /// Abilities owned by this player, indexed by slot (0-4)
    /// </summary>
    private AbilityInstance?[] _abilities = new AbilityInstance?[5];
    
    /// <summary>
    /// Set of unlocked ability names
    /// </summary>
    private HashSet<string> _unlockedAbilities = new();
    
    /// <summary>
    /// Currently selected ability slot
    /// </summary>
    public int SelectedAbilitySlot { get; private set; } = 0;
    
    /// <summary>
    /// Event fired when abilities change
    /// </summary>
    public event Action? OnAbilitiesChanged;
    
    /// <summary>
    /// Gets the ability at the specified slot
    /// </summary>
    public AbilityInstance? GetAbility(int slot)
    {
        if (slot < 0 || slot >= _abilities.Length)
            return null;
        return _abilities[slot];
    }
    
    /// <summary>
    /// Gets all ability instances
    /// </summary>
    public AbilityInstance?[] GetAllAbilities() => _abilities;
    
    /// <summary>
    /// Checks if an ability is unlocked
    /// </summary>
    public bool HasAbility(string abilityName) => _unlockedAbilities.Contains(abilityName);
    
    /// <summary>
    /// Gets the set of unlocked ability names
    /// </summary>
    public HashSet<string> GetUnlockedAbilities() => _unlockedAbilities;
    
    /// <summary>
    /// Unlocks an ability and assigns it to its designated slot
    /// </summary>
    public void UnlockAbility(string abilityName)
    {
        if (_unlockedAbilities.Contains(abilityName))
            return;
            
        var data = AbilityRegistry.GetAbility(abilityName);
        if (data == null)
        {
            Console.WriteLine($"AbilityRegistry: Unknown ability '{abilityName}'");
            return;
        }
        
        // Find the slot index for this ability
        int slotIndex = Array.IndexOf(AbilityRegistry.ShopAbilities, abilityName);
        if (slotIndex < 0 || slotIndex >= _abilities.Length)
        {
            Console.WriteLine($"AbilityRegistry: No slot for ability '{abilityName}'");
            return;
        }
        
        _unlockedAbilities.Add(abilityName);
        _abilities[slotIndex] = new AbilityInstance(data);
        
        // Sync with inventory for saving
        SyncAbilitiesToInventory();
        
        Console.WriteLine($"Player {OwningClientId} unlocked ability: {abilityName} in slot {slotIndex}");
        OnAbilitiesChanged?.Invoke();
    }
    
    /// <summary>
    /// Uses the ability at the specified slot
    /// </summary>
    public bool UseAbility(int slot)
    {
        var ability = GetAbility(slot);
        if (ability == null)
        {
            Console.WriteLine($"No ability in slot {slot}");
            return false;
        }
        
        return ability.TryUse(this);
    }
    
    /// <summary>
    /// Uses the currently selected ability
    /// </summary>
    public bool UseSelectedAbility()
    {
        return UseAbility(SelectedAbilitySlot);
    }
    
    /// <summary>
    /// Selects the ability slot
    /// </summary>
    public void SelectAbilitySlot(int slot)
    {
        if (slot >= 0 && slot < _abilities.Length)
        {
            SelectedAbilitySlot = slot;
        }
    }
    
    /// <summary>
    /// Initializes ability system
    /// </summary>
    private void ConstructAbilities()
    {
        // Hook up shop purchase event
        if (_shopUI != null)
        {
            _shopUI.OnItemPurchased += OnShopItemPurchased;
        }
        
        // Subscribe to inventory load event for future loads
        Inventory.OnAbilitiesLoaded += OnAbilitiesLoaded;
        
        // Load any abilities that were already loaded from the save file
        // (since PlayerInventory.EnterSelf loads before ConstructAbilities is called)
        if (Inventory.UnlockedAbilities != null && Inventory.UnlockedAbilities.Length > 0)
        {
            OnAbilitiesLoaded(Inventory.UnlockedAbilities);
        }
    }
    
    /// <summary>
    /// Called when abilities are loaded from save file
    /// </summary>
    private void OnAbilitiesLoaded(string[] abilityNames)
    {
        foreach (var abilityName in abilityNames)
        {
            // Directly unlock without triggering the sync back to inventory
            var data = AbilityRegistry.GetAbility(abilityName);
            if (data == null) continue;
            
            int slotIndex = Array.IndexOf(AbilityRegistry.ShopAbilities, abilityName);
            if (slotIndex < 0 || slotIndex >= _abilities.Length) continue;
            
            _unlockedAbilities.Add(abilityName);
            _abilities[slotIndex] = new AbilityInstance(data);
        }
        
        Console.WriteLine($"Loaded {abilityNames.Length} abilities from save");
        OnAbilitiesChanged?.Invoke();
    }
    
    /// <summary>
    /// Syncs unlocked abilities to inventory for saving
    /// </summary>
    private void SyncAbilitiesToInventory()
    {
        Inventory.UnlockedAbilities = _unlockedAbilities.ToArray();
    }
    
    /// <summary>
    /// Called when an item is purchased from the shop
    /// </summary>
    private void OnShopItemPurchased(string itemName, int price)
    {
        // Try to unlock as ability
        UnlockAbility(itemName);
        
        // Deduct coins
        Stats.Coins -= price;
    }
    
    /// <summary>
    /// Updates abilities (cooldowns and input)
    /// </summary>
    private void UpdateAbilities(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Update all ability cooldowns
        foreach (var ability in _abilities)
        {
            ability?.Update(deltaTime);
        }
        
        // Handle ability input - use ACTION_USE to trigger selected ability
        var input = InputManager.Instance;
        
        if (input.IsActionPressed(InputSetup.ACTION_USE))
        {
            // Use the currently selected ability
            UseAbility(SelectedAbilitySlot);
        }
    }
}
