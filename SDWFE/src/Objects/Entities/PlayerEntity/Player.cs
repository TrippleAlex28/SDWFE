using System;
using Engine;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;
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
        
        // Animated Sprite Setup
        Texture2D spriteSheet = ExtendedGame.AssetManager.LoadTexture("16x32 Idle v2-Sheet", "Entities/Player/");
        Sprite = new AnimatedSprite(spriteSheet, 16, 32)
        {
            SourceRectangle = new Rectangle(new Point(0, 0), new Point(16, 32)),
            OriginType = OriginType.TopLeft,
        };

        this.AddChild(Sprite);
        this.CameraOffset = new Vector2(8, 16); // hardcoded numbers from the spritesheet, because brain fog

        ConstructStats();
        ConstructInventory();
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        Sprite.Color = GameState.Instance.SessionManager.CurrentSession?.LocalClientId == this.OwningClientId ? Color.Red : Color.Blue;
        
        GameState.Instance.CurrentScene?.UIRoot.AddChild(new UIHotbar(Inventory));
        GameState.Instance.CurrentScene?.UIRoot.AddChild(new UIStats(Stats));
        GameState.Instance.CurrentScene?.UIRoot.AddChild(new UIWeapons(Inventory));
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

        UpdateInventory();
        UpdateMovement(gameTime);
    }
}