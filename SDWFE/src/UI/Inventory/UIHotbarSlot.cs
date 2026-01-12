using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.UI.Inventory;

public class UIHotbarSlot : UIContainer
{
    private int _index;
    
    private UIVisual _background;
    private UIVisual _selected;
    private UIVisual? _indexText;
    private UIVisual? _icon;
    private UIVisual? _stackText;

    private Texture2D _slotSheet;
    private Rectangle _slotSheetRect;
    private Rectangle _selectedSlotSheetRect;
    
    public UIHotbarSlot(int index, float slotSize, Texture2D slotSheet, Rectangle slotSheetRect, Rectangle selectedSlotSheetRect)
    {
        _index = index;
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
        
        // Selected
        _selected = UIVisual.FromTexture(_slotSheet, _selectedSlotSheetRect);
        _background.AlignmentPoint = Alignment.MiddleCenter;
        _selected.DesiredSize = new Vector2(slotSize);
        AddChild(_selected);
    }

    public void SetSelected(bool selected)
    {
        _selected.Tint = selected ? Color.White : Color.Transparent;

        // Safety: keep it topmost, even after icons/text get re-added
        RemoveChild(_selected);
        AddChild(_selected);
    }
    
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

    public void SetIcon(Texture2D? icon)
    {
        if (icon == null)
        {
            if (_icon != null)
            {
                RemoveChild(_icon);
                _icon = null;
            }
            return;
        }
        
        if (_icon != null)
            RemoveChild(_icon);

        _icon = UIVisual.FromTexture(icon);
        _icon.AlignmentPoint = Alignment.MiddleCenter;
        _icon.DesiredSize = DesiredSize / 2;
        AddChild(_icon);
    }
}