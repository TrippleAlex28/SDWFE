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
using SDWFE.Objects.Entities.Items;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Tilemap;
using SDWFE.Objects.Tiles;

namespace SDWFE.Scenes;

public class GameplayScene : Scene
{
    public const string KEY = "GameplayScene";
    
    private const string INTRO_DIALOGUE = "Greatings mighty warrior, |p You were choosen by the emperor to battle the mighty wizard, who has cursed this land with a terrible spell. This curse will cause our empire to fall in an endless shadow, and we will never know peace again. You have to battle your way through the temple and fight countless enemies, and it will not be easy, but you are the only one who can do it. |p You will have to enter multiple levels of this temple, and fight your way through rooms and waves full of enemies to reach the evil wizzard at the top, who you will hopefully defeat. |p Good luck on your quest warrior, our fate depends on you.";

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
        string tilemaptoLoad = $"{SceneData.levelName}.tmj";
        map = new Tilemap(tilemaptoLoad, HitboxManager);
        ExtendedGame.LightShaderInstance.Enabled = true;
        
        SpawnPoint = map.SpawnPoint;
        map.RegisterHitboxes(HitboxManager);
        WaveManager waveManager = new WaveManager(map.Portals, map.Doors, map.Enemies, HitboxManager);
        
        this.AddObject(waveManager);
        this.LevelIndex = SceneData.levelIndex;
        // Hub level
        if (SceneData.levelIndex == -1)
        {
            foreach (var portalData in map.Portals)
            {
                var portal = new Portal(portalData, HitboxManager);
                
                this.AddObject(portal);
            }

            Vector2 shopkeeperPosition = new Vector2(50, 100);
            Texture2D shopkeeperTexture = ExtendedGame.AssetManager.LoadTexture("32x16 Idle-Sheet", "Entities/NPC/");
            ShopKeeper shopkeeper = new ShopKeeper(new Rectangle((int)shopkeeperPosition.X - 12, (int)shopkeeperPosition.Y - 12, 56, 56), shopkeeperTexture, HitboxManager);
            shopkeeper.GlobalPosition = shopkeeperPosition;
            this.AddObject(shopkeeper);
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
        HitboxManager.UpdateDebug();
        ExtendedGame.LightShaderInstance.SetLights(allWorldLights);
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_INTERACT))
            Coins.CreateRandomDrop(new Vector2(100, 100), HitboxManager);
         // Handle pause input
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_PAUSE))
            GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
    }
    public override void DrawScene(SpriteBatch spriteBatch)
    {
        base.DrawScene(spriteBatch);
        HitboxManager.DrawDebug(spriteBatch);
    }
}