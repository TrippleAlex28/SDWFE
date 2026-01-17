using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIInventoryItemSlot : UIInventorySlot
{
    private PlayerInventory _inventory;
    private bool _isVault;

    private Color _hoverTint = new Color(255, 255, 255, 180);
    private Color _normalTint = Color.White;

    public UIInventoryItemSlot(
        InventorySlot slot,
        int index,
        float slotSize,
        Texture2D slotSheet,
        Rectangle slotSheetRect,
        Rectangle selectedSlotSheetRect,
        PlayerInventory inventory,
        bool isVault)
        : base(slot, index, slotSize, slotSheet, slotSheetRect, selectedSlotSheetRect)
    {
        _inventory = inventory;
        _isVault = isVault;

        // Hook up interaction events
        HoverEntered += OnHoverEnter;
        HoverExited += OnHoverExit;
        Released += OnClick;
    }

    private void OnHoverEnter(UIControl control)
    {
        // TODO: HOVER INTERACTION
        if (!_slot.IsEmpty())
        {
            _background.Tint = _hoverTint;
        }
    }

    private void OnHoverExit(UIControl control)
    {
        // TODO: STOP HOVER INTERACTION
        _background.Tint = _normalTint;
    }

    private void OnClick(UIControl control)
    {
        if (_slot.IsEmpty())
            return;

        // TODO: INVENTORY SLOT INTERACTION, currently switches between vault and inventory
        
        // Transfer item between inventory and vault
        if (_isVault)
        {
            // Transfer from vault to inventory
            _inventory.TransferFromVault(_slot.Item!.Name, _slot.Item.StackSize);
        }
        else
        {
            // Transfer from inventory to vault (if accessible)
            if (_inventory.Vault.IsAccessible)
            {
                _inventory.TransferToVault(_slot.Item!.Name, _slot.Item.StackSize);
            }
        }
    }
}