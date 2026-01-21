using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI.Elements;

public class UIScrollContainer : UIContainer
{
    private float _scrollOffset = 0f;
    private float _speed = 25f;
    private bool _isInside = false;
    private bool _isHorizontal = false;
    
    // Total size of content (height for vertical, width for horizontal)
    private float _contentSize = 0f;
    private bool _isAutomatic = false;
    private bool _hideOutOfBound = false;

    public UIScrollContainer(bool isHorizontal = false, bool isAutomatic = false, bool hideOutOfBound = true, int speed = 25) : base()
    {
        _isHorizontal = isHorizontal;
        _isAutomatic = isAutomatic;
        _hideOutOfBound = hideOutOfBound;
        _speed = speed;
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        if (!IsVisible) return;

        Rectangle slot = CalculateActualSlot();
        Vector2 size = new Vector2(slot.Width, slot.Height);
        _isInside = IsMouseInside(GlobalPosition, size);

        if (_isAutomatic)
        {
            // Auto scroll down
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _scrollOffset -= _speed * dt;
            MarkLayoutDirty();
        }
        if (_isInside && !_isAutomatic)
        {
            // bool scrollForward = _isHorizontal 
            //     ? (InputManager.Instance.IsActionDown("UIScrollUp") || InputManager.Instance.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            //     : (InputManager.Instance.IsActionDown("UIScrollUp") || InputManager.Instance.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up));
            //
            // bool scrollBackward = _isHorizontal
            //     ? (InputManager.Instance.IsActionDown("UIScrollDown") || InputManager.Instance.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            //     : (InputManager.Instance.IsActionDown("UIScrollDown") || InputManager.Instance.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down));

            bool scrollForward = InputManager.Instance.IsActionDown("UIScrollUp");
            bool scrollBackward = InputManager.Instance.IsActionDown("UIScrollDown");
            
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // For vertical scrolling we treat negative offset as "scrolled up" (content moved up).
            if (scrollForward)
            {
                _scrollOffset += _speed;
                MarkLayoutDirty();
            }
            else if (scrollBackward)
            {
                _scrollOffset -= _speed;
                MarkLayoutDirty();
            }
        }

        // Clamp scroll offset
        float visibleSize = _isHorizontal 
            ? slot.Width - Margin.X - Margin.Z
            : slot.Height - Margin.Y - Margin.W;
        float maxScroll = Math.Max(0, _contentSize - visibleSize);
        
        float oldOffset = _scrollOffset;
        _scrollOffset = Math.Clamp(_scrollOffset, -maxScroll, 0f);
        
        if (oldOffset != _scrollOffset)
            MarkLayoutDirty();
    }

    protected override void SetChildLayout()
    {
        UIElement[] children = GetChildren().Cast<UIElement>().ToArray();
        
        Rectangle actualSlot = CalculateActualSlot();
        
        if (_isHorizontal)
        {
            SetHorizontalLayout(children, actualSlot);
        }
        else
        {
            SetVerticalLayout(children, actualSlot);
        }
    }

    private void SetVerticalLayout(UIElement[] children, Rectangle actualSlot)
    {
        float visibleWidth = actualSlot.Width - Margin.X - Margin.Z;
        float currentY = Margin.Y + _scrollOffset;
        float totalHeight = 0f;

        foreach (UIElement child in children)
        {
            float childHeight = child.DesiredSize.Y;
            if (childHeight < child.MinSize.Y) childHeight = child.MinSize.Y;
            if (childHeight > child.MaxSize.Y) childHeight = child.MaxSize.Y;
            
            child.layoutSlot = new Rectangle(
                (int)Margin.X,
                (int)currentY,
                (int)visibleWidth,
                (int)childHeight
            );
            child.MarkLayoutDirty();
            
            if (_hideOutOfBound)
            {
                // Hide children that are out of bounds
                child.IsVisible = !IsChildOutOfBounds(child.layoutSlot, actualSlot);
            }
            currentY += childHeight + Spacing;
            totalHeight += childHeight + Spacing;
        }
        
        if (children.Length > 0)
            totalHeight -= Spacing;

        _contentSize = totalHeight;
    }

    private void SetHorizontalLayout(UIElement[] children, Rectangle actualSlot)
    {
        float visibleHeight = actualSlot.Height - Margin.Y - Margin.W;
        float currentX = Margin.X + _scrollOffset;
        float totalWidth = 0f;

        foreach (UIElement child in children)
        {
            float childWidth = child.DesiredSize.X;
            if (childWidth < child.MinSize.X) childWidth = child.MinSize.X;
            if (childWidth > child.MaxSize.X) childWidth = child.MaxSize.X;
            
            child.layoutSlot = new Rectangle(
                (int)currentX,
                (int)Margin.Y,
                (int)childWidth,
                (int)visibleHeight
            );
            child.MarkLayoutDirty();
            
            if (_hideOutOfBound)
            {
                // Hide children that are out of bounds
                child.IsVisible = !IsChildOutOfBounds(child.layoutSlot, actualSlot);
            }

            currentX += childWidth + Spacing;
            totalWidth += childWidth + Spacing;
        }
        
        if (children.Length > 0)
            totalWidth -= Spacing;

        _contentSize = totalWidth;
    }

    /// <summary>
    /// Checks if a child's layout slot is completely outside the visible container bounds.
    /// </summary>
    private bool IsChildOutOfBounds(Rectangle childSlot, Rectangle containerSlot)
    {
        if (_isHorizontal)
        {
            if (childSlot.Left < Margin.X)
                return true;
            // Child is completely to the right of visible area
            if (childSlot.Right > containerSlot.Width - Margin.Z)
                return true;
        }
        else
        {
            // Child is completely above visible area
            if (childSlot.Top < Margin.Y)
                return true;
            // Child is completely below visible area
            if (childSlot.Bottom > containerSlot.Height - Margin.W)
                return true;
        }
        return false;
    }

    private bool IsMouseInside(Vector2 globalPosition, Vector2 size)
    {
        Point mousePos = ExtendedGame.GetMouseUIPosition().ToPoint();
        
        if (mousePos.X > globalPosition.X && mousePos.X < globalPosition.X + size.X 
        && mousePos.Y > globalPosition.Y && mousePos.Y < globalPosition.Y + size.Y)
        {
            return true;
        }
        return false;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

       
            // // Debug drawing
            // Rectangle r = CalculateActualSlot();
            // Color c = _isInside ? new Color(0, 255, 0, 100) : new Color(255, 0, 0, 100);
            // spriteBatch.Draw(EngineResources.BlankSquare, r, c);
        
    }
}