using System.Text.Json.Serialization;

namespace SDWFE.Objects.Inventory.Item;

public class WeaponData : ItemData
{
    public float Damage { get; set; } = 10f;
    public float AttackSpeed { get; set; } = 1f; // Shots per second
    public float Range { get; set; } = 100f;

    public WeaponData()
    {
        ItemType = ItemType.Weapon;
        MaxStackSize = 1;
    }
}

public class InventoryWeapon : InventoryItem
{
    [JsonIgnore] private ItemData? _weaponData = null;
    [JsonIgnore] public ItemData WeaponData => _weaponData ??= ItemDatabase.Instance.GetWeaponData(Name);
    
    public InventoryWeapon(string name) : base(name, 1)
    {
        
    }

    public override InventoryItem Clone()
    {
        return new InventoryWeapon(Name);
    }
}