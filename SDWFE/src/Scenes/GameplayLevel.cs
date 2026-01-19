using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Engine.Input;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Managers;
using SDWFE.Objects;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Tilemap;
using SDWFE.Objects.Tiles;

namespace SDWFE.Scenes.Levels;

public abstract class GameplayLevel : Scene
{
    private string _levelSuffix;
    public GameplayLevelManager LevelManager { get; set; }

    protected Tilemap map;
    protected WaveManager waveManager;
    
    public GameplayLevel(string key, string levelSuffix) : base(key)
    {
        _levelSuffix = $"{levelSuffix}.tmj";
        BackgroundColor = new Color(22, 17, 11);
        SetDefaultPlayerClass<Player>(() => new Player());
        
        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_GAMEPLAY);
    }

    public override void Enter()
    {
        base.Enter();
        
        ExtendedGame.LightShaderInstance.Enabled = true;
        
        #region Tilemap & Hitboxes Loading
        map = new Tilemap(_levelSuffix, HitboxManager);
        SpawnPoint = map.SpawnPoint;
        map.RegisterHitboxes(HitboxManager);
        
        waveManager = new WaveManager(this, map.Portals, map.Doors, map.Enemies, HitboxManager);
        this.AddObject(waveManager);
                
        // Manage hub level special wave case
        if (LevelIndex is -1 or 0)
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
        }
        else
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
        
        SetUpHitboxes();
        
        this.AddObject(map);
        #endregion
        
        // Only run on server
        if (GameState.Instance.SessionManager.IsHost || GameState.Instance.SessionManager.IsSingleplayer)
        {
            LevelManager = new GameplayLevelManager();
            AddObject(LevelManager);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Set up hitboxes for any new players that may have joined
        SetUpHitboxes();
        
        // Update triggers for ALL players, not just the local one
        List<PointLight> allWorldLights = new List<PointLight>();
        foreach (var playerObject in GetAllPawns())
        {
            if (playerObject is not Player player) continue;
            
            // Update lighting shader with all world lights
            allWorldLights.Add(new PointLight()
            {
                WorldPosition = player.GlobalPosition + new Vector2(8, 16),
                WorldRadius = 150f,
                LightColor = Color.White * 0.5f
            });
                
            // Update player hitbox
            Rectangle playerHitbox = new Rectangle(
                (int)player.GlobalPosition.X, 
                (int)player.GlobalPosition.Y + 24, 
                16, 
                8
            );
            HitboxManager.UpdateTriggersForObject(player, playerHitbox, HitboxLayer.All);
        }
        
        HitboxManager.UpdateDebug();
        
        ExtendedGame.LightShaderInstance.SetLights(allWorldLights);
    }

    public override void DrawScene(SpriteBatch spriteBatch)
    {
        base.DrawScene(spriteBatch);
        HitboxManager.DrawDebug(spriteBatch);
    }

    #region Helpers
    
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
    
    #endregion
}