using Microsoft.Xna.Framework;

namespace Engine.UI.Elements;

public class UIRoot : UIContainer
{
    public UIRoot()
    {
    }

    /// <summary>
    /// Sets size of the root rect, most likely just the window size
    /// </summary>
    /// <param name="screenRect"></param>
    public void SetRootRect(Rectangle screenRect)
    {
        layoutSlot = screenRect;
        DesiredSize = new Vector2(screenRect.Width, screenRect.Height);
        _layoutDirty = true;
    }

    // When Padding is asked do nothing with the padding, as UIRoot should not have padding
    protected override Rectangle SlotWithPadding()
    {
        return layoutSlot;
    }
}