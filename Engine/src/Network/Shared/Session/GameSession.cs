using Engine.Network.Shared.Command;
using Microsoft.Xna.Framework;

namespace Engine.Network.Shared.Session;

public enum SessionType
{
    Singleplayer,
    MultiplayerClient,
    MultiplayerHost,
}

public abstract class GameSession
{
    public SessionType Type { get; private set; }
    public int LocalClientId { get; protected set; }

    protected bool IsInitialized = false;
    
    public GameSession(SessionType type)
    {
        Type = type;
    }

    public abstract void HandleInput(List<NetCommand> commands);
    public abstract void Update(GameTime gameTime);

    public virtual void OnSwitchScene(uint sceneEpoch, string sceneKey, int levelIndex = -1)
    {
        
    }
    
    /// <summary>
    /// Handle startup logic for the session
    /// </summary>
    public virtual void Initialize()
    {
        IsInitialized = true;
    }
    
    /// <summary>
    /// Handle shutdown logic for the session
    /// </summary>
    public virtual void Reset()
    {
        IsInitialized = false;
    }
}