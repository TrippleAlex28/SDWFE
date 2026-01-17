using System;
using System.Collections.Generic;
using Engine;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Ability;

#nullable enable

namespace SDWFE.UI.Inventory;

public class UIHotbar : UIContainer
{
    private Player _player;

    private UIVisual _background;
    private UIHBoxContainer _slotRow;
    private List<UIHotbarSlot> _slots = new();

    private Texture2D _slotSheet;
    private Rectangle _slotSheetRect;
    private Rectangle _selectedSlotSheetRect;

    private const float BACKGROUND_PADDING_X = 8f;
    private const float BACKGROUND_PADDING_Y = 4f;
    private const float SLOT_SIZE = 20f;
    private const float SPACING = 4f;
    private const int ABILITY_COUNT = 5;
    
    public UIHotbar(Player player)
    {
        _player = player;
        _player.OnAbilitiesChanged += HandleAbilitiesChanged;
        
        _slotSheet = ExtendedGame.AssetManager.LoadTexture("inventorySheet", "UI/");
        _slotSheetRect = new Rectangle(0, 32, 28, 28);
        _selectedSlotSheetRect = new Rectangle(0, 64, 28, 28);

        AlignmentPoint = Alignment.BottomMiddle;
        Margin = new Vector4(0, 0, 0, 10);

        // Create background
        _background = UIVisual.FromColor(new Color(255, 20, 20, 0));
        _background.AlignmentPoint = Alignment.BottomMiddle;
        float width = (SLOT_SIZE * ABILITY_COUNT) + (SPACING * Math.Max(0, ABILITY_COUNT - 1)) + BACKGROUND_PADDING_X * 2f;
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
        
        // Create Slots for each ability
        for (int i = 0; i < ABILITY_COUNT; i++)
        {
            var slot = new UIHotbarSlot(
                index: i,
                slotSize: SLOT_SIZE,
                slotSheet: _slotSheet,
                slotSheetRect: _slotSheetRect,
                selectedSlotSheetRect: _selectedSlotSheetRect
            );
            
            // Set placeholder icon (grayed out ability icon)
            var abilityName = AbilityRegistry.ShopAbilities[i];
            var abilityData = AbilityRegistry.GetAbility(abilityName);
            if (abilityData != null)
            {
                slot.SetPlaceholderIcon(abilityData.Icon);
            }

            _slots.Add(slot);
            _slotRow.AddChild(slot);
        }
        
        // Initialize with current abilities
        HandleAbilitiesChanged();
        HandleSelectionChange(_player.SelectedAbilitySlot);
    }
    
    private void HandleAbilitiesChanged()
    {
        var abilities = _player.GetAllAbilities();
        
        for (int i = 0; i < _slots.Count && i < abilities.Length; i++)
        {
            if (abilities[i] != null)
            {
                _slots[i].SetAbility(abilities[i]);
            }
            else
            {
                // Show grayed out placeholder
                var abilityName = AbilityRegistry.ShopAbilities[i];
                var abilityData = AbilityRegistry.GetAbility(abilityName);
                if (abilityData != null)
                {
                    _slots[i].SetPlaceholderIcon(abilityData.Icon);
                }
            }
        }
    }

    private void HandleSelectionChange(int newSelection)
    {
        for (int i = 0; i < _slots.Count; i++)
            _slots[i].SetSelected(i == newSelection);
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update cooldown visuals each frame
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].UpdateCooldown();
        }
        
        // Update selection
        HandleSelectionChange(_player.SelectedAbilitySlot);
    }
}