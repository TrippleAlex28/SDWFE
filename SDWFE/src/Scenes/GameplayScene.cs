using System;
using Engine;
using Engine.Hitbox;
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
    private HitboxManager _hitboxManager = new();
    private Tilemap map;
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
        map = new Tilemap("level_hub.tmj");
        
        map.SetHitboxes(_hitboxManager);
        SetUpHitboxes();
        this.AddObject(map);
        _bulletTrailSystem.AddEmitter(ParticlePresets.BulletTrail);
    }
    private void SetUpHitboxes()
    {
        var session = GameState.Instance.SessionManager.CurrentSession;
        if (session == null) return;
        
        var playerObject = GetPawn(session.LocalClientId);
        if (playerObject is Player player)
        {
            player.HitboxManager = _hitboxManager;
            player.HitboxLayer = HitboxLayer.Player;
            player.CollisionSize = new Vector2(16, 8);
            player.CollisionOffset = new Vector2(0, 24);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _bulletTrailSystem.Update(gameTime.DeltaSeconds());
        // Access the local player
        var session = GameState.Instance.SessionManager.CurrentSession;
        if (session != null)
        {
            var playerObject = GetPawn(session.LocalClientId);
            if (playerObject is Player player)
            {
                if (playerObject.HitboxManager == null)
                {
                    SetUpHitboxes();
                }
                
                Rectangle playerHitbox = new Rectangle((int)player.GlobalPosition.X, (int)player.GlobalPosition.Y + 24, 16, 8);

                _hitboxManager.UpdateTriggersForObject(player, playerHitbox, HitboxLayer.All);
            }
        }
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_PAUSE))
            GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
    }

    public override void DrawScene(SpriteBatch spriteBatch)
    {
        base.DrawScene(spriteBatch);
        
        _bulletTrailSystem.Draw(spriteBatch);
    }
}