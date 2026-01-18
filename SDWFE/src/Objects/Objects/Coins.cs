using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
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

        private Vector2 velocity;
        private float gravity = 800f;
        private float bounceDamping = 0.6f;
        private float groundY;
        private bool isDropping = true;
        private int bounceCount = 0;
        private int maxBounces = 5;

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
            this.CollisionSize = new Vector2(Sprite.SourceRectangle.Width, Sprite.SourceRectangle.Height);
            this.CollisionOffset = new Vector2(0, 16);
            this.HitboxManager = hitboxManager;
            this.HitboxLayer = HitboxLayer.Enemy;
            this.Hitbox = new TriggerHitbox(new Rectangle(
                (int)(dropPosition.X - CollisionSize.X / 2),
                (int)(dropPosition.Y - CollisionSize.Y / 2),
                (int)CollisionSize.X,
                (int)CollisionSize.Y))
            {
                DetectsLayers = HitboxLayer.Player
            };
            this.Hitbox.OnEnter += OnPlayerCollect;

            this.GlobalPosition = dropPosition;
            this.velocity = initialVelocity;
            this.groundY = dropPosition.Y;

            GameState.Instance.CurrentScene?.AddObject(this);
            hitboxManager.AddTrigger(Hitbox);
        }

        protected override void UpdateSelf(GameTime gameTime)
        {
            if (!isDropping)
                return;

            float dt = gameTime.DeltaSeconds();

            velocity.Y += gravity * dt;

            // Apply movement with simple collision detection
            Vector2 newPosition = GlobalPosition;
            
            // Move horizontally
            newPosition.X += velocity.X * dt;
            GlobalPosition = newPosition;
            
            // Move vertically with collision
            newPosition.Y += velocity.Y * dt;
            
            Rectangle verticalBounds = new Rectangle(
                (int)(newPosition.X - CollisionSize.X / 2),
                (int)(newPosition.Y - CollisionSize.Y / 2),
                (int)CollisionSize.X,
                (int)CollisionSize.Y);
            
            if (_hitboxManager.CheckStaticCollision(verticalBounds, HitboxLayer))
            {
                velocity.Y = -velocity.Y * bounceDamping;
                velocity.X = -velocity.X * 0.8f;
                //bounceCount++;
            }
            else
            {
                GlobalPosition = new Vector2(newPosition.X, newPosition.Y);
            }

            // Check if coin reached or passed the ground level
            if (GlobalPosition.Y >= groundY && velocity.Y > 0)
            {
                GlobalPosition = new Vector2(GlobalPosition.X, groundY);
                velocity.Y = -velocity.Y * bounceDamping;
                velocity.X *= 0.8f;
                bounceCount++;
            }

            // Stop bouncing after max bounces or if velocity is very low
            if (bounceCount >= maxBounces )
            {
                isDropping = false;
                velocity = Vector2.Zero;
                GlobalPosition = new Vector2(GlobalPosition.X, groundY);
            }

            Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, CollisionSize);
            Hitbox.Bounds = new Rectangle(
                (int)(GlobalPosition.X - CollisionSize.X / 2),
                (int)(GlobalPosition.Y - CollisionSize.Y / 2),
                (int)CollisionSize.X,
                (int)CollisionSize.Y);
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
            Console.WriteLine($"Creating coin drop at {dropPosition} with initial velocity {initialVelocity}");
            return new Coins(dropPosition, initialVelocity, hitboxManager);
        }

        private void OnPlayerCollect(TriggerHitbox hitbox, object other, TriggerSide side)
        {
            if (other is Player player)
            {
                player.Stats.Coins += 1;
                this.RemoveSelf();
            }
        }
    }
}