using Engine;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory.Ability;

#nullable enable

namespace SDWFE.UI.Inventory;

public class UIHotbarSlot : UIContainer
{
    private int _index;
    private float _slotSize;
    
    private UIVisual _background;
    private UIVisual _selected;
    private UIVisual? _icon;
    private UIVisual? _lockedOverlay;
    private UIVisual? _cooldownOverlay;
    private UIVisual? _cooldownText;

    private Texture2D _slotSheet;
    private Rectangle _slotSheetRect;
    private Rectangle _selectedSlotSheetRect;
    
    private AbilityInstance? _ability;
    private bool _isUnlocked;
    
    public UIHotbarSlot(int index, float slotSize, Texture2D slotSheet, Rectangle slotSheetRect, Rectangle selectedSlotSheetRect)
    {
        _index = index;
        _slotSize = slotSize;
        _slotSheet = slotSheet;
        _slotSheetRect = slotSheetRect;
        _selectedSlotSheetRect = selectedSlotSheetRect;

        AlignmentPoint = Alignment.MiddleCenter;

        DesiredSize = new Vector2(slotSize);
        MinSize = new Vector2(slotSize);
        MaxSize = new Vector2(slotSize);
        
        // Background
        _background = UIVisual.FromTexture(_slotSheet, _slotSheetRect);
        _background.AlignmentPoint = Alignment.MiddleCenter;
        _background.DesiredSize = new Vector2(slotSize);
        AddChild(_background);
        
        // Locked overlay (shown when ability not unlocked)
        _lockedOverlay = UIVisual.FromColor(new Color(0, 0, 0, 150));
        _lockedOverlay.AlignmentPoint = Alignment.MiddleCenter;
        _lockedOverlay.DesiredSize = new Vector2(slotSize - 2);
        _lockedOverlay.IsVisible = true;
        AddChild(_lockedOverlay);
        
        // Cooldown overlay (semi-transparent, grows from top)
        _cooldownOverlay = UIVisual.FromColor(new Color(0, 0, 0, 180));
        _cooldownOverlay.AlignmentPoint = Alignment.TopMiddle;
        _cooldownOverlay.DesiredSize = new Vector2(slotSize - 2, 0);
        _cooldownOverlay.IsVisible = false;
        AddChild(_cooldownOverlay);
        
        // Selected border
        _selected = UIVisual.FromTexture(_slotSheet, _selectedSlotSheetRect);
        _selected.AlignmentPoint = Alignment.MiddleCenter;
        _selected.DesiredSize = new Vector2(slotSize);
        _selected.Tint = Color.Transparent;
        AddChild(_selected);
    }

    public void SetSelected(bool selected)
    {
        _selected.Tint = selected ? Color.White : Color.Transparent;

        // Safety: keep it topmost, even after icons/text get re-added
        RemoveChild(_selected);
        AddChild(_selected);
    }
    
    /// <summary>
    /// Sets the ability for this slot
    /// </summary>
    public void SetAbility(AbilityInstance? ability)
    {
        _ability = ability;
        _isUnlocked = ability != null;
        
        // Update locked overlay
        if (_lockedOverlay != null)
        {
            _lockedOverlay.IsVisible = !_isUnlocked;
        }
        
        // Update icon
        if (_icon != null)
        {
            RemoveChild(_icon);
            _icon = null;
        }
        
        if (ability != null)
        {
            _icon = UIVisual.FromTexture(ability.Data.Icon);
            _icon.AlignmentPoint = Alignment.MiddleCenter;
            _icon.DesiredSize = new Vector2(_slotSize - 4);
            
            // Insert before overlays
            RemoveChild(_lockedOverlay!);
            RemoveChild(_cooldownOverlay!);
            RemoveChild(_selected);
            
            AddChild(_icon);
            AddChild(_lockedOverlay!);
            AddChild(_cooldownOverlay!);
            AddChild(_selected);
        }
    }
    
    /// <summary>
    /// Sets the placeholder icon for an ability slot that hasn't been unlocked yet
    /// </summary>
    public void SetPlaceholderIcon(Texture2D icon)
    {
        if (_icon != null)
        {
            RemoveChild(_icon);
            _icon = null;
        }
        
        _icon = UIVisual.FromTexture(icon);
        _icon.AlignmentPoint = Alignment.MiddleCenter;
        _icon.DesiredSize = new Vector2(_slotSize - 4);
        _icon.Tint = new Color(100, 100, 100, 200); // Grayed out
        
        // Insert before overlays
        RemoveChild(_lockedOverlay!);
        RemoveChild(_cooldownOverlay!);
        RemoveChild(_selected);
        
        AddChild(_icon);
        AddChild(_lockedOverlay!);
        AddChild(_cooldownOverlay!);
        AddChild(_selected);
    }
    
    /// <summary>
    /// Updates cooldown display each frame
    /// </summary>
    public void UpdateCooldown()
    {
        if (_ability == null || _cooldownOverlay == null)
            return;
            
        if (_ability.IsOnCooldown)
        {
            // Show cooldown overlay proportional to remaining cooldown
            float progress = _ability.CooldownProgress;
            float overlayHeight = (_slotSize - 2) * progress;
            _cooldownOverlay.DesiredSize = new Vector2(_slotSize - 2, overlayHeight);
            _cooldownOverlay.IsVisible = true;
            
            // Show cooldown text
            if (_cooldownText != null)
            {
                RemoveChild(_cooldownText);
            }
            
            int secondsLeft = (int)System.Math.Ceiling(_ability.RemainingCooldown);
            _cooldownText = UIVisual.FromText(secondsLeft.ToString(), Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 8), Color.White);
            _cooldownText.MinSize = new Vector2(20, 20);
            _cooldownText.AlignmentPoint = Alignment.MiddleCenter;
            AddChild(_cooldownText);
        }
        else
        {
            _cooldownOverlay.IsVisible = false;
            _cooldownOverlay.DesiredSize = new Vector2(_slotSize - 2, 0);
            
            if (_cooldownText != null)
            {
                RemoveChild(_cooldownText);
                _cooldownText = null;
            }
        }
    }
    
    #region Legacy methods for weapon slots
    private UIVisual? _stackText;
    
    /// <summary>
    /// Sets the stack count (for weapons/items display)
    /// </summary>
    public void SetStackCount(int count)
    {
        if (_stackText != null)
            RemoveChild(_stackText);
        
        if (count <= 1)
        {
            _stackText = null;
            return;
        }

        _stackText = UIVisual.FromText(count.ToString(), Resources.TextFont, Color.White);
        _stackText.AlignmentPoint = Alignment.BottomRight;
        _stackText.Margin = new Vector4(0, 0, 4, 2);
        AddChild(_stackText);
    }

    /// <summary>
    /// Sets the icon directly (for weapons/items display)
    /// </summary>
    public void SetIcon(Texture2D? icon)
    {
        if (icon == null)
        {
            if (_icon != null)
            {
                RemoveChild(_icon);
                _icon = null;
            }
            // Show locked overlay when no icon
            if (_lockedOverlay != null)
                _lockedOverlay.IsVisible = true;
            return;
        }
        
        if (_icon != null)
            RemoveChild(_icon);

        _icon = UIVisual.FromTexture(icon);
        _icon.AlignmentPoint = Alignment.MiddleCenter;
        _icon.DesiredSize = new Vector2(_slotSize - 4);
        
        // Insert before overlays
        RemoveChild(_lockedOverlay!);
        RemoveChild(_cooldownOverlay!);
        RemoveChild(_selected);
        
        AddChild(_icon);
        AddChild(_lockedOverlay!);
        AddChild(_cooldownOverlay!);
        AddChild(_selected);
        
        // Hide locked overlay when icon is set
        if (_lockedOverlay != null)
            _lockedOverlay.IsVisible = false;
    }
    #endregion
}