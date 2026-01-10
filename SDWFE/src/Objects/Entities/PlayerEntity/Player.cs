using System;
using Engine;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;
using SDWFE.UI.Dialogue;
using SDWFE.UI.Inventory;
using SDWFE.UI.PlayerData;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player : GameObject
{
    public override uint TypeId => (uint)NetObjects.Player;
    
    public AnimatedSprite Sprite { get; }

    public Player()
    {
        this.ReplicatesOverNetwork = true;
        
        RegisterProperty(
            nameof(Stats.MaxHealth),
            () => Stats.MaxHealth,
            (v) => Stats.MaxHealth = v
        );
        
        RegisterProperty(
            nameof(Stats.CurrentHealth),
            () => Stats.CurrentHealth,
            (v) => Stats.CurrentHealth = v
        );
        
        RegisterProperty(
            nameof(Stats.MaxStamina),
            () => Stats.MaxStamina,
            (v) => Stats.MaxStamina = v
        );
        
        RegisterProperty(
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

        ConstructInventory();
        ConstructDialogue();
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        Sprite.Color = GameState.Instance.SessionManager.CurrentSession?.LocalClientId == this.OwningClientId ? Color.Blue : Color.Red;
        
        // Only create UI for the locally owned player
        if (IsLocallyOwned())
        {
            StatsUI = new UIStats(this);
            GameState.Instance.CurrentScene?.UIRoot.AddChild(HotbarUI);
            GameState.Instance.CurrentScene?.UIRoot.AddChild(StatsUI);
            GameState.Instance.CurrentScene?.UIRoot.AddChild(WeaponsUI);
            
            if (_dialogue != null)
                GameState.Instance.CurrentScene?.UIRoot.AddChild(_dialogue);
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
            Sprite.BaseDrawLayer = (float)(0.8f / 1000f) * (this.GlobalPosition.Y + 24);
        }
        
        UpdateInventory();
        UpdateMovement(gameTime);
        UpdateDialogue(gameTime);
    }
}