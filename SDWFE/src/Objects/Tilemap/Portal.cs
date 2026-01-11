using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Scenes;


namespace SDWFE.Objects.Tiles
{
    public class Portal : GameObject
    {
        private readonly int _levelIndex;
        private Text label;
        private readonly SpriteFont labelFont;

        public AnimatedSprite Sprite { get; private set; }
        public TriggerHitbox Hitbox { get; private set; }

        private readonly HitboxManager _hitboxManager;
        private Texture2D _spriteIdleSheet;
        private Texture2D _spriteEnterSheet;

        public Portal(Vector2 globalPosition, HitboxManager hitboxManager)
        {
            this._hitboxManager = hitboxManager;
            this.GlobalPosition = globalPosition;
            labelFont = ExtendedGame.AssetManager.LoadFont("Upheavel", "Fonts/");
            _spriteIdleSheet = ExtendedGame.AssetManager.LoadTexture("TM_Portal_RodHakGames", "Tilemap/");
            _spriteEnterSheet = ExtendedGame.AssetManager.LoadTexture("TM_Portal_Entrance", "Tilemap/");
            this.Sprite = new AnimatedSprite(_spriteIdleSheet, 48, 32, true, true)
            {
                OriginType = OriginType.TopLeft,
            };

            int height = Sprite._spriteHeight;
            int width = Sprite._spriteWidth;

            this.Hitbox = new TriggerHitbox(new Rectangle((int)globalPosition.X, (int)globalPosition.Y, width, height));
            Hitbox.DetectsLayers = HitboxLayer.Player;
            _hitboxManager.AddTrigger(Hitbox);

            Hitbox.OnEnter += OnEnterPortal;
            this.Sprite.BaseDrawLayer = (float)(0.8f / 1000f) * (globalPosition.Y + 32);

            
            this.AddChild(Sprite);
        }
        private void OnEnterPortal(TriggerHitbox hitbox, object other, TriggerSide side)
        {
            if (other is Player player)
            {
                player.IsVisible = false;
            }
            this.RemoveChild(Sprite);
            Sprite = new AnimatedSprite(_spriteEnterSheet, 48, 32, false, true)
            {
                OriginType = OriginType.TopLeft,
            };
            this.Sprite.BaseDrawLayer = (float)(0.8f / 1000f) * (GlobalPosition.Y + 32);
            Sprite.AnimationCompleted += OnAnimationComplete;
            this.AddChild(Sprite);
        }

        private void OnAnimationComplete()
        {
            // Switch to the appropriate Scene
            GameState.Instance.SwitchScene(GameplayScene.KEY);
        }

        // public bool PortalUnlocked()
        // {
        //     LevelProgress.LoadProgress();

        //     if (LevelProgress.GetLevelStatus(_levelIndex + 1) == LevelStatus.Unlocked)
        //         return true;
        //     else
        //         return false;
        // }

        // private void AddPortalText()
        // {
        //     string portalText = "Level " + this._levelIndex;

        //     label = new(
        //         label: portalText,
        //         font: labelFont,
        //         color: Color.Black,
        //         offset: new Vector2(0, -10),
        //         spriteWidth: 24
        //     );
        //     this.AddChild(label);
        // }

        // private float time = 0;

        // public void AnimatePortal(GameTime gameTime)
        // {
        //     time += (float)gameTime.ElapsedGameTime.TotalSeconds;

        //     if (time > 0.8) time = 0;

        //     if (time >= 0 && time <= 0.2)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 0);
        //     if (time > 0.2 && time <= 0.4)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 1);
        //     if (time > 0.4 && time <= 0.6)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 2);
        //     if (time > 0.6 && time <= 0.8)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 3);
        // }

        // public void PortalEntrence(GameTime gameTime)
        // {
        //     Texture2D spriteSheetRun = ExtendedGame.AssetManager.LoadTexture("Portal Entrance-Sheet", "Entity/Player/");

        //     time += (float)gameTime.ElapsedGameTime.TotalSeconds;

        //     Sprite.Texture = spriteSheetRun;

        //     if (time >= 0 && time <= 0.2)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 0);
        //     if (time > 0.2 && time <= 0.4)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 1);
        //     if (time > 0.4 && time <= 0.6)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 2);
        //     if (time > 0.6 && time <= 0.8)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 3);
        //     if (time > 0.8 && time <= 1)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 4);
        //     if (time > 1 && time <= 1.2)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 5);
        //     if (time > 1.2 && time <= 1.4)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 6);
        //     if (time > 1.4 && time <= 1.6)
        //         Sprite.SourceRectangle = Sprite.GetSpriteFromSheet(0, 7);
        //     if (time > 1.6 && time <= 1.8)
        //     {
        //         switch (_levelIndex)
        //         {
        //             case 0:
        //                 SceneManager.Instance.LoadScene(SceneRegistry.HubLevel);
        //                 break;
        //             case 1:
        //                 SceneManager.Instance.LoadScene(SceneRegistry.Level1);
        //                 break;
        //             default:
        //                 SceneManager.Instance.LoadScene(SceneRegistry.Level1);
        //                 break;
        //         }
        //     }
        // }
    }
}