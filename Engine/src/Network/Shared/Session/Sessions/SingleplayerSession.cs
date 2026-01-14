using Engine.Network.Shared.Command;
using Microsoft.Xna.Framework;

namespace Engine.Network.Shared.Session.Sessions;

public class SingleplayerSession : GameSession
{
    private List<NetCommand> _frameCommands = new();

    public SingleplayerSession() : base(SessionType.Singleplayer)
    {
        // Should remain unused in a SingleplayerSession but whatever
        LocalClientId = 0;
    }
    
    public override void Initialize()
    {
        GameState.Instance.CurrentScene?.AddPlayer(LocalClientId);
        
        base.Initialize();
    }

    public override void Reset()
    {
        base.Reset();
    }

    public override void HandleInput(List<NetCommand> commands)
    {
        _frameCommands = commands;
    }

    public override void Update(GameTime gameTime)
    {
        Scene.Scene? scene = GameState.Instance.CurrentScene;
        if (scene == null) return;
        
        foreach (var c in _frameCommands)
        {
            c.Apply(scene, LocalClientId);
        }
    }

    public override void OnSwitchScene(uint sceneEpoch, string sceneKey, int levelIndex = -1)
    {
        GameState.Instance.CurrentScene!.AddPlayer(LocalClientId);
    }
}