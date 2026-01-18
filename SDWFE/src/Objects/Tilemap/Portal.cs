using Engine;
using Engine.Hitbox;
using Engine.Input;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Scenes;


namespace SDWFE.Objects.Tiles
{
    public class Portal : GameObject
    {
        private readonly PortalData _portalData;
        private readonly SpriteFont labelFont;

        public AnimatedSprite Sprite { get; private set; }
        public TriggerHitbox Hitbox { get; private set; }

        private readonly HitboxManager _hitboxManager;
        private Texture2D _spriteIdleSheet;
        private Texture2D _spriteEnterSheet;

        public Portal(PortalData data, HitboxManager hitboxManager)
        {
            this._hitboxManager = hitboxManager;
            this.GlobalPosition = data.Position;
            this._portalData = data;
            labelFont = ExtendedGame.AssetManager.LoadFont("Upheavel", "Fonts/");
            _spriteIdleSheet = ExtendedGame.AssetManager.LoadTexture("TM_Portal_RodHakGames", "Tilemap/");
            _spriteEnterSheet = ExtendedGame.AssetManager.LoadTexture("TM_Portal_Entrance", "Tilemap/");
            
            var column = SceneData.levelsUnlocked >= _portalData.LevelIndex ? 0 : 1;
            
            AnimationData animData = new AnimationData(_spriteIdleSheet, column, 200f, true);
            
            this.Sprite = new AnimatedSprite(_spriteIdleSheet, 48, 32, 200f, true, true)
            {
                OriginType = OriginType.TopLeft,
            };
            this.Sprite.SetAnimation(animData);

            int height = Sprite._spriteHeight;
            int width = Sprite._spriteWidth;

            this.Hitbox = new TriggerHitbox(new Rectangle((int)data.Position.X, (int)data.Position.Y, width, height + 10));
            Hitbox.DetectsLayers = HitboxLayer.Player;
            _hitboxManager.AddTrigger(Hitbox);

            Hitbox.OnEnter += OnEnterPortal;
            this.Sprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 33));

            
            this.AddChild(Sprite);
        }
        private void OnEnterPortal(TriggerHitbox hitbox, object other, TriggerSide side)
        {
            bool isUnlocked = SceneData.levelsUnlocked >= _portalData.LevelIndex;
            if (Sprite.IsVisible == false || this.IsVisible == false || !isUnlocked) return;
            if (other is Player player)
            {
                player.IsVisible = false;
                InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);
            }
            this.RemoveChild(Sprite);
            Sprite = new AnimatedSprite(_spriteEnterSheet, 48, 32, 200f, false, true)
            {
                OriginType = OriginType.TopLeft,
            };
            this.Sprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 33));
            Sprite.AnimationCompleted += OnAnimationComplete;
            this.AddChild(Sprite);
        }

        private void OnAnimationComplete()
        {
            SceneData.levelIndex = _portalData.LevelIndex;
            // Switch to the appropriate Scene
            GameState.Instance.SwitchScene(GameplayScene.KEY);
        }
    }
}