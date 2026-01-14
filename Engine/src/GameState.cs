using Engine.Network.Shared.Command;
using Engine.Network.Shared.Session;
using Engine.Network.Shared.Session.Sessions;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine;

public class GameState
{
    private static GameState? _instance;
    public static GameState Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameState();
            return _instance;
        }
    }
    
    public uint ClientTick { get; private set; } = 0;
    public uint ServerTick { get; private set; } = 0;
    
    public SessionManager SessionManager { get; private set; }

    public uint SceneEpoch { get; private set; } = 0;
    public Scene.Scene? CurrentScene { get; private set; }

    /// <summary>
    /// Event called when MultiplayerClient gets disconnected from the server
    /// </summary>
    public event Action<string>? OnDisconnected;
    
    /// <summary>
    /// Event called when client receives a scene change from the server.
    /// Parameters: (sceneKey, levelIndex)
    /// </summary>
    public event Action<string, int>? OnSceneChangeReceived;
    
    private GameState()
    {
        SessionManager = new SessionManager();
    }

    public void AdvanceClientTick()
    {
        ClientTick++;
    }

    public void SetServerTick(uint serverTick)
    {
        if (serverTick > ServerTick)
            ServerTick = serverTick;
    }
    
    public void SwitchSession(SessionType sessionType)
    {
        ClientTick = 0;
        ServerTick = 0;
        
        // TODO: Start loading screen
        SessionManager.SwitchSession(sessionType);
        // TODO: Stop loading screen
    }

    /// <summary>
    /// Switch scenes as a Host, clients should only switch scenes when receiving packets using the function below
    /// </summary>
    public void SwitchScene(string sceneKey, int levelIndex = -1)
    {
        if (SessionManager.CurrentSession?.Type == SessionType.MultiplayerClient)
            return;
        
        // TODO: Start loading screen
        var scene = SceneRegistry.Create(sceneKey);
        
        CurrentScene?.Exit();
        CurrentScene = scene;
        CurrentScene.Enter();

        SceneEpoch += 1;
        
        SessionManager.CurrentSession?.OnSwitchScene(SceneEpoch, sceneKey, levelIndex);
        // TODO: Stop loading screen
    }

    public void SwitchSessionAndScene(SessionType sessionType, string sceneKey)
    {
        SwitchSession(sessionType);
        SwitchScene(sceneKey);
    }
    
    public void SwitchSceneClient(uint sceneEpoch, string sceneKey, int levelIndex)
    {
        // Notify game code before switching (so it can update global state)
        OnSceneChangeReceived?.Invoke(sceneKey, levelIndex);
        
        // TODO: Start loading screen
        var scene = SceneRegistry.Create(sceneKey);
        
        CurrentScene?.Exit();
        CurrentScene = scene;
        CurrentScene.Enter();

        SceneEpoch = sceneEpoch;
        // TODO: Stop loading screen
    }

    public void OnDisconnect(string reason)
    {
        SwitchSession(SessionType.Singleplayer);
        OnDisconnected?.Invoke(reason);
    }
    
    public void HandleInput(List<NetCommand> commands)
    {
        SessionManager.HandleInput(commands);
    }
    
    public void Update(GameTime gameTime)
    {
        SessionManager.Update(gameTime);
        CurrentScene?.Update(gameTime);
    }

    public void DrawScene(SpriteBatch spriteBatch)
    {
        CurrentScene?.DrawScene(spriteBatch);
    }

    public void DrawUI(SpriteBatch spriteBatch)
    {
        CurrentScene?.DrawUI(spriteBatch);
    }
}