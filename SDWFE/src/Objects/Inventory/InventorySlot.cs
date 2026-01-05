using System;
using System.Text.Json.Serialization;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Objects.Inventory;

/// <summary>
/// The data structure that gets serialized into the save file
/// </summary>
public class InventorySlotData
{
    public string Name { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemType ItemType { get; set; }
    
    public int StackSize { get; set; }
}

public class InventorySlot
{
    [JsonIgnore] public InventoryItem? Item { get; set; }
    
    // Serialization
    public InventorySlotData? SlotData
    {
        get
        {
            if (Item == null || Item.StackSize <= 0)
                return null;

            return new InventorySlotData
            {
                ItemType = Item.Data.ItemType,
                Name = Item.Name,
                StackSize = Item.StackSize,
            };
        }
        set
        {
            if (value == null || string.IsNullOrEmpty(value.Name) || value.StackSize <= 0)
            {
                Item = null;
                return;
            }

            Item = value.ItemType switch
            {
                ItemType.Weapon => new InventoryWeapon(value.Name),
                _ => new InventoryItem(value.Name, value.StackSize),
            };
        }
    }

    /// <summary>
    /// Returns remainder of the items that couldn't be added to inventory
    /// </summary>
    public int AddItem(InventoryItem item)
    {
        if (IsEmpty())
        {
            Item = item.Clone();
            Item!.StackSize = 0;
        }
        else
        {
            if (!Item!.CanStack(item)) return item.StackSize;
        }

        int spaceAvailable = HasRoom();
        int amountToAdd = Math.Min(spaceAvailable, item.StackSize);
        Item.StackSize += amountToAdd;

        return item.StackSize - amountToAdd;
    }

    public InventoryItem? RemoveItem(int amount)
    {
        if (IsEmpty()) return null;

        int amountToRemove = Math.Min(amount, Item!.StackSize);
        
        InventoryItem removed = Item.Clone();
        removed.StackSize = amountToRemove;
        
        Item.StackSize -= amountToRemove;
        if (Item.StackSize <= 0)
        {
            Item = null;
        }
        
        return removed;
    }

    public void Clear()
    {
        Item = null;
    }
    
    /// <summary>
    /// Returns whether this slot is holding a valid InventoryItem
    /// </summary>
    public bool IsEmpty()
    {
        return Item == null || Item.StackSize <= 0;
    }
    
    /// <summary>
    /// Returns the amount of room this slot has available, -1 if the slot is empty
    /// </summary>
    public int HasRoom()
    {
        return IsEmpty() ? 64 : Math.Max(0, Item!.Data.MaxStackSize - Item.StackSize);
    }
}