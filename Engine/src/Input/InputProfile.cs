namespace Engine.Input;

/// <summary>
/// Represents a context-specific collection of input actions (e.g., Gameplay, UI)
/// </summary>
public sealed class InputProfile
{
    public string Name { get; private set; }
    private Dictionary<string, InputAction> _actions = new();

    public InputProfile(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Adds or updates an action in this profile
    /// </summary>
    public void RegisterAction(InputAction action)
    {
        _actions[action.Name] = action;
    }
    
    /// <summary>
    /// Removes an action from this profile
    /// </summary>
    public void UnregisterAction(string actionName)
    {
        _actions.Remove(actionName);
    }
    
    /// <summary>
    /// Checks if an action exists in this profile
    /// </summary>
    public bool HasAction(string actionName)
    {
        return _actions.ContainsKey(actionName);
    }
    
    /// <summary>
    /// Gets an action by name, or null if not found
    /// </summary>
    public InputAction? GetAction(string actionName)
    {
        _actions.TryGetValue(actionName, out var action);
        return action;
    }
    
    /// <summary>
    /// Gets all actions in this profile
    /// </summary>
    public IEnumerable<InputAction> GetAllActions()
    {
        return _actions.Values;
    }
    
    /// <summary>
    /// Convenience method to check if an action is pressed
    /// </summary>
    public bool IsActionPressed(string actionName, InputState state)
    {
        var action = GetAction(actionName);
        return action?.IsPressed(state) ?? false;
    }

    /// <summary>
    /// Convenience method to check if an action is held down
    /// </summary>
    public bool IsActionDown(string actionName, InputState state)
    {
        var action = GetAction(actionName);
        return action?.IsDown(state) ?? false;
    }

    /// <summary>
    /// Convenience method to check if an action is released
    /// </summary>
    public bool IsActionReleased(string actionName, InputState state)
    {
        var action = GetAction(actionName);
        return action?.IsReleased(state) ?? false;
    }
}