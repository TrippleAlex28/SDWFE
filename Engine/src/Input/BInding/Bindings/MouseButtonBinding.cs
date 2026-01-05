using Microsoft.Xna.Framework.Input;

namespace Engine.Input.Binding.Bindings;

public class MouseButtonBinding : InputBinding
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        WheelUp,
        WheelDown,
        XButton1,
        XButton2,
    }
    
    public MouseButton Button { get; set; }

    public MouseButtonBinding(MouseButton button)
    {
        Button = button;
    }
    
    public override string GetDisplayName() => $"Mouse {Button}";

    public override bool IsPressed(InputState state)
    {
        if (Button == MouseButton.WheelDown || Button == MouseButton.WheelUp)
            return GetButtonState(state.CurrentMouse, state) == ButtonState.Pressed;
        
        return GetButtonState(state.CurrentMouse, state) == ButtonState.Pressed &&
               GetButtonState(state.PreviousMouse, state) == ButtonState.Released;
    }

    public override bool IsDown(InputState state)
    {
        return GetButtonState(state.CurrentMouse, state) == ButtonState.Pressed;
    }

    public override bool IsReleased(InputState state)
    {
        if (Button == MouseButton.WheelDown || Button == MouseButton.WheelUp)
            return GetButtonState(state.CurrentMouse, state) == ButtonState.Released;
        
        return GetButtonState(state.CurrentMouse, state) == ButtonState.Released &&
               GetButtonState(state.PreviousMouse, state) == ButtonState.Pressed;
    }

    private ButtonState GetButtonState(MouseState mouseState, InputState state)
    {
        return Button switch
        {
            MouseButton.Left => mouseState.LeftButton,
            MouseButton.Right => mouseState.RightButton,
            MouseButton.Middle => mouseState.MiddleButton,
            MouseButton.WheelUp => (state.CurrentMouse.ScrollWheelValue - state.PreviousMouse.ScrollWheelValue) > 0 ? ButtonState.Pressed : ButtonState.Released,
            MouseButton.WheelDown => (state.CurrentMouse.ScrollWheelValue - state.PreviousMouse.ScrollWheelValue) < 0 ? ButtonState.Pressed : ButtonState.Released,
            MouseButton.XButton1 => mouseState.XButton1,
            MouseButton.XButton2 => mouseState.XButton2,
            _ => ButtonState.Released
        };
    }
}