using System;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using Engine;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Objects.Inventory.Item;

public class ItemData
{
    public string Name { get; set; } = "Item";
    public string ItemType { get; set; } = "item"; // "item" or "weapon"
    public int MaxStackSize { get; set; } = 16;
    public string IconPath { get; set; } = ""; // TODO: Point to placeholder texture
    
    public string? UseActionId { get; set; }
}

public class InventoryItem
{
    public string Name { get; }
    public int StackSize { get; set; }

    [JsonIgnore] private ItemData? _data;
    [JsonIgnore] public ItemData Data => _data ??= ItemDatabase.Instance.GetItemData(Name);
    
    [JsonIgnore] public Texture2D Icon { get; }

    public InventoryItem(string name, int stackSize = 1)
    {
        Name = name;
        StackSize = stackSize;
        
        Icon = ExtendedGame.AssetManager.LoadTexture(Data.IconPath, "Items/"); 
    }

    public virtual InventoryItem Clone()
    {
        return new InventoryItem(Name, StackSize);;
    }
    
    /// <summary>
    /// Returns whether these items are compatible to stack
    /// </summary>
    public bool CanStack(InventoryItem other)
    {
        return other != null && this.GetType() == other.GetType() && this.Name == other.Name;
    }
}
