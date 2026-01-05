namespace Engine.Scene;

public static class SceneRegistry
{
    private static readonly Dictionary<string, Func<Scene>> _sceneFactories = new();
    
    /// <summary>
    /// Register a new scene
    /// </summary>
    public static void Register<T>(string key) where T : Scene, new()
    {
        if (_sceneFactories.ContainsKey(key))
            throw new InvalidOperationException($"Scene key \'{key}\' already registered");
        
        _sceneFactories[key] = () => new T();
    }

    /// <summary>
    /// Create a scene from the saved registry
    /// </summary>
    public static Scene Create(string key)
    {
        if (!_sceneFactories.TryGetValue(key, out var factory))
            throw new InvalidOperationException($"Unknown scene key: \'{key}\'");

        return factory();
    }
}
