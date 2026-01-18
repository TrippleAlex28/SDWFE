using System;
using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.Items;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

#nullable enable

namespace SDWFE.Objects.Entities.Enemies;

public class Grunt : ChasingEnemy
{
    public override uint TypeId => (uint)NetObjects.Grunt;

    public AnimatedSprite Sprite { get; private set; } // TODO: Replace with Animated Sprite
    
    public StaticHitbox? Hitbox { get; private set; }
    private bool _hitboxadded = false;
    public Grunt() : base(100, 100f, 32f, 10f, 1.0f)
    {
        
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        Hitbox = new StaticHitbox(CollisionBounds)
        {
            Owner = this,
            Layer = HitboxLayer.Enemy,
            BlocksLayers = HitboxLayer.AllExceptPlayer
        };
        Texture2D texture = ExtendedGame.AssetManager.LoadTexture("32x32 Han_Soldier_Idle", "Entities/NPC/");
        Sprite = new AnimatedSprite(texture, 32, 32, _timePerFrame: 200f, isLooping: true, isPlaying: true);
        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, new Vector2(0, 32));
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
        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, new Vector2(0, 32));
        // TODO: Update animation stuff here
    }

    protected override void Attack()
    {
        Console.WriteLine("Grunt Attack");
        if (Target == null) return;
        
        if (Vector2.Distance(this.GlobalPosition, Target.GlobalPosition) > AttackRange)
            return;
        
        if (Target is Player player)
        {
            player.Stats.CurrentHealth -= Damage;

        }
        // TODO: Play animation or something
        
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        
        // Remove collisions
        if (_hitboxadded && HitboxManager != null && Hitbox != null)
        {
            HitboxManager.RemoveStatic(Hitbox);
        }
        
        // Drop coins
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
