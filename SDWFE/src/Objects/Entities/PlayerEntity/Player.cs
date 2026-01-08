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

        ConstructInventory();
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        ConstructStats();
        
        Sprite.Color = GameState.Instance.SessionManager.CurrentSession?.LocalClientId == this.OwningClientId ? Color.Red : Color.Blue;
        
        // Only create UI for the locally owned player
        if (IsLocallyOwned())
        {
            GameState.Instance.CurrentScene?.UIRoot.AddChild(new UIHotbar(Inventory));
            GameState.Instance.CurrentScene?.UIRoot.AddChild(new UIStats(Stats));
            GameState.Instance.CurrentScene?.UIRoot.AddChild(new UIWeapons(Inventory));
        }
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        Sprite.BaseDrawLayer = (float)(0.8f / ExtendedGame.DrawResolution.Y) * (this.GlobalPosition.Y + 24); // 16 is half the height of the sprite
        UpdateInventory();
        UpdateMovement(gameTime);
    }
}