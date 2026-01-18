using Engine;
using Engine.Hitbox;
using Engine.Input;
using Engine.Network.Shared.Session;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Scenes;
using SDWFE.Scenes.Levels;


namespace SDWFE.Objects.Tiles
{
    public class Portal : GameObject
    {
        private PortalData _portalData;
        private readonly SpriteFont labelFont;

        public AnimatedSprite Sprite { get; private set; }
        public TriggerHitbox Hitbox { get; private set; }

        private readonly HitboxManager _hitboxManager;
        private Texture2D _spriteIdleSheet;
        private Texture2D _spriteEnterSheet;

        public Portal(PortalData data, HitboxManager hitboxManager)
        {
            // TODO: Think about this, only the host switches scenes so honestly only the host has to see the portal. Not replicating the portal saves bandwidth as well, but uncomment the part underneath and it still works
            // this.ReplicatesOverNetwork = true;
            // RegisterProperty(
            //     101,
            //     nameof(_portalData.LevelIndex),
            //     () => _portalData.LevelIndex,
            //     (v) => _portalData.LevelIndex = v
            // );
            
            this._hitboxManager = hitboxManager;
            this._portalData = data;
            
            this.GlobalPosition = data.Position;
            
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
            // Prevent multiplayer client from getting stuck in the portal & trying to change the scene
            if (GameState.Instance.SessionManager.IsClient) return;
            
            // Prevent interaction when level is locked
            bool isUnlocked = SceneData.levelsUnlocked >= _portalData.LevelIndex;
            if (!Sprite.IsVisible || !this.IsVisible || !isUnlocked) return;
            
            // Hide player and lock movement
            if (other is Player player)
            {
                player.IsVisible = false;
                InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);
            }
            
            // Run player sprite walk in animation & switch scenes on finish
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
            // Switch to the appropriate Scene
            SceneData.levelIndex = _portalData.LevelIndex;
            if (_portalData.LevelIndex == -1 || _portalData.LevelIndex == 0)
            {
                GameState.Instance.SwitchScene(HubLevel.KEY);
            }
            else
            {
                GameState.Instance.SwitchScene($"Level{_portalData.LevelIndex}");
            }
        }
    }
}