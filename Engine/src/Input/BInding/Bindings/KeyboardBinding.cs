using Microsoft.Xna.Framework.Input;

namespace Engine.Input.Binding.Bindings;

public class KeyboardBinding : InputBinding
{
    public Keys Key { get; set; }

    public KeyboardBinding(Keys key)
    {
        Key = key;
    }

    public override string GetDisplayName() => Key.ToString();

    public override bool IsPressed(InputState state)
    {
        return state.CurrentKeyboard.IsKeyDown(Key) &&
               state.PreviousKeyboard.IsKeyUp(Key);
    }
    
    public override bool IsDown(InputState state)
    {
        return state.CurrentKeyboard.IsKeyDown(Key);
    }

    public override bool IsReleased(InputState state)
    {
        return state.CurrentKeyboard.IsKeyUp(Key) && 
               state.PreviousKeyboard.IsKeyDown(Key);
    }
}