using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SDWFE.Objects.Inventory;

public class PlayerVault
{
    private const int VAULT_SIZE = 60;
    
    public InventorySlot[] Slots { get; set; }
    
    [JsonIgnore] public List<Func<bool>> AccessConditions { get; private set; }
    [JsonIgnore] public bool IsAccessible => AccessConditions.All(c => c());

    public PlayerVault()
    {
        Slots = new InventorySlot[VAULT_SIZE];
        for (int i = 0; i < VAULT_SIZE; i++)
            Slots[i] = new InventorySlot();

        AccessConditions = new List<Func<bool>>();
    }

    public void AddAccessCondition(Func<bool> condition)
    {
        AccessConditions.Add(condition);
    }
    
    
}