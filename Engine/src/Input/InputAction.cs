using Engine.Input.Binding;

namespace Engine.Input;

/// <summary>
/// Represents a named action that can have multiple input bindings
/// </summary>
public sealed class InputAction
{
    public string Name { get; private set; }
    public List<InputBinding> Bindings { get; private set; } = new();

    public InputAction(string name)
    {
        Name = name;
    }

    public InputAction AddBinding(InputBinding binding)
    {
        Bindings.Add(binding);
        return this;
    }

    public InputAction RemoveBinding(InputBinding binding)
    {
        Bindings.Remove(binding);
        return this;
    }

    public void ClearBindings()
    {
        Bindings.Clear();
    }

    /// <summary>
    /// Returns true if any binding was pressed this frame
    /// </summary>
    public bool IsPressed(InputState state)
    {
        foreach (var binding in Bindings)
        {
            if (binding.IsPressed(state))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if any binding is currently held down
    /// </summary>
    public bool IsDown(InputState state)
    {
        foreach (var binding in Bindings)
        {
            if (binding.IsDown(state))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if any binding was released this frame
    /// </summary>
    public bool IsReleased(InputState state)
    {
        foreach (var binding in Bindings)
        {
            if (binding.IsReleased(state))
                return true;
        }
        return false;
    }
}