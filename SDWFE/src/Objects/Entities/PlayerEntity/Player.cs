using System;
using System.Globalization;
using Engine;
using Engine.Sprite;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;
using SDWFE.Scenes.Levels;
using SDWFE.UI.PlayerData;
using SDWFE.UI.Dialogue;

namespace SDWFE.Objects.Entities.PlayerEntity;

public enum LifeState : byte
{
    Alive = 0,
    Dead = 1,
}

public partial class Player : GameObject
{
    public override uint TypeId => (uint)NetObjects.Player;
    
    public AnimatedSprite Sprite { get; }

    private Texture2D _hurtTexture = ExtendedGame.AssetManager.LoadTexture("32x16 Hurt-Sheet", "Entities/Player/");
    private Texture2D _runSheet = ExtendedGame.AssetManager.LoadTexture("32x16 Run-Sheet", "Entities/Player/");

    public LifeState State = LifeState.Alive;
    public float RespawnTimer;

    private UIContainer _deathContainer;
    
    public Player()
    {
        this.ReplicatesOverNetwork = true;
        
        RegisterProperty(
            0,
            nameof(IsVisible),
            () => IsVisible,
            (v) => IsVisible = v
        );
        
        RegisterProperty(
            1,
            nameof(GlobalPosition),
            () => GlobalPosition,
            (v) => GlobalPosition = v
        );
        
        RegisterProperty(
            2,
            nameof(Direction),
            () => Direction,
            (v) => Direction = v
        );
        
        RegisterProperty(
            3,
            nameof(Velocity),
            () => Velocity,
            (v) => Velocity = v
        );
        
        RegisterProperty(
            4,
            nameof(Stats.MaxHealth),
            () => Stats.MaxHealth,
            (v) => Stats.MaxHealth = v
        );
        
        RegisterProperty(
            5,
            nameof(Stats.CurrentHealth),
            () => Stats.CurrentHealth,
            (v) => Stats.CurrentHealth = v
        );
        
        RegisterProperty(
            6,
            nameof(Stats.MaxStamina),
            () => Stats.MaxStamina,
            (v) => Stats.MaxStamina = v
        );
        
        RegisterProperty(
            7,
            nameof(Stats.CurrentStamina),
            () => Stats.CurrentStamina,
            (v) => Stats.CurrentStamina = v
        );
        
        RegisterProperty(
            8,
            nameof(State),
            () => (byte)State,
            (v) => State = (LifeState)v
        );
        
        RegisterProperty(
            9,
            nameof(RespawnTimer),
            () => RespawnTimer,
            (v) => RespawnTimer = v
        );
        
        // Animated Sprite Setup
        Texture2D spriteSheet = ExtendedGame.AssetManager.LoadTexture("16x32 Idle v2-Sheet", "Entities/Player/");
        Sprite = new AnimatedSprite(spriteSheet, 16, 32)
        {
            SourceRectangle = new Rectangle(new Point(0, 0), new Point(16, 32)),
            OriginType = OriginType.TopLeft,
        };

        this.AddChild(Sprite);
        this.CameraOffset = new Vector2(8, 16); // hardcoded numbers from the spritesheet, because brain fog

        InitializeAnimations();
        ConstructInventory();
        // Note: ConstructDialogue is called in EnterSelf for locally owned players only
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        Sprite.Color = GameState.Instance.SessionManager.CurrentSession?.LocalClientId == this.OwningClientId ? Color.Blue : Color.Red;
        
        // Only create UI for the locally owned player
        if (IsLocallyOwned())
        {
            ConstructDialogue();
            
            ConstructShopUI();
            ConstructAbilities();
            ConstructStats(); // Must be after ConstructAbilities (needs Inventory)
            
            // Add inventory UI
            GameState.Instance.CurrentScene?.UIRoot.AddChild(InventoryUI);

            StatsUI = new UIStats(this);
            StatsUI.UpdateStats(); // Initial update
            Stats.OnStatsChanged += OnStatsChanged;
            Stats.OnDeath += OnDeath;
            
            GameState.Instance.CurrentScene?.UIRoot.AddChild(StatsUI);
            GameState.Instance.CurrentScene?.UIRoot.AddChild(ShopUI);
            
            this.HitboxLayer = Engine.Hitbox.HitboxLayer.Player;
            if (_dialogue != null)
                GameState.Instance.CurrentScene?.UIRoot.AddChild(_dialogue);
            
            if (_dialogueChoice != null)
                GameState.Instance.CurrentScene?.UIRoot.AddChild(_dialogueChoice);

            #region Death/Respawn UI

            _deathContainer = new UIContainer();
            _deathContainer.IsVisible = false;
            _deathContainer.DesiredSize = UIExtensionMethods.GetScreenPercentage(100);
            _deathContainer.MinSize = _deathContainer.DesiredSize;
            _deathContainer.MaxSize = _deathContainer.DesiredSize;
            _deathContainer.ChildAlignment = Alignment.MiddleCenter;
            GameState.Instance.CurrentScene?.UIRoot.AddChild(_deathContainer);

            var timeLeft = UIVisual.FromText($"Respawning in: {((int)RespawnTimer).ToString()}", Resources.TextFont, Color.White);
            _deathContainer.AddChild(timeLeft);
            
            #endregion
        }
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        // Use stair-based Y-sort when on stairs, otherwise use position-based
        if (IsOnStairs && StairYSort > 0f)
        {
            Sprite.BaseDrawLayer = StairYSort;
        }
        else
        {
            Sprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 31));
        }
        
        UpdateNPC(gameTime);
        UpdateInventory();
        UpdateAbilities(gameTime);
        UpdateShop();
        UpdateMovement(gameTime);
        UpdateDialogue(gameTime);
        UpdateWeapons(gameTime);
        
        UpdateRespawn(gameTime);
    }
    
    private void OnDeath()
    {
        // Only run on server
        if (!GameState.Instance.SessionManager.IsHost && !GameState.Instance.SessionManager.IsSingleplayer) return;

        Stats.Coins = Math.Max(0, Stats.Coins - 50);
        
        State = LifeState.Dead;
        RespawnTimer = 30f;
        
        // Refill stats
        Stats.CurrentHealth = Stats.MaxHealth;
        Stats.CurrentStamina = Stats.MaxStamina;
        
        // TODO: Hide certain ui menus
        _deathContainer.IsVisible = true;
    }

    private void UpdateRespawn(GameTime gameTime)
    {
        // Only run on server
        if (!GameState.Instance.SessionManager.IsHost && !GameState.Instance.SessionManager.IsSingleplayer) return;
        
        // Check if player is dead
        if (State != LifeState.Dead) return;
        
        // Save scene
        GameplayLevel level;
        try
        {
            level = (GameplayLevel)GameState.Instance.CurrentScene;
        }
        catch (Exception e)
        {
            return;
        }
        
        if (level.LevelManager.LevelFailed) return;

        bool shouldRespawn = false;
        foreach (var obj in level.GetAllPawns())
        {
            if (obj is not Player player) continue;
            if (player.State != LifeState.Alive) continue;
            
            shouldRespawn = true;
            break;
        }

        if (!shouldRespawn)
        {
            level.LevelManager.LevelFailed = true;
            level.LevelManager.FailReason = LevelFailReason.AllDead;
            
            // TODO: Add some kind of smooth transition
            GameState.Instance.SwitchScene(GameOverScene.KEY);
            
            return;
        }

        // Respawn player when enough ticks have passed
        RespawnTimer -= gameTime.DeltaSeconds();
        if (RespawnTimer <= 0f)
        {
            _deathContainer.IsVisible = false;
            
            State = LifeState.Alive;
            this.GlobalPosition = level.SpawnPoint;
            this.Velocity = 0f;
        }
        Console.WriteLine($"Update Respawn: {RespawnTimer}, {gameTime.DeltaSeconds()}");
    }
}