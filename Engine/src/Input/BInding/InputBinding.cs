using Microsoft.Xna.Framework.Input;

namespace Engine.Input.Binding;

/// <summary>
/// Represents a single input binding (keyboard key, mouse button, or gamepad button)
/// </summary>
public abstract class InputBinding
{
    public abstract string GetDisplayName();
    
    public abstract bool IsPressed(InputState state);
    public abstract bool IsDown(InputState state);
    public abstract bool IsReleased(InputState state);
}