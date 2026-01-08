using System;
using Engine;
using Engine.Input;
using Engine.Network.Shared.Session;
using Engine.Particle;
using Engine.Scene;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Scenes;

public class GameplayScene : Scene
{
    public const string KEY = "GameplayScene";

    private ParticleSystem _bulletTrailSystem = new();
    
    public GameplayScene() : base(KEY)
    {
        // Set dynamic background color based on the session type
        var currentSession = GameState.Instance.SessionManager.CurrentSession;
        BackgroundColor = currentSession != null 
            ? currentSession.Type == SessionType.Singleplayer
                ? Color.Green
                : currentSession.Type == SessionType.MultiplayerClient
                    ? Color.Red
                    : Color.Blue
            : Color.Black;
        
        SetDefaultPlayerClass<Player>(() => new Player());

        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_GAMEPLAY);
    }

    public override void Enter()
    {
        base.Enter();
        Tilemap map = new Tilemap("TestMap.tmj");
        this.AddObject(map);
        _bulletTrailSystem.AddEmitter(ParticlePresets.BulletTrail);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _bulletTrailSystem.Update(gameTime.DeltaSeconds());
        
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_PAUSE))
            GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
    }

    public override void DrawScene(SpriteBatch spriteBatch)
    {
        base.DrawScene(spriteBatch);
        
        _bulletTrailSystem.Draw(spriteBatch);
    }
}