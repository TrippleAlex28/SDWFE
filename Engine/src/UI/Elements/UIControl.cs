using Engine.Input;
using Microsoft.Xna.Framework;

namespace Engine.UI.Elements;

public class UIControl : UIElement
{
    public event Action<UIControl>? HoverEntered;
    public event Action<UIControl>? HoverExited;
    public event Action<UIControl>? Pressed;
    public event Action<UIControl>? Released;


    private bool _isInside = false;
    private bool _isPressed = false;


    public UIControl() : base()
    {
        
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        // Don't process input if not visible
        if (!IsVisible)
            return;

        Rectangle slot = CalculateActualSlot();
        Vector2 size = new Vector2(slot.Width, slot.Height);

        if (IsMouseInside(GlobalPosition, size) && !_isInside)
        {
            HoverEntered?.Invoke(this);
            _isInside = true;
        }
        else if (!IsMouseInside(GlobalPosition, size) && _isInside)
        {
            HoverExited?.Invoke(this);
            _isInside = false;
        }
        
        if (IsMouseInside(GlobalPosition, size) && !_isPressed && InputManager.Instance.IsActionPressed("UISelect"))
        {
            Pressed?.Invoke(this);
            _isPressed = true;
        }
        
        if (IsMouseInside(GlobalPosition, size) && _isPressed && InputManager.Instance.IsActionReleased("UISelect"))
        {
            Released?.Invoke(this);
            _isPressed = false;
        
        }
    }
    private bool IsMouseInside(Vector2 globalPosition, Vector2 size)
    {
        Vector2 mousePos = ExtendedGame.GetMouseUIPosition();
        
        if (mousePos.X > globalPosition.X && mousePos.X < globalPosition.X + size.X 
                                          && mousePos.Y > globalPosition.Y && mousePos.Y < globalPosition.Y + size.Y)
        {
            return true;
        }
        return false;
    }
    
}