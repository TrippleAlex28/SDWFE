using System;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIInventoryItemSlot : UIInventorySlot, IUIFocusableSlot
{
    private PlayerInventory _inventory;
    public bool IsVault { get; private set; }
    public UISlotGroup Group => IsVault ? UISlotGroup.Vault : UISlotGroup.Inventory;

    public bool IsFocused { get; private set; }
    private UIContainer? _focusOverlay;
    private UIVisual? _focusPopup;
    public event Action<bool>? OnFocusChange;
    
    public event Action<IUIFocusableSlot>? Clicked;
    
    private Color _focusTint = new Color(255, 255, 155, 255);
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
        IsVault = isVault;

        // Hook up interaction events
        HoverEntered += OnHoverEnter;
        HoverExited += OnHoverExit;
        Released += OnClick;
    }

    private void OnHoverEnter(UIControl control)
    {
        if (!Slot.IsEmpty() && !IsFocused)
        {
            _background.Tint = _hoverTint;
        }
    }

    private void OnHoverExit(UIControl control)
    {
        _background.Tint = _normalTint;
    }

    private void OnClick(UIControl control)
    {
        Clicked?.Invoke(this);
    }

    public void SetFocused(bool focused)
    {
        OnFocusChange?.Invoke(focused);
        
        IsFocused = focused;
        if (IsFocused)
            Focus();
        else
            Unfocus();
    }

    public void Focus()
    {
        _background.Tint = _focusTint;
        CreateFocusVisuals();
    }
    
    public void Unfocus()
    {
        _background.Tint = _normalTint;
        RemoveFocusVisuals();
    }
    
    private void CreateFocusVisuals()
    {
        // Create focus overlay (bright border / glow effect)
        if (_focusOverlay == null)
        {
            _focusOverlay = new UIContainer
            {
                AlignmentPoint = Alignment.TopLeft,
                DesiredSize = new Vector2(_slotSize),
                MinSize = DesiredSize,
                MaxSize = DesiredSize,
            };
            AddChild(_focusOverlay);
            
            var topEdge = UIVisual.FromColor(new Color(255, 255, 0, 255));
            topEdge.AlignmentPoint = Alignment.TopLeft;
            topEdge.DesiredSize = new Vector2(_slotSize, 4);
            _focusOverlay.AddChild(topEdge);
            
            var leftEdge = UIVisual.FromColor(new Color(255, 255, 0, 255));
            leftEdge.AlignmentPoint = Alignment.TopLeft;
            leftEdge.DesiredSize = new Vector2(4, _slotSize);
            _focusOverlay.AddChild(leftEdge);
            
            var rightEdge = UIVisual.FromColor(new Color(255, 255, 0, 255));
            rightEdge.AlignmentPoint = Alignment.TopRight;
            rightEdge.DesiredSize = new Vector2(4, _slotSize);
            _focusOverlay.AddChild(rightEdge);
            
            var bottomEdge = UIVisual.FromColor(new Color(255, 255, 0, 255));
            bottomEdge.AlignmentPoint = Alignment.BottomLeft;
            bottomEdge.DesiredSize = new Vector2(_slotSize, 4);
            _focusOverlay.AddChild(bottomEdge);
        }

        // Create popup with instructions
        if (_focusPopup == null)
        {
            var popupContainer = new UIContainer()
            {
                AlignmentPoint = Alignment.BottomMiddle,
                DesiredSize = new Vector2(150, 40),
                Margin = new Vector4(0, 0, 0, _slotSize + 10)
            };

            var popupBg = UIVisual.FromColor(new Color(20, 20, 20, 240));
            popupBg.AlignmentPoint = Alignment.MiddleCenter;
            popupBg.Padding = new Vector4(6, 4, 6, 4);
            popupContainer.AddChild(popupBg);

            var popupText = UIVisual.FromText(
                "Press 1-2 for weapon\nPress 3-7 for hotbar",
                Resources.TextFont,
                new Color(200, 200, 200, 255)
            );
            popupText.AlignmentPoint = Alignment.MiddleCenter;
            popupBg.AddChild(popupText);

            _focusPopup = popupBg;
            AddChild(popupContainer);
        }
    }

    private void RemoveFocusVisuals()
    {
        if (_focusOverlay != null)
        {
            RemoveChild(_focusOverlay);
            _focusOverlay = null;
        }

        if (_focusPopup != null)
        {
            // The popup is inside a container, we need to remove the parent
            var parent = _focusPopup.Parent;
            if (parent != null && parent != this)
            {
                RemoveChild((UIElement)parent);
            }
            _focusPopup = null;
        }
    }
}