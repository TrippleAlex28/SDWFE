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
using SDWFE.Managers;
using SDWFE.Objects.Entities;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Tilemap;
using SDWFE.Objects.Tiles;

namespace SDWFE.Scenes;

public class GameplayScene : Scene
{
    public const string KEY = "GameplayScene";
    
    private const string INTRO_DIALOGUE = "To the Chosen Warrior, |p In the ninth year of my reign, beneath Heaven's watchful gaze, I write to you with a burdened heart. The year is 967, and a dark curse has fallen upon our kingdom. Fields grow silent, rivers run uneasy, and the people whisper of ill fate. This calamity was wrought by a wandering wizard, learned in forbidden arts, whose magic now binds our land in suffering.|p By the Mandate of Heaven, I command you-brave warrior-to journey beyond our borders and seek this wizard. Face his trials, endure his deceptions, and compel him to lift the curse that shackles our realm.|p The fate of the kingdom rests upon your blade and your resolve.|p Return with victory, and your name shall be etched in history. Fail, and our dynasty may fade into shadow.|p May the spirits guide your path,and may Heaven grant you strength.|p-The King";

    private ParticleSystem _bulletTrailSystem = new();
    private Tilemap map;
    public GameplayScene() : base(KEY)
    {
        // Set dynamic background color based on the session type
        var currentSession = GameState.Instance.SessionManager.CurrentSession;
        BackgroundColor = new Color(22, 17, 11);
        // BackgroundColor = currentSession != null 
        //     ? currentSession.Type == SessionType.Singleplayer
        //         ? Color.Green
        //         : currentSession.Type == SessionType.MultiplayerClient
        //             ? Color.Red
        //             : Color.Blue
        //     : Color.Black;
        
        SetDefaultPlayerClass<Player>(() => new Player());

        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_GAMEPLAY);
    }

    public override void Enter()
    {
        base.Enter();

        #region Load Tilemap and Setup
        string tilemaptoLoad = $"{SceneData.LevelName}.tmj";
        map = new Tilemap(tilemaptoLoad, HitboxManager);
        ExtendedGame.LightShaderInstance.Enabled = true;
        
        SpawnPoint = map.SpawnPoint;
        map.RegisterHitboxes(HitboxManager);
        WaveManager waveManager = new WaveManager(map.Portals, map.Doors, map.Enemies, HitboxManager);
        
        this.AddObject(waveManager);

        if (SceneData.LevelIndex == -1)
        {
            foreach (var portalData in map.Portals)
            {
                var portal = new Portal(portalData, HitboxManager);
                
                this.AddObject(portal);
            }
        } else 
        {
            waveManager.StartWaves();
        }
        
        // Spawn NPCs
        foreach (var npcData in map.NPCs)
        {
            NPC newNPC = new NPC(npcData.RootNode, new Rectangle((int)npcData.Position.X - 12, (int)npcData.Position.Y - 12, 56, 56), npcData.Texture, HitboxManager);
            newNPC.GlobalPosition = npcData.Position;

            this.AddObject(newNPC);
        }
            
        if (GameState.Instance.SessionManager.IsHost || GameState.Instance.SessionManager.IsSingleplayer)
        {
            var grunt = new Grunt()
            {
                GlobalPosition = new Vector2(350, 350),
            };
            AddObject(grunt);
        }
        
        SetUpHitboxes();
        
        this.AddObject(map);
        #endregion
        
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
                if (!SceneData.hasSeenIntro)
                {
                    SceneData.hasSeenIntro = true;
                    player.ShowDialogue(INTRO_DIALOGUE);
                }
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