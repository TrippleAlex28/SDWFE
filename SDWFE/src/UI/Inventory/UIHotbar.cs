using System;
using System.Collections.Generic;
using Engine;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIHotbar : UIContainer
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
    private const float SLOT_SIZE = 16f;
    private const float SPACING = 4f;

    public UIHotbar(PlayerInventory inventory)
    {
        _inventory = inventory;
        _inventory.OnInventoryChanged += HandleInventoryChange;
        _inventory.OnHotbarSelectionChanged += HandleHotbarSelectionChange;

        _slotSheet = ExtendedGame.AssetManager.LoadTexture("inventorySheet", "UI/");
        _slotSheetRect = new Rectangle(0, 32, 28, 28);
        _selectedSlotSheetRect = new Rectangle(0, 64, 28, 28);

        AlignmentPoint = Alignment.BottomMiddle;
        Margin = new Vector4(0, 0, 0, 10);

        // Create background
        _background = UIVisual.FromColor(new Color(20, 20, 20, 100));
        _background.AlignmentPoint = Alignment.BottomMiddle;
        float width = (SLOT_SIZE * _inventory.Hotbar.Length) + (SPACING * Math.Max(0, _inventory.Hotbar.Length - 1)) + BACKGROUND_PADDING_X * 2f;
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

        // Create slots
        for (int i = 0; i < _inventory.Hotbar.Length; i++)
        {
            var slot = new UIHotbarSlot(
                _inventory.Hotbar[i],
                i,
                SLOT_SIZE,
                _slotSheet,
                _slotSheetRect,
                _selectedSlotSheetRect
            );

            _slots.Add(slot);
            _slotRow.AddChild(slot);
        }

        // Update slots & selection
        HandleInventoryChange();
        HandleHotbarSelectionChange(_inventory.SelectedHotbarIndex);
    }

    private void HandleInventoryChange()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Refresh();
        }
    }

    private void HandleHotbarSelectionChange(int newSelection)
    {
        for (int i = 0; i < _slots.Count; i++)
            _slots[i].SetSelected(i == newSelection);
    }
}