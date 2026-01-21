using System;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public enum UISlotGroup
{
    Inventory,
    Vault,
    Hotbar,
    Weapon
}

public interface IUIFocusableSlot
{ 
    InventorySlot Slot { get; }
    int Index { get; }
    UISlotGroup Group { get; }
    
    bool IsFocused { get; }
    event Action<bool>? OnFocusChange;

    event Action<IUIFocusableSlot>? Clicked;

    void SetFocused(bool focused);
    void Unfocus();
}