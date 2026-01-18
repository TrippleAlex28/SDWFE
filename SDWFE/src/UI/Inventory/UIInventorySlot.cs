using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIInventorySlot : UIControl
{
    public InventorySlot Slot { get; protected set; }
    public int Index { get; protected set; }
    
    protected UIVisual _background;
    protected UIVisual _selected;
    protected UIVisual? _icon;
    protected UIVisual? _stackText;
    
    protected Texture2D _slotSheet;
    protected Rectangle _slotSheetRect;
    protected Rectangle _selectedSlotSheetRect;
    
    protected float _slotSize;
    protected bool _isSelected = false;
    
    public UIInventorySlot(
        InventorySlot slot,
        int index,
        float slotSize,
        Texture2D slotSheet,
        Rectangle slotSheetRect,
        Rectangle selectedSlotSheetRect)
    {
        Slot = slot;
        Index = index;
        _slotSize = slotSize;
        _slotSheet = slotSheet;
        _slotSheetRect = slotSheetRect;
        _selectedSlotSheetRect = selectedSlotSheetRect;

        AlignmentPoint = Alignment.MiddleCenter;
        DesiredSize = new Vector2(slotSize);
        MinSize = new Vector2(slotSize);
        MaxSize = new Vector2(slotSize);

        CreateVisuals();
        Refresh();
    }
    
    protected virtual void CreateVisuals()
    {
        // Background
        _background = UIVisual.FromTexture(_slotSheet, _slotSheetRect);
        _background.AlignmentPoint = Alignment.MiddleCenter;
        _background.DesiredSize = new Vector2(_slotSize);
        AddChild(_background);

        // Selected overlay
        _selected = UIVisual.FromTexture(_slotSheet, _selectedSlotSheetRect);
        _selected.AlignmentPoint = Alignment.MiddleCenter;
        _selected.DesiredSize = new Vector2(_slotSize);
        _selected.Tint = Color.Transparent;
        AddChild(_selected);
    }
    
    public virtual void Refresh()
    {
        // Clear icon and stack text
        if (_icon != null)
        {
            RemoveChild(_icon);
            _icon = null;
        }
        if (_stackText != null)
        {
            RemoveChild(_stackText);
            _stackText = null;
        }

        if (Slot.IsEmpty())
            return;

        // Add icon
        if (Slot.Item?.Icon != null)
        {
            _icon = UIVisual.FromTexture(Slot.Item.Icon);
            _icon.AlignmentPoint = Alignment.MiddleCenter;
            _icon.DesiredSize = new Vector2(_slotSize * 0.6f);
            AddChild(_icon);
        }

        // Add stack count
        int stackSize = Slot.Item?.StackSize ?? 0;
        if (stackSize > 1)
        {
            _stackText = UIVisual.FromText(stackSize.ToString(), Resources.TextFont, Color.White);
            _stackText.AlignmentPoint = Alignment.BottomRight;
            _stackText.Margin = new Vector4(0, 0, _slotSize * 0.15f, _slotSize * 0.1f);
            AddChild(_stackText);
        }

        // Ensure selected overlay is on top
        if (_isSelected)
        {
            RemoveChild(_selected);
            AddChild(_selected);
        }
    }

    public virtual void SetSelected(bool selected)
    {
        _isSelected = selected;
        _selected.Tint = selected ? Color.White : Color.Transparent;

        // Keep selection overlay on top
        RemoveChild(_selected);
        AddChild(_selected);
    }
}