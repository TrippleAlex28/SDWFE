using System;
using Engine.Input;
using SDWFE.Objects.Inventory;
using SDWFE.UI.Inventory;

#nullable enable

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public PlayerInventory Inventory { get; private set; } = null!;
    public UIHotbar? HotbarUI { get; set; }
    public UIWeapons? WeaponsUI { get; set; }
    
    private void ConstructInventory()
    {
        // Inventory Setup
        Inventory = new PlayerInventory();
        WeaponsUI = new UIWeapons(Inventory);
        this.AddChild(Inventory);

        // Inventory.AddWeaponByName(ItemSetup.SHOTGUN);
        Inventory.AddWeaponByName(ItemSetup.PISTOL);
        Inventory.AddItemByName(ItemSetup.BANDAGE,
            ItemSetup.ItemDataMap.TryGetValue(ItemSetup.BANDAGE, out var data) ? data.MaxStackSize : 1);
    }
    
    /// <summary>
    /// Constructs the hotbar UI - must be called after ConstructAbilities
    /// </summary>
    private void ConstructHotbarUI()
    {
        HotbarUI = new UIHotbar(this);
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
        
        // Hotbar keys now select abilities
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_1))
            SelectAbilitySlot(0);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_2))
            SelectAbilitySlot(1);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_3))
            SelectAbilitySlot(2);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_4))
            SelectAbilitySlot(3);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_5))
            SelectAbilitySlot(4);
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_LEFT))
        {
            int nextIndex = SelectedAbilitySlot + 1;
            SelectAbilitySlot(nextIndex > 4 ? 0 : nextIndex);
        }
        if (input.IsActionPressed(InputSetup.ACTION_HOTBAR_RIGHT))
        {
            int nextIndex = SelectedAbilitySlot - 1;
            SelectAbilitySlot(nextIndex < 0 ? 4 : nextIndex);
        }
    }
}