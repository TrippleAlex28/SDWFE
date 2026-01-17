using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using Engine;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Objects.Inventory;

/// <summary>
/// Inventory save data structure
/// </summary>
public class PlayerInventoryData
{
    public InventorySlot[] Hotbar { get; set; } = Array.Empty<InventorySlot>();
    public InventorySlot[] Inventory { get; set; } = Array.Empty<InventorySlot>();
    public InventorySlot[] WeaponSlots { get; set; } = Array.Empty<InventorySlot>();
    public InventorySlot[] Vault { get; set; } = Array.Empty<InventorySlot>();
    public string[] UnlockedAbilities { get; set; } = Array.Empty<string>();
    
    // Player stats
    public float CurrentHealth { get; set; } = 500f;
    public float MaxHealth { get; set; } = 500f;
    public float CurrentStamina { get; set; } = 100f;
    public float MaxStamina { get; set; } = 100f;
    public int Coins { get; set; } = 1000;
}

public class PlayerInventory : GameObject
{
    private const int HOTBAR_SIZE = 5;
    private const int INVENTORY_SIZE = 10;
    private const int WEAPON_SLOTS = 2;
    
    public InventorySlot[] Hotbar { get; private set; }
    public InventorySlot[] Inventory { get; private set; }
    public InventorySlot[] WeaponSlots { get; private set; }
    
    public PlayerVault Vault { get; private set; }

    public int SelectedHotbarIndex { get; private set; } = 0;
    public int SelectedWeaponIndex { get; private set; } = 0;
    
    /// <summary>
    /// Unlocked abilities to be saved/loaded
    /// </summary>
    public string[] UnlockedAbilities { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Player stats to be saved/loaded
    /// </summary>
    public float SavedCurrentHealth { get; set; } = 500f;
    public float SavedMaxHealth { get; set; } = 500f;
    public float SavedCurrentStamina { get; set; } = 100f;
    public float SavedMaxStamina { get; set; } = 100f;
    public int SavedCoins { get; set; } = 1000;
    
    /// <summary>
    /// Event fired when abilities are loaded from file
    /// </summary>
    public event Action<string[]>? OnAbilitiesLoaded;
    
    /// <summary>
    /// Event fired when stats are loaded from file
    /// </summary>
    public event Action? OnStatsLoaded;
    
    public event Action? OnInventoryChanged;
    public event Action<int>? OnHotbarSelectionChanged;
    public event Action<int>? OnWeaponSelectionChanged;

    public PlayerInventory()
    {
        // Initialize Slots
        Hotbar = new InventorySlot[HOTBAR_SIZE];
        Inventory = new InventorySlot[INVENTORY_SIZE];
        WeaponSlots = new InventorySlot[WEAPON_SLOTS];

        for (int i = 0; i < HOTBAR_SIZE; i++)
            Hotbar[i] = new InventorySlot();
        for (int i = 0; i < INVENTORY_SIZE; i++)
            Inventory[i] = new InventorySlot();
        for (int i = 0; i < WEAPON_SLOTS; i++)
            WeaponSlots[i] = new InventorySlot();
        
        // Initialize Vault
        Vault = new PlayerVault();
    }
    
    #region Creation Helpers

    public static InventoryItem CreateItem(string itemName, int amount = 1)
    {
        var itemData = ItemDatabase.Instance.GetItemData(itemName);

        return itemData.ItemType switch
        {
            ItemType.Weapon => new InventoryWeapon(itemName),
            _ => new InventoryItem(itemName, amount),
        };
    }

    public static InventoryWeapon CreateWeapon(string weaponName)
    {
        return new InventoryWeapon(weaponName);
    }
    
    #endregion
    
    #region Vault Management

    public bool CanAccessVault()
    {
        return Vault.IsAccessible;
    }

    /// <summary>
    /// Adds an item to the vault if accessible, returns success
    /// </summary>
    public bool AddItemToVault(InventoryItem item)
    {
        if (!Vault.IsAccessible)
            return false;

        int remainingAmount = item.StackSize;
        
        // Try to add to existing stacks
        remainingAmount = TryAddToExistingStacks(Vault.Slots, item, remainingAmount);
        if (remainingAmount <= 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Try to add to empty slots
        remainingAmount = TryAddToEmptySlots(Vault.Slots, item, remainingAmount);

        OnInventoryChanged?.Invoke();
        return remainingAmount <= 0;
    }

    /// <summary>
    /// Removes an item from the vault if accessible, returns success
    /// </summary>
    public InventoryItem? RemoveItemFromVault(string itemName, int amount = 1)
    {
        if (!Vault.IsAccessible)
            return null;

        InventoryItem? removed = TryRemoveFromSlots(Vault.Slots, itemName, amount);
        OnInventoryChanged?.Invoke();
        return removed;
    }

    /// <summary>
    /// Transfers an item from inventory to vault, returns success
    /// </summary>
    public bool TransferToVault(string itemName, int amount = 1)
    {
        if (!Vault.IsAccessible)
            return false;

        InventoryItem? item = RemoveItem(itemName, amount);
        if (item != null)
        {
            if (AddItemToVault(item))
                return true;
            
            // If vault is full, add back to inventory
            AddItem(item);
        }

        return false;
    }

    /// <summary>
    /// Transfers an item from vault to inventory
    /// </summary>
    public bool TransferFromVault(string itemName, int amount = 1)
    {
        if (!Vault.IsAccessible)
            return false;

        InventoryItem? item = RemoveItemFromVault(itemName, amount);
        if (item != null)
        {
            if (AddItem(item))
                return true;
            
            // If inventory is full, add back to vault
            AddItemToVault(item);
        }

        return false;
    }
    
    #endregion 
    
    #region Inventory Management

    /// <summary>
    /// Adds an item to the inventory. Returns true if successful, false if inventory full
    /// </summary>
    public bool AddItem(InventoryItem item)
    {
        int remainingAmount = item.StackSize;

        // Try adding to existing stacks in hotbar
        remainingAmount = TryAddToExistingStacks(Hotbar, item, remainingAmount);
        if (remainingAmount <= 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Try adding to existing stacks in inventory 
        remainingAmount = TryAddToExistingStacks(Inventory, item, remainingAmount);
        if (remainingAmount <= 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        
        // Try adding to new hotbar stacks
        remainingAmount = TryAddToEmptySlots(Hotbar, item, remainingAmount);
        if (remainingAmount <= 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Try adding to new inventory stacks
        remainingAmount = TryAddToEmptySlots(Inventory, item, remainingAmount);

        OnInventoryChanged?.Invoke();
        return remainingAmount <= 0;
    }

    /// <summary>
    /// Adds an item by name from the database
    /// </summary>
    public bool AddItemByName(string itemName, int amount = 1)
    {
        return AddItem(CreateItem(itemName, amount));
    }

    /// <summary>
    /// Adds a weapon to the first available weapon slot
    /// </summary>
    public bool AddWeapon(InventoryWeapon weapon)
    {
        for (int i = 0; i < WEAPON_SLOTS; i++)
        {
            if (WeaponSlots[i].IsEmpty())
            {
                WeaponSlots[i].AddItem(weapon);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a weapon by name from the database
    /// </summary>
    public bool AddWeaponByName(string weaponName)
    {
        return AddWeapon(CreateWeapon(weaponName));
    }

    /// <summary>
    /// Removes an item from the inventory. Returns the removed item or null if not found
    /// </summary>
    public InventoryItem? RemoveItem(string itemName, int amount = 1)
    {
        // Remove from hotbar
        InventoryItem? removed = TryRemoveFromSlots(Hotbar, itemName, amount);
        if (removed != null && removed.StackSize >= amount)
        {
            OnInventoryChanged?.Invoke();
            return removed;
        }

        // Remove from inventory if necessary
        int remainingToRemove = amount - (removed?.StackSize ?? 0);
        InventoryItem? additionalRemoved = TryRemoveFromSlots(Inventory, itemName, remainingToRemove);

        if (additionalRemoved != null)
        {
            if (removed != null)
                removed.StackSize += additionalRemoved.StackSize;
            else
                removed = additionalRemoved;
        }

        OnInventoryChanged?.Invoke();
        return removed;
    }

    /// <summary>
    /// Gets currently selected hotbar item
    /// </summary>
    public InventoryItem? GetSelectedItem()
    {
        if (SelectedHotbarIndex >= 0 && SelectedHotbarIndex < HOTBAR_SIZE)
            return Hotbar[SelectedHotbarIndex].Item;
        return null;
    }

    /// <summary>
    /// Gets the equipped weapon at the specified slot
    /// </summary>
    public InventoryWeapon? GetEquippedWeapon()
    {
        if (SelectedWeaponIndex >= 0 && SelectedWeaponIndex < WEAPON_SLOTS)
            return WeaponSlots[SelectedWeaponIndex].Item as InventoryWeapon;
        return null;
    }

    public void SelectHotbarSlot(int index)
    {
        if (index >= 0 && index < HOTBAR_SIZE)
        {
            SelectedHotbarIndex = index;
            OnHotbarSelectionChanged?.Invoke(index);
        }
    }

    public void SelectWeaponSlot(int index)
    {
        if (index >= 0 && index < WEAPON_SLOTS)
        {
            SelectedWeaponIndex = index;
            OnWeaponSelectionChanged?.Invoke(index);
        }
    }
    
    /// <summary>
    /// Swaps items between two slots in the same or different slot array
    /// </summary>
    public void SwapSlots(InventorySlot slot1, InventorySlot slot2)
    {
        InventoryItem? temp = slot1.Item;
        slot1.Item = slot2.Item;
        slot2.Item = temp;
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Checks if the inventory contains a specific item with at least the specified amount
    /// </summary>
    public bool HasItem(string itemName, int amount = 1)
    {
        int totalCount = CountItem(itemName);
        return totalCount >= amount;
    }

    /// <summary>
    /// Counts the total number of a specific item across all slots
    /// </summary>
    public int CountItem(string itemName)
    {
        int count = 0;
        
        foreach (var slot in Hotbar)
            if (!slot.IsEmpty() && slot.Item!.Name == itemName)
                count += slot.Item.StackSize;
        
        foreach (var slot in Inventory)
            if (!slot.IsEmpty() && slot.Item!.Name == itemName)
                count += slot.Item.StackSize;
        
        return count;
    }
    
    #endregion 
    
    #region Saving & Loading

    /// <summary>
    /// Saves the inventory & vault to a JSON file
    /// </summary>
    public bool SaveToFile(string filePath)
    {
        try
        {
            var saveData = new PlayerInventoryData
            {
                Hotbar = Hotbar,
                Inventory = Inventory,
                WeaponSlots = WeaponSlots,
                Vault = Vault.Slots,
                UnlockedAbilities = UnlockedAbilities,
                CurrentHealth = SavedCurrentHealth,
                MaxHealth = SavedMaxHealth,
                CurrentStamina = SavedCurrentStamina,
                MaxStamina = SavedMaxStamina,
                Coins = SavedCoins,
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = false,
            };
            
            string json = JsonSerializer.Serialize(saveData, options);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerInventory.SaveToFile Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads the inventory & vault from a JSON file
    /// </summary>
    public bool LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Inventory save file not found: {filePath}");
                return false;
            }

            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                IncludeFields = false,
            };

            var saveData = JsonSerializer.Deserialize<PlayerInventoryData>(json, options);
            if (saveData == null)
            {
                Console.WriteLine($"Failed to deserialize inventory save data");
                return false;
            }
            
            // Load data into inventory
            if (saveData.Hotbar.Length == HOTBAR_SIZE)
                Hotbar = saveData.Hotbar;
            if (saveData.Inventory.Length == INVENTORY_SIZE)
                Inventory = saveData.Inventory;
            if (saveData.WeaponSlots.Length == WEAPON_SLOTS)
                WeaponSlots = saveData.WeaponSlots;
            if (saveData.Vault.Length == 60)
                Vault.Slots = saveData.Vault;
            
            // Load unlocked abilities
            if (saveData.UnlockedAbilities != null && saveData.UnlockedAbilities.Length > 0)
            {
                UnlockedAbilities = saveData.UnlockedAbilities;
                OnAbilitiesLoaded?.Invoke(saveData.UnlockedAbilities);
            }
            
            // Load player stats
            SavedCurrentHealth = saveData.CurrentHealth;
            SavedMaxHealth = saveData.MaxHealth;
            SavedCurrentStamina = saveData.CurrentStamina;
            SavedMaxStamina = saveData.MaxStamina;
            SavedCoins = saveData.Coins;
            OnStatsLoaded?.Invoke();

            OnInventoryChanged?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerInventory.LoadFromFile Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Auto-saves the inventory periodically.
    /// </summary>
    public void AutoSaveUpdate(GameTime gameTime, float intervalSeconds = 120f)
    {
        _autoSaveTimer += gameTime.DeltaSeconds();
        if (_autoSaveTimer >= intervalSeconds)
        {
            Console.WriteLine("Autosave Player Inventory");
            SaveToFile(GetDefaultSavePath(ExtendedGame.GAME_NAME));
            _autoSaveTimer = 0f;
        }
    }
    private float _autoSaveTimer = 0f;

    public static string GetDefaultSavePath(string gameName)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string gameFolder = Path.Combine(appData, gameName);
        return Path.Combine(gameFolder, "inventory_save.json");
    }
    
    #endregion

    #region Helper Methods

    private int TryAddToExistingStacks(InventorySlot[] slots, InventoryItem item, int amount)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.Item!.CanStack(item))
            {
                amount = slot.AddItem(item);
                if (amount <= 0) break;
            }
        }
        return amount;
    }
    
    private int TryAddToEmptySlots(InventorySlot[] slots, InventoryItem item, int amount)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                amount = slot.AddItem(item);
                if (amount <= 0) break;
            }
        }
        return amount;
    }

    private InventoryItem? TryRemoveFromSlots(InventorySlot[] slots, string itemName, int amount)
    {
        InventoryItem? result = null;
        int remainingToRemove = amount;

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.Item!.Name == itemName)
            {
                InventoryItem? removed = slot.RemoveItem(remainingToRemove);
                if (removed != null)
                {
                    if (result == null)
                        result = removed;
                    else
                        result.StackSize += removed.StackSize;

                    remainingToRemove -= removed.StackSize;
                    if (remainingToRemove <= 0) break;
                }
            }
        }

        return result;
    }
    
    #endregion 
    
    #region GameObject Overrides

    protected override void EnterSelf()
    {
        base.EnterSelf();

        LoadFromFile(GetDefaultSavePath(ExtendedGame.GAME_NAME));
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        AutoSaveUpdate(gameTime);
    }

    protected override void ExitSelf()
    {
        base.ExitSelf();

        SaveToFile(GetDefaultSavePath(ExtendedGame.GAME_NAME));
    }

    #endregion
}
