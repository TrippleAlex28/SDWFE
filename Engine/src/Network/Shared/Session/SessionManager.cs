using Engine.Network.Shared.Command;
using Engine.Network.Shared.Session.Sessions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Network.Shared.Session;

public class SessionManager
{
    public GameSession? CurrentSession { get; private set; }
    
    public bool IsHost => CurrentSession?.Type == SessionType.MultiplayerHost;
    public bool IsClient => CurrentSession?.Type == SessionType.MultiplayerClient;
    public bool IsSingleplayer => CurrentSession?.Type == SessionType.Singleplayer;
    public bool IsMultiplayer => IsHost || IsClient;

    public void HandleInput(List<NetCommand> commands)
    {
        CurrentSession?.HandleInput(commands);
    }
    
    public void Update(GameTime gameTime)
    {
        CurrentSession?.Update(gameTime);
    }
    
    public void SwitchSession(SessionType type)
    {
        switch (type)
        {
            case SessionType.Singleplayer:
                SingleplayerSession spSession = new();
                SetSession(spSession);
                break;
            case SessionType.MultiplayerClient:
                MultiplayerClientSession mpcSession = new();
                SetSession(mpcSession);
                break;
            case SessionType.MultiplayerHost:
                MultiplayerHostSession mphSession = new();
                SetSession(mphSession);
                break;
        }
    }

    private void SetSession(GameSession newSession)
    {
        // Cleanup previous session
        if (CurrentSession != null)
        {
            CurrentSession.Reset();
        }
        
        // Start new session
        CurrentSession = newSession;
        CurrentSession.Initialize();
    }
}