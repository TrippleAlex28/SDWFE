using System.Collections.Generic;
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
            }
        },
        {
            SUPERIOR_HEALTH_POTION, new ItemData
            {
                Name = SUPERIOR_HEALTH_POTION,
                MaxStackSize = 4,
            }
        },
        {
            ASSAULT_RIFLE, new WeaponData
            {
                Name = ASSAULT_RIFLE,
                Damage = 15f,
                AttackSpeed = 5f,
                Range = 80f,
                MagSize = 30,
            }
        },
        {
            SHOTGUN, new WeaponData
            {
                Name = SHOTGUN,
                Damage = 5f,
                AttackSpeed = 1f,
                Range = 20f,
                MagSize = 6,
            }
        },
    };
}