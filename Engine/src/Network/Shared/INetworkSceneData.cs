namespace Engine.Network.Shared;

/// <summary>
/// Interface for scene data that needs to be synced over the network.
/// Implement this in your game project and register it with GameState.
/// </summary>
public interface INetworkSceneData
{
    /// <summary>
    /// The current level index for scene transitions
    /// </summary>
    int LevelIndex { get; set; }
}
