using System;
using Engine.Input;
using SDWFE.Objects.Inventory;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public PlayerInventory Inventory { get; private set; }
    
    private void ConstructInventory()
    {
        // Inventory Setup
        Inventory = new PlayerInventory();
        this.AddChild(Inventory);

        Inventory.AddWeaponByName(ItemSetup.SHOTGUN);
        Inventory.AddItemByName(ItemSetup.BANDAGE,
            ItemSetup.ItemDataMap.TryGetValue(ItemSetup.BANDAGE, out var data) ? data.MaxStackSize : 1);
    }

    private void UpdateInventory()
    {
        var input = InputManager.Instance;

        if (input.IsActionPressed(InputSetup.ACTION_WEAPON_1))
            Inventory.SelectWeaponSlot(0);
        if (input.IsActionPressed(InputSetup.ACTION_WEAPON_2))
            Inventory.SelectWeaponSlot(1);
        if (input.IsActionPressed(InputSetup.ACTION_WEAPON_SWITCH))
            Inventory.SelectWeaponSlot(Inventory.SelectedWeaponIndex == 0 ? 1 : 0);
        
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_1))
            Inventory.SelectHotbarSlot(0);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_2))
            Inventory.SelectHotbarSlot(1);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_3))
            Inventory.SelectHotbarSlot(2);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_4))
            Inventory.SelectHotbarSlot(3);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_5))
            Inventory.SelectHotbarSlot(4);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_LEFT))
        {
            int nextIndex = Inventory.SelectedHotbarIndex + 1;
            Inventory.SelectHotbarSlot(nextIndex > Inventory.Hotbar.Length - 1 ? 0 : nextIndex);
        }
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_RIGHT))
        {
            int nextIndex = Inventory.SelectedHotbarIndex - 1;
            Inventory.SelectHotbarSlot(nextIndex < 0 ? 4 : nextIndex);
        }
    }
}