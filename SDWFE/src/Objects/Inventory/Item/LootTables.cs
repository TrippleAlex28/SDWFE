using System;
using System.Collections.Generic;
using Engine;

namespace SDWFE.Objects.Inventory.Item;

public class LootTableEntry
{
    public string ItemName;
    public int MinStackSize;
    public int MaxStackSize;
}

public static class LootTables
{
    public static List<KeyValuePair<LootTableEntry?, int>> GruntLootTable = new()
    {
        new KeyValuePair<LootTableEntry?, int>(
            null,
            50
        ),
        new KeyValuePair<LootTableEntry?, int>(
            new LootTableEntry
            {
                ItemName = ItemSetup.BANDAGE,
                MinStackSize = 1,
                MaxStackSize = 3
            },
            40
        ),
        new KeyValuePair<LootTableEntry?, int>(
            new LootTableEntry
            {
                ItemName = ItemSetup.MEDKIT,
                MinStackSize = 1,
                MaxStackSize = 1
            },
            10
        ),
    };

    public static InventoryItem? RollLootTable(List<KeyValuePair<LootTableEntry?, int>> lootTable)
    {
        if (lootTable == null || lootTable.Count <= 0) throw new ArgumentNullException(nameof(lootTable));
        
        // Calculate total weight
        int totalWeight = 0;
        for (int i = 0; i < lootTable.Count; i++)
        {
            int weight = lootTable[i].Value;
            if (weight <= 0)
                continue;
            
            totalWeight += weight;
        }
        
        if (totalWeight <= 0)
            throw new InvalidOperationException("Loot table has no valid entries (non-null with weight > 0).");

        int roll = ExtendedGame.Random.Next(totalWeight);

        // Pick entry
        foreach (var pair in lootTable)
        {
            int weight = pair.Value;
            if (weight <= 0)
                continue;

            if (roll < weight)
            {
                // NULL RESULT → no loot
                if (pair.Key == null)
                    return null;

                var chosen = pair.Key;

                // Validate entry
                if (string.IsNullOrWhiteSpace(chosen.ItemName))
                    throw new InvalidOperationException("LootTableEntry has empty ItemName.");

                int min = Math.Max(1, chosen.MinStackSize);
                int max = Math.Max(min, chosen.MaxStackSize);

                int amount = ExtendedGame.Random.Next(min, max + 1);
                return PlayerInventory.CreateItem(chosen.ItemName, amount);
            }

            roll -= weight;
        }

        // Should never happen
        throw new InvalidOperationException("Loot roll failed unexpectedly.");
    }
}