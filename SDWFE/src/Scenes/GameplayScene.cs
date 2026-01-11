using System;
using System.Collections.Generic;
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
using SDWFE.Objects.Tilemap;

namespace SDWFE.Scenes;

public class GameplayScene : Scene
{
    public const string KEY = "GameplayScene";

    private ParticleSystem _bulletTrailSystem = new();
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
        
        map.RegisterHitboxes(HitboxManager);
        Vector2 spawnPointNPC = new Vector2(300, 300);
        NPC npc = new NPC("fireman_root", new Rectangle((int)spawnPointNPC.X - 12, (int)spawnPointNPC.Y - 12, 56, 56), ExtendedGame.AssetManager.LoadTexture("32x32 Han_Soldier_Idle", "Entities/NPC/"), HitboxManager);
        npc.GlobalPosition = spawnPointNPC;
        this.AddObject(npc);
        SetUpHitboxes();
        this.AddObject(map);
        _bulletTrailSystem.AddEmitter(ParticlePresets.BulletTrail);
    }
    private void SetUpHitboxes()
    {
        // Set up hitboxes for ALL players, not just the local one
        foreach (var playerObject in GetAllPawns())
        {
            if (playerObject is Player player && player.HitboxManager == null)
            {
                player.HitboxManager = HitboxManager;
                player.HitboxLayer = HitboxLayer.Player;
                player.CollisionSize = new Vector2(16, 8);
                player.CollisionOffset = new Vector2(0, 24);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _bulletTrailSystem.Update(gameTime.DeltaSeconds());
        
        List<PointLight> allWorldLights = new List<PointLight>();
        // Set up hitboxes for any new players that may have joined
        SetUpHitboxes();
        // Update triggers for ALL players, not just the local one
        foreach (var playerObject in GetAllPawns())
        {
            if (playerObject is Player player)
            {
                // Update lighting shader with all world lights
                allWorldLights.Add(new PointLight()
                {
                    WorldPosition = player.GlobalPosition + new Vector2(8, 16),
                    WorldRadius = 150f,
                    LightColor = Color.White * 0.5f
                });
                Rectangle playerHitbox = new Rectangle(
                    (int)player.GlobalPosition.X, 
                    (int)player.GlobalPosition.Y + 24, 
                    16, 
                    8
                );
                HitboxManager.UpdateTriggersForObject(player, playerHitbox, HitboxLayer.All);
            }
        }
        ExtendedGame.LightShaderInstance.SetLights(allWorldLights);
        
         // Handle pause input
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_PAUSE))
            GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
    }

    public override void DrawScene(SpriteBatch spriteBatch)
    {
        base.DrawScene(spriteBatch);
        
        _bulletTrailSystem.Draw(spriteBatch);
    }
}