using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIWeaponSlot : UIInventorySlot
{
    public UIWeaponSlot(
        InventorySlot slot,
        int index,
        float slotSize,
        Texture2D slotSheet,
        Rectangle slotSheetRect,
        Rectangle selectedSlotSheetRect)
        : base(slot, index, slotSize, slotSheet, slotSheetRect, selectedSlotSheetRect)
    {
    }
}