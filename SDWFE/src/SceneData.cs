using Engine.Network.Shared;

/// <summary>
/// Scene data that syncs over the network for level transitions.
/// Register this with GameState.Instance.SceneData in your game startup.
/// </summary>
public class SceneData : INetworkSceneData
{
    public static readonly SceneData Instance = new();
    
    public int LevelIndex { get; set; } = -1;
    public bool HasSeenIntro { get; set; } = false;

    public string LevelName => LevelNames.GetLevelName(LevelIndex);
    
    // Keep static accessors for backwards compatibility
    public static int levelIndex
    {
        get => Instance.LevelIndex;
        set => Instance.LevelIndex = value;
    }
    
    public static bool hasSeenIntro
    {
        get => Instance.HasSeenIntro;
        set => Instance.HasSeenIntro = value;
    }
    
    public static string levelName => Instance.LevelName;
}