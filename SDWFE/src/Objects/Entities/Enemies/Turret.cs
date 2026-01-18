using System;
using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.Items;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Objects.Entities.Enemies;

public class Turret : Enemy
{
    public override uint TypeId => (uint)NetObjects.Grunt;

    public AnimatedSprite Sprite { get; private set; } // TODO: Replace with Animated Sprite
    
    public StaticHitbox? Hitbox { get; private set; }
    private bool _hitboxadded = false;
    public Turret() : base(100, 400f, 50f, 5f, new Vector2(0, 0))
    {
        
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        CollisionSize = new Vector2(16, 8);
        CollisionOffset = new Vector2(0, 24);
        Hitbox = new StaticHitbox(CollisionBounds)
        {
            Owner = this,
            Layer = HitboxLayer.Enemy,
            BlocksLayers = HitboxLayer.AllExceptPlayer
        };
        Texture2D texture = ExtendedGame.AssetManager.LoadTexture("32x16 Idle-Sheet", "Entities/Enemies/");
        Sprite = new AnimatedSprite(texture, 16, 32, _timePerFrame: 200f, isLooping: true, isPlaying: true);
        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, new Vector2(0, 16));
        if (HitboxManager != null && Hitbox != null)
        {
            HitboxManager.AddStatic(Hitbox);
            _hitboxadded = true;
        }
        
        // Sprite = new Sprite(ExtendedGame.AssetManager.LoadTexture("Grunt", "Entities/Enemies/"));
        AddChild(Sprite);
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        if (!_hitboxadded && HitboxManager != null && Hitbox != null)
        {
            HitboxManager.AddStatic(Hitbox);
            _hitboxadded = true;
        }

        HitboxLayer = HitboxLayer.Enemy;

        if (Hitbox != null)
        {
            // Keep the static hitbox aligned with the dynamic collision bounds
            Hitbox.Bounds = CollisionBounds;
        }

        if (Target != null && IsTargetInRange(Target, DetectionRange))
        {
            AttackCalculator(gameTime);
        }

        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, new Vector2(0, 32));
        // TODO: Update animation stuff here
    }
    private void AttackCalculator(GameTime gameTime)
    {
        if (!IsAlive) return;
        if (AttackTimer > 0f)
        {
            AttackTimer -= gameTime.DeltaSeconds();
        }
        else
        {
            AttackTimer = AttackCooldown;
            AnimationData attackAnimation = new AnimationData(
                ExtendedGame.AssetManager.LoadTexture("32x16 Encourage-Sheet", "Entities/Enemies/"),
                0,
                200f,
                false
            );
            Sprite.PlayOneShot(attackAnimation);
            Sprite.AnimationCompleted += onAttackAnimationCompleted;
        }
    }
    private void onAttackAnimationCompleted()
    {
        // This method is called when the attack animation completes
        Attack();
        State = EnemyState.Idle;
        Sprite.AnimationCompleted -= onAttackAnimationCompleted;
    }

    protected override void Attack()
    {
        if (Target == null) return;
        
        if (Vector2.Distance(this.GlobalPosition, Target.GlobalPosition) > AttackRange)
            return;
        
        Orb newOrb = new Orb(
            this.GlobalPosition,
            Vector2.Normalize((Target.GlobalPosition + new Vector2(8, 28)) - this.GlobalPosition),
            200f,
            500f,
            Damage,
            this,
            HitboxManager
        );
        GameState.Instance.CurrentScene?.AddObject(newOrb);
        
        // TODO: Play anim
        
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        
        // Remove collision
        if (_hitboxadded && HitboxManager != null && Hitbox != null)
        {
            HitboxManager.RemoveStatic(Hitbox);
        }
        
        // Spawn coins
        for (int i = 0; i < 5; i++)
        {
            Coins.CreateRandomDrop(GlobalPosition, HitboxManager!);
        }
        
        // Drop items
        var droppedItem = LootTables.RollLootTable(LootTables.GruntLootTable);
        if (droppedItem != null)
        {
            var pickup = new ItemPickup(droppedItem, HitboxManager)
            {
                GlobalPosition = this.GlobalPosition
            };
            GameState.Instance.CurrentScene?.AddObject(pickup);
        }
        
        // TODO: Play some effect
    }
}