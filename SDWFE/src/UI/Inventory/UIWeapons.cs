using System;
using System.Collections.Generic;
using Engine;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory;

public class UIWeapons : UIContainer
{
    private PlayerInventory _inventory;

    private UIVisual _background;
    private UIHBoxContainer _slotRow;
    private List<UIHotbarSlot> _slots = new();

    private Texture2D _slotSheet;
    private Rectangle _slotSheetRect;
    private Rectangle _selectedSlotSheetRect;

    private const float BACKGROUND_PADDING_X = 8f;
    private const float BACKGROUND_PADDING_Y = 4f;
    private const float SLOT_SIZE = 32f;
    private const float SPACING = 8f;
    
    public UIWeapons(PlayerInventory inventory)
    {
        _inventory = inventory;
        _inventory.OnInventoryChanged += HandleInventoryChange;
        _inventory.OnWeaponSelectionChanged += HandleWeaponSelectionChange;
        
        _slotSheet = ExtendedGame.AssetManager.LoadTexture("inventorySheet", "UI/");
        _slotSheetRect = new Rectangle(0, 32, 28, 28);
        _selectedSlotSheetRect = new Rectangle(0, 64, 28, 28);

        AlignmentPoint = Alignment.BottomLeft;
        Margin = new Vector4(10, 0, 0, 10);

        // Create background
        _background = UIVisual.FromColor(new Color(20, 20, 20, 100));
        _background.AlignmentPoint = Alignment.BottomLeft;
        float width = (SLOT_SIZE * _inventory.WeaponSlots.Length) + (SPACING * Math.Max(0, _inventory.WeaponSlots.Length - 1)) + BACKGROUND_PADDING_X * 2f;
        float height = SLOT_SIZE + BACKGROUND_PADDING_Y * 2f;
        _background.DesiredSize = new Vector2(width, height);
        AddChild(_background);

        // Create container for slots
        _slotRow = new UIHBoxContainer()
        {
            DesiredSize = Vector2.Zero,
            AlignmentPoint = Alignment.MiddleCenter,
            Spacing = SPACING,
            Padding = new Vector4(BACKGROUND_PADDING_X, BACKGROUND_PADDING_Y, BACKGROUND_PADDING_X, BACKGROUND_PADDING_Y),
        };
        _background.AddChild(_slotRow);
        
        // Create Slots
        for (int i = 0; i < _inventory.WeaponSlots.Length; i++)
        {
            var slot = new UIHotbarSlot(
                index: i,
                slotSize: SLOT_SIZE,
                slotSheet: _slotSheet,
                slotSheetRect: _slotSheetRect,
                selectedSlotSheetRect: _selectedSlotSheetRect
            );

            _slots.Add(slot);
            _slotRow.AddChild(slot);
        }
        
        // Update slots & selection
        HandleInventoryChange();
        HandleWeaponSelectionChange(_inventory.SelectedHotbarIndex);
    }
    
    private void HandleInventoryChange()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetStackCount(_inventory.WeaponSlots[i].Item?.StackSize ?? 0);
            _slots[i].SetIcon(_inventory.WeaponSlots[i].Item?.Icon ?? null);
        }
    }

    private void HandleWeaponSelectionChange(int newSelection)
    {
        // _selected.Padding = new Vector4(BACKGROUND_PADDING_X + newSelection * (SLOT_SIZE + SPACING), BACKGROUND_PADDING_Y, 0, 0);

        for (int i = 0; i < _slots.Count; i++)
            _slots[i].SetSelected(i == newSelection);
    }
}