using Engine.Input;
using SDWFE.Objects.Inventory;
using SDWFE.UI.Inventory2;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public PlayerInventory Inventory { get; private set; }
    public UIInventory InventoryUI { get; private set; }
    
    private void ConstructInventory()
    {
        // Inventory Setup
        Inventory = new PlayerInventory();
        InventoryUI = new UIInventory(Inventory);
        this.AddChild(Inventory);
        
        // Setup vault access conditions
        Inventory.Vault.AddAccessCondition(() => true);

        // Inventory.AddWeaponByName(ItemSetup.SHOTGUN);
        // Inventory.AddItemByName(ItemSetup.PISTOL);
        // Inventory.AddItemByName(ItemSetup.ASSAULT_RIFLE);
        // Inventory.AddItemByName(ItemSetup.SHOTGUN);
        // Inventory.AddItemByName(ItemSetup.FIREWORK_LAUNCHER);
        // Inventory.AddItemByName(ItemSetup.BANDAGE,
        //     ItemSetup.ItemDataMap.TryGetValue(ItemSetup.BANDAGE, out var data1) ? data1.MaxStackSize : 1);
        // Inventory.AddItemByName(ItemSetup.MEDKIT,
        //     ItemSetup.ItemDataMap.TryGetValue(ItemSetup.MEDKIT, out var data2) ? data2.MaxStackSize : 1);
        // Inventory.AddItemByName(ItemSetup.WOOD,
        //     ItemSetup.ItemDataMap.TryGetValue(ItemSetup.MEDKIT, out var data3) ? data3.MaxStackSize : 1);
        // Inventory.AddItemByName(ItemSetup.IRON,
        //     ItemSetup.ItemDataMap.TryGetValue(ItemSetup.MEDKIT, out var data4) ? data4.MaxStackSize : 1);

        Inventory.AddItemByName(ItemSetup.BOW);

    }

    private void UpdateInventory()
    {
        var input = InputManager.Instance;

        if (input.IsActionPressed(InputSetup.ACTION_WEAPON_1))
        {
            Inventory.SelectWeaponSlot(0);
            ResetShootCooldown();
        }
        if (input.IsActionPressed(InputSetup.ACTION_WEAPON_2))
        {
            Inventory.SelectWeaponSlot(1);
            ResetShootCooldown();
        }
        if (input.IsActionPressed(InputSetup.ACTION_WEAPON_SWITCH))
        {
            Inventory.SelectWeaponSlot(Inventory.SelectedWeaponIndex == 0 ? 1 : 0);
            ResetShootCooldown();
        }
        
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
            if (!InventoryUI.IsMenuOpen)
            {
                int nextIndex = Inventory.SelectedHotbarIndex + 1;
                Inventory.SelectHotbarSlot(nextIndex > Inventory.Hotbar.Length - 1 ? 0 : nextIndex);
            }
        }
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_RIGHT))
        {
            if (!InventoryUI.IsMenuOpen)
            {
                int nextIndex = Inventory.SelectedHotbarIndex - 1;
                Inventory.SelectHotbarSlot(nextIndex < 0 ? 4 : nextIndex);
            }
        }
    }
}