using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SDWFE.Objects.Inventory.Item;

public class ItemDatabase
{
    private static ItemDatabase? _instance;
    public static ItemDatabase Instance => _instance ??= new ItemDatabase();

    private Dictionary<string, ItemData> _itemDataMap = new();
    private string _databasePath  = "Data/item_database.json";

    public bool LoadDatabase()
    {
        try
        {
            if (!File.Exists(_databasePath))
            {
                Console.WriteLine($"ItemDatabase.LoadDatabase: database not found, creating default database");
                CreateDefaultDatabase();
                SaveDatabase();
                return true;
            }

            // Load the database
            string json = File.ReadAllText(_databasePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            // Deserialize a list of ItemData's from the database
            var itemDataList = JsonSerializer.Deserialize<List<ItemData>>(json, options);
            if (itemDataList == null)
            {
                Console.WriteLine("ItemDatabase.LoadDatabase: Failed to deserialize item database.");
                return false;
            }
            
            // Load ItemData into map by item name
            _itemDataMap.Clear();
            foreach (var itemData in itemDataList)
            {
                // Convert to WeaponData if item type is weapon
                if (itemData.ItemType == ItemType.Weapon && itemData is not WeaponData)
                {
                    var weaponData = new WeaponData
                    {
                        Name = itemData.Name,
                        IconPath = itemData.IconPath,
                        UseActionId = itemData.UseActionId
                    };
                    
                    // Try to extract weapon properties if they exist
                    var element = JsonSerializer.SerializeToElement(itemData);
                    if (element.TryGetProperty("Damage", out var damage))
                        weaponData.Damage = damage.GetSingle();
                    if (element.TryGetProperty("AttackSpeed", out var attackSpeed))
                        weaponData.AttackSpeed = attackSpeed.GetSingle();
                    if (element.TryGetProperty("Range", out var range))
                        weaponData.Range = range.GetSingle();

                    _itemDataMap[itemData.Name] = weaponData;
                }
                else
                {
                    _itemDataMap[itemData.Name] = itemData;
                }
            }

            Console.WriteLine($"ItemDatabase.LoadDatabase: Successfully loaded {_itemDataMap.Count} items from the database");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ItemDatabase.LoadDatabase: Failed to load item database: {ex.Message}");
            return false;
        }
    }

    public bool SaveDatabase()
    {
        try
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var itemDataList = _itemDataMap.Values.ToList();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            string json = JsonSerializer.Serialize(itemDataList, options);
            File.WriteAllText(_databasePath, json);
            
            Console.WriteLine($"Saved {itemDataList.Count} items to database");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save item database: {ex.Message}");
            return false;
        }
    }
    
    private void CreateDefaultDatabase()
    {
        _itemDataMap = ItemSetup.ItemDataMap;
    }

    public ItemData GetItemData(string itemName)
    {
        if (_itemDataMap.TryGetValue(itemName, out var data))
            return data;

        throw new Exception($"No ItemData found in the database for an item with name={itemName}");
    }

    public WeaponData GetWeaponData(string weaponName)
    {
        return GetItemData(weaponName) as WeaponData;
    }
}