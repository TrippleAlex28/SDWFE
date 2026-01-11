using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            options.Converters.Add(new JsonStringEnumConverter());

            var elements = JsonSerializer.Deserialize<List<JsonElement>>(json, options);
            if (elements == null) throw new Exception("Item Database is Empty");
            
            _itemDataMap.Clear();

            foreach (var el in elements)
            {
                ItemType itemType = ItemType.Item;
                if (el.TryGetProperty("ItemType", out var itemTypeEl))
                {
                    itemType = itemTypeEl.ValueKind switch
                    {
                        JsonValueKind.String => Enum.TryParse<ItemType>(itemTypeEl.GetString(), true, out var it)
                            ? it
                            : ItemType.Item,
                        JsonValueKind.Number => (ItemType)itemTypeEl.GetInt32(), 
                        _ => ItemType.Item,
                    };
                }

                ItemData data = itemType == ItemType.Weapon
                    ? el.Deserialize<WeaponData>(options)!
                    : el.Deserialize<ItemData>(options)!; 
                
                _itemDataMap[data.Name] = data;
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
            options.Converters.Add(new JsonStringEnumConverter());
            
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