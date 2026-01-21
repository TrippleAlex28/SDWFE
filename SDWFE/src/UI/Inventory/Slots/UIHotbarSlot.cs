using System;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIHotbarSlot : UIInventorySlot, IUIFocusableSlot
{
    public UISlotGroup Group => UISlotGroup.Hotbar;

    public bool IsFocused { get; private set; }
    public event Action<bool>? OnFocusChange;
    public event Action<IUIFocusableSlot>? Clicked;

    private UIContainer? _focusOverlay;

    private readonly Color _normalTint = Color.White;
    private readonly Color _hoverTint = new Color(220, 220, 220);
    private readonly Color _focusTint = new Color(255, 255, 200);
    
    public UIHotbarSlot(
        InventorySlot slot,
        int index,
        float slotSize,
        Texture2D slotSheet,
        Rectangle slotSheetRect,
        Rectangle selectedSlotSheetRect)
        : base(slot, index, slotSize, slotSheet, slotSheetRect, selectedSlotSheetRect)
    {
        HoverEntered += OnHoverEnter;
        HoverExited += OnHoverExit;
        Released += OnClick;
    }
    
    private void OnHoverEnter(UIControl c)
    {
        if (!IsFocused)
            _background.Tint = _hoverTint;
    }

    private void OnHoverExit(UIControl c)
    {
        _background.Tint = IsFocused ? _focusTint : _normalTint;
    }

    private void OnClick(UIControl c)
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

    private void Focus()
    {
        _background.Tint = _focusTint;
        CreateFocusVisuals();
    }

    public void Unfocus()
    {
        IsFocused = false;
        _background.Tint = _normalTint;
        RemoveFocusVisuals();
    }
    
    private void CreateFocusVisuals()
    {
        if (_focusOverlay != null) return;

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

    private void RemoveFocusVisuals()
    {
        if (_focusOverlay == null) return;
        RemoveChild(_focusOverlay);
        _focusOverlay = null;
    }
}