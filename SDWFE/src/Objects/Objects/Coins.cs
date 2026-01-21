using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;
using System;

namespace SDWFE.Objects.Entities.Items
{
    public class Coins : GameObject
    {
        public Sprite Sprite { get; private set; }
        public TriggerHitbox Hitbox { get; private set; }
        private HitboxManager _hitboxManager;


        private float zHeight = 0f;
        private float zVelocity = 180f;
        private float zGravity = 600f;
        private float groundY;
        private int bounces = 0;

        private Vector2 randomDirection;
        private int randomVelocity;
        private int randomHeightVelocity;


        public Coins(Vector2 dropPosition, Vector2 initialVelocity, HitboxManager hitboxManager)
        {
            _hitboxManager = hitboxManager;
            var texture = ExtendedGame.AssetManager.LoadTexture("16x16 Bronze Coin", "Items/");
            Sprite = new(texture)
            {
                OriginType = OriginType.Center
            };
            Sprite.Scale = new Vector2(0.5f, 0.5f);
            AddChild(Sprite);
            this.CollisionSize = new Vector2(Sprite.SourceRectangle.Width, Sprite.SourceRectangle.Height) / 2;
            this.CollisionOffset = new Vector2(0, 0);
            this.HitboxManager = hitboxManager;
            this.HitboxLayer = HitboxLayer.Enemy;
            this.Hitbox = new TriggerHitbox(new Rectangle(
                (int)(dropPosition.X - CollisionSize.X / 2),
                (int)(dropPosition.Y - CollisionSize.Y / 2),
                (int)CollisionSize.X * 10,
                (int)CollisionSize.Y* 10))
            {
                DetectsLayers = HitboxLayer.Player
            };
            this.Hitbox.OnEnter += OnPlayerCollect;

            this.GlobalPosition = dropPosition;
            this.randomDirection = new Vector2((float)(ExtendedGame.Random.NextDouble() * 2 - 1), (float)(ExtendedGame.Random.NextDouble() * 2 - 1));
            this.randomVelocity = ExtendedGame.Random.Next(50, 200);
            this.randomHeightVelocity = ExtendedGame.Random.Next(50, 180);
            this.groundY = dropPosition.Y;

            GameState.Instance.CurrentScene?.AddObject(this);
            hitboxManager.AddTrigger(Hitbox);
        }

        protected override void UpdateSelf(GameTime gameTime)
        {
            // make it here bounce
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update vertical velocity and height
            zVelocity -= zGravity * dt;
            zHeight += zVelocity * dt;

            // Bounce when hitting the ground
            if (zHeight < 0f && bounces < 5)
            {
                zHeight = 0f;
                zVelocity = randomHeightVelocity * (1f / (bounces + 1)); // reset upward velocity for infinite bounce
                bounces++;
            }
            // Update horizontal position in a circular path
            if (bounces < 5){
                this.Direction = randomDirection;
                this.Velocity = randomVelocity * (1f / (bounces + 1));
                // Visual offset for height
                Sprite.LocalPosition = new Vector2(0, -zHeight);

                // Y-sort for draw order
                Sprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 16));

                // Update hitbox position
                Hitbox.Bounds = new Rectangle(
                    (int)(GlobalPosition.X - (CollisionSize.X * 3) / 2),
                    (int)(GlobalPosition.Y - (CollisionSize.Y * 3) / 2),
                    (int)CollisionSize.X * 3,
                    (int)CollisionSize.Y * 3
                );
            }
            
        }



        protected override void ExitSelf()
        {
            _hitboxManager.RemoveTrigger(Hitbox);
            base.ExitSelf();
        }

        public static Coins CreateRandomDrop(Vector2 dropPosition, HitboxManager hitboxManager)
        {
            float horizontalSpeed = (float)(ExtendedGame.Random.NextDouble() * 50 - 25);
            float verticalSpeed = (float)(ExtendedGame.Random.NextDouble() * -50 - 50);

            Vector2 initialVelocity = new Vector2(horizontalSpeed, verticalSpeed);
            return new Coins(dropPosition, initialVelocity, hitboxManager);
        }

        private void OnPlayerCollect(TriggerHitbox hitbox, object other, TriggerSide side)
        {
            if (other is Player player)
            {
                SoundEffect collectSound = ExtendedGame.AssetManager.LoadSoundEffect("CoinPickup", "SFX/");
                collectSound.Play(volume: 0.5f, pitch: 0.0f, pan: 0.0f);
                
                player.Stats.Coins += 1;
                this.RemoveSelf();
            }
        }
    }
}