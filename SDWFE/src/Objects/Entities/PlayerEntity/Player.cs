using System;
using Engine;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;
using SDWFE.UI.PlayerData;
using SDWFE.UI.Dialogue;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player : GameObject
{
    public override uint TypeId => (uint)NetObjects.Player;
    
    public AnimatedSprite Sprite { get; }

    private Texture2D _hurtTexture = ExtendedGame.AssetManager.LoadTexture("32x16 Hurt-Sheet", "Entities/Player/");
    private Texture2D _runSheet = ExtendedGame.AssetManager.LoadTexture("32x16 Run-Sheet", "Entities/Player/");
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
            GameState.Instance.CurrentScene?.UIRoot.AddChild(StatsUI);
            GameState.Instance.CurrentScene?.UIRoot.AddChild(ShopUI);
            
            this.HitboxLayer = Engine.Hitbox.HitboxLayer.Player;
            if (_dialogue != null)
                GameState.Instance.CurrentScene?.UIRoot.AddChild(_dialogue);
            
            if (_dialogueChoice != null)
                GameState.Instance.CurrentScene?.UIRoot.AddChild(_dialogueChoice);
        }
    }

    protected void OnDeath()
    {
        
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
    }
}