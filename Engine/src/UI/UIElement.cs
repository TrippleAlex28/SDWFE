using Microsoft.Xna.Framework;

namespace Engine.UI;

public class UIElement : GameObject
{
    // This is the size the parent takes into account when arranging the layoutSlot, so that layoutslot remains the source of truth
    public Vector2 DesiredSize { get; set; } = new Vector2(0, 0);
    public Vector2 MinSize { get; set; } = new Vector2(0, 0);
    public Vector2 MaxSize { get; set; } = new Vector2(float.MaxValue, float.MaxValue);
    public Alignment AlignmentPoint = Alignment.TopLeft;
    public event Action<UIElement>? LayoutDirty;
    
    // The slot the UIelement receives from its parent, so that you can use local position
    internal Rectangle layoutSlot { get; set; }

    public Vector4 Margin;
    public Vector4 Padding;
    protected bool _layoutDirty;
    
    /// <summary>
    /// Draw layer offset inherited from parent UIElement.
    /// Ensures children render on top of their parents.
    /// </summary>
    private float _parentDrawLayerOffset = 0;

    /// <summary>
    /// Override DrawLayer to include parent offset for UIElements.
    /// This ensures child UIElements are drawn on top of their parents.
    /// </summary>
    public new float DrawLayer => (this.BaseDrawLayer + this.FineDrawLayer + this._parentDrawLayerOffset).Clamp(0, 1);

    // makes sure the child added is a UIElement
    public override void AddChild(GameObject @object)
    {
        if (@object is not UIElement child)
        {
            throw new Exception("Child of parent should be of class UIElement");
        }
        child.LayoutDirty += OnChildLayoutDirty;
        MarkLayoutDirty();
        
        // Add child first so we know its index
        base.AddChild(child);
        
        // Set the child's draw layer offset based on parent's layer + child index
        // This ensures later children are drawn on top of earlier children
        int childIndex = Children.IndexOf(child);
        child._parentDrawLayerOffset = this.DrawLayer + (0.0001f * (childIndex + 1));
        
        // Recursively update all descendants' draw layer offsets
        child.UpdateChildDrawLayers();
    }

    /// <summary>
    /// Recursively updates draw layer offsets for all children.
    /// Called when this element is added to a parent to propagate the correct offsets.
    /// </summary>
    private void UpdateChildDrawLayers()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is UIElement childElement)
            {
                childElement._parentDrawLayerOffset = this.DrawLayer + (0.0001f * (i + 1));
                childElement.UpdateChildDrawLayers();
            }
        }
    }

    /// <summary>
    /// Removes a child UIElement and resets its draw layer offset.
    /// </summary>
    public void RemoveChild(UIElement child)
    {
        if (child is UIElement uiElement)
        {
            uiElement.LayoutDirty -= OnChildLayoutDirty;
            uiElement._parentDrawLayerOffset = 0; // Reset draw layer offset when removed from parent
            MarkLayoutDirty();
        }
        base.RemoveChild(child);
    }
    
    private void OnChildLayoutDirty(UIElement child)
    {
        MarkLayoutDirty();
    }
    protected virtual void SetChildLayout()
    {
        UIElement[] children = Children.Cast<UIElement>().ToArray();

        foreach(UIElement child in children)
        {
            /*
            int width = (int)Math.Clamp(
                child.DesiredSize.X, 
                child.MinSize.X, 
                Math.Min(child.MaxSize.X, childMaxSlot.Width));
            
            int height = (int)Math.Clamp(
                child.DesiredSize.Y, 
                child.MinSize.Y, 
                Math.Min(child.MaxSize.Y, childMaxSlot.Height));

            */
            child.layoutSlot = ChildSlotWithMargin();
            /*child.layoutSlot = new Rectangle(
                (int)(childMaxSlot.X + child.LocalPosition.X),
                (int)(childMaxSlot.Y + child.LocalPosition.Y),
                width,
                height
            );*/
        }
    }
    private void SetLocalPos()
    {
        Rectangle actualSlot = CalculateActualSlot();
        LocalPosition = new Vector2(actualSlot.X, actualSlot.Y);
    }
    protected Rectangle CalculateActualSlot()
    {
        Rectangle layout = SlotWithPadding();
        float maxWidth = Math.Max(MinSize.X, Math.Min(MaxSize.X, layout.Width));
        float maxHeight = Math.Max(MinSize.Y, Math.Min(MaxSize.Y, layout.Height));
        
        // If DesiredSize is not set (0,0), use layout size as fallback
        float desiredWidth = DesiredSize.X > 0 ? DesiredSize.X : layout.Width;
        float desiredHeight = DesiredSize.Y > 0 ? DesiredSize.Y : layout.Height;
        
        int width = (int)Math.Clamp(desiredWidth, MinSize.X, maxWidth);
        int height = (int)Math.Clamp(desiredHeight, MinSize.Y, maxHeight);

        int freeWidth = layout.Width - width;
        int freeHeight = layout.Height - height;

        Vector2 anchor = ResolveAlignment();

        Rectangle actualSlot = new Rectangle(
            layout.X + (int)(freeWidth * anchor.X),
            layout.Y + (int)(freeHeight * anchor.Y),
            width,
            height
        );

        return actualSlot;
    }
    private Rectangle ChildSlotWithMargin()
    {
        Rectangle actualSlot = CalculateActualSlot();
        return new Rectangle(
            (int)(actualSlot.X - LocalPosition.X + Margin.X),
            (int)(actualSlot.Y - LocalPosition.Y + Margin.Y),
            (int)(actualSlot.Width - Margin.X - Margin.Z),
            (int)(actualSlot.Height - Margin.Y - Margin.W)
        );
    }
    protected virtual Rectangle SlotWithPadding()
    {
        return new Rectangle(
            (int)(layoutSlot.X + Padding.X),
            (int)(layoutSlot.Y + Padding.Y),
            (int)(layoutSlot.Width - Padding.X - Padding.Z),
            (int)(layoutSlot.Height - Padding.Y - Padding.W)
        );
    }
    // Called to make sure the position is correct of all elements
    protected override void UpdateSelf(GameTime gameTime)
    {
        if (_layoutDirty)
        {
            ResolveLayout();
            _layoutDirty = false;
        }
    }

    protected virtual void ResolveLayout()
    {
        SetLocalPos();
        SetChildLayout();
    }

    protected Vector2 ResolveAlignment()
    {
        Vector2 anchor = AlignmentPoint switch
        {
            Alignment.TopLeft => new Vector2(0, 0),
            Alignment.TopMiddle => new Vector2(0.5f, 0),
            Alignment.TopRight => new Vector2(1, 0),

            Alignment.MiddleLeft => new Vector2(0, 0.5f),
            Alignment.MiddleCenter => new Vector2(0.5f, 0.5f),
            Alignment.MiddleRight => new Vector2(1, 0.5f),

            Alignment.BottomLeft => new Vector2(0, 1),
            Alignment.BottomMiddle => new Vector2(0.5f, 1),
            Alignment.BottomRight => new Vector2(1, 1),

            _ => Vector2.Zero
        };
        return anchor;
    }

    protected void MarkLayoutDirty()
    {
        _layoutDirty = true;
        LayoutDirty?.Invoke(this);
    }
}