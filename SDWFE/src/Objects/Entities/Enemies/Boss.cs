
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.Items;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

public class Boss : Enemy
{
    public override uint TypeId => (uint)NetObjects.Boss;

    public AnimatedSprite Sprite { get; private set; } // TODO: Replace with Animated Sprite
    
    public StaticHitbox? Hitbox { get; private set; }

    public bool IsImmortal = false;
    private bool _hitboxadded = false;

    protected BossStage currentStage { get; set; } = BossStage.Stage3;
    protected AttackPattern currentAttackPattern { get; set; } = AttackPattern.PatternC;

    private int currentAttackPatternIndex = 0;
    private float attackPatternTimer = 20f;
    private bool isAttacking = false;

    private float attackSpeed;
    private float standardAttackTimer = 5f;

    private List<Vector2> SpawnPoints = new List<Vector2>()
    {
        new Vector2(-100, -100),
        new Vector2(100, -100),
        new Vector2(-100, 100),
        new Vector2(100, 100)
    };

    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    public Boss() : base(2000, 200f, 75f, 5f, new Vector2(12, -6))
    {
        
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        Hitbox = new StaticHitbox(CollisionBounds)
        {
            Owner = this,
            Layer = HitboxLayer.Boss,
            BlocksLayers = HitboxLayer.AllExceptPlayer
        };
        Texture2D texture = ExtendedGame.AssetManager.LoadTexture("32x32 Idle-Sheet", "Entities/Enemies/Boss/");
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
        if (Hitbox != null)
        {
            // Keep the static hitbox aligned with the dynamic collision bounds
            Hitbox.Bounds = CollisionBounds;
        }
        HitboxLayer = HitboxLayer.Boss;

        IsImmortal = _spawnedEnemies.Count > 0;
        // figure out the stage of the boss based on health
        float healthPercentage = (float)CurrentHealth / MaxHealth;
        if (healthPercentage > 0.66f)
        {
            currentStage = BossStage.Stage1;
        }
        else if (healthPercentage > 0.33f)
        {
            currentStage = BossStage.Stage2;
        }
        else
        {
            currentStage = BossStage.Stage3;
        }
        ChooseAttack(gameTime);
        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, new Vector2(0, 32));
        // TODO: Update animation stuff here
        if (isAttacking){
            attackPatternTimer -= gameTime.DeltaSeconds();
            UpdateAttacks();
        } 
    }
    private void ChooseAttack(GameTime gameTime)
    {
        if (!IsAlive) return;
        if (AttackTimer > 0f)
        {
            AttackTimer -= gameTime.DeltaSeconds();
            return;
        }
        switch (currentStage)
        {
            case BossStage.Stage1:
                AttackTimer = AttackCooldown;
                AttackTypeA();
                break;
            case BossStage.Stage2:
                AttackTimer = AttackCooldown * 0.8f;
                int randomChoice = ExtendedGame.Random.Next(0, 2);
                if (randomChoice == 0)
                    AttackTypeA();
                else
                    AttackTypeC();
                break;
            case BossStage.Stage3:
                AttackTimer = AttackCooldown * 0.6f;
                int randomChoiceStage3 = ExtendedGame.Random.Next(0, 3);
                if (randomChoiceStage3 == 0)
                    AttackTypeB();
                else if (randomChoiceStage3 == 1){
                    AttackTimer = AttackCooldown * 0.2f;
                    AttackTypeC();
                } 
                else if (_spawnedEnemies.Count < 2)
                    AttackTypeD();
                break; 
            default:
                AttackTypeA();
                break;
        }
    }
    protected override void Attack()
    {
        if (Target == null) return;
        
        if (Vector2.Distance(this.GlobalPosition, Target.GlobalPosition) > AttackRange)
            return;

        switch (currentStage)
        {
            case BossStage.Stage1:
                AttackTypeA();
                break;
            case BossStage.Stage2:
                AttackTypeB();
                break;
            case BossStage.Stage3:
                AttackTypeD();
                break; 
            default:
                AttackTypeD();
                break;
        }
        // Check if target is within range
        // TODO: Play anim
        
    }

    private void AttackTypeA()
    {
        if (Target is Player player)
        {
            Vector2 startPos = this.GlobalPosition + new Vector2(24, 0);
            Orb newOrb = new Orb(
            startPos,
            Vector2.Normalize(Target.GlobalPosition + new Vector2(8, 28) - startPos),
            300f,
            500f,
            Damage,
            this,
            HitboxManager
            );
            GameState.Instance.CurrentScene?.AddObject(newOrb);
        }
        
    }
    private void UpdateAttacks()
    {
        switch (currentAttackPattern)
        {
            case AttackPattern.PatternA:
                isAttacking = false;
                currentAttackPatternIndex = 0;
                attackPatternTimer = 0.2f;
                break;
            case AttackPattern.PatternB:
                UpdateAttackBPattern();
                break;
            case AttackPattern.PatternC:
                AttackTypeC();
                isAttacking = false;
                currentAttackPatternIndex = 0;
                attackPatternTimer = 0.2f;
                break; 
            default:
                AttackTypeD();
                isAttacking = false;
                currentAttackPatternIndex = 0;
                attackPatternTimer = 0.2f;
                break;
        }
        
    }
    private void UpdateAttackBPattern()
    {
        if (currentAttackPatternIndex >= 16){
            isAttacking = false;
            currentAttackPatternIndex = 0;
            attackPatternTimer = 0.2f;
            return;
        }
        if (attackPatternTimer <= 0f){
            float angle = currentAttackPatternIndex * (MathF.PI / 8); // 45 degrees apart
            Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 startPos = this.GlobalPosition + new Vector2(24, 0);
            Orb newOrb = new Orb(
            startPos,
            direction,
            200f,
            500f,
            Damage,
            this,
            HitboxManager
            );
            GameState.Instance.CurrentScene?.AddObject(newOrb);
            currentAttackPatternIndex += 1;
            attackPatternTimer = 0.2f;
        }
    }
    private void AttackTypeB()
    {
        if (Target is Player player)
        {
            isAttacking = true;
            attackPatternTimer = 0f;
            currentAttackPatternIndex = 0;
            currentAttackPattern = AttackPattern.PatternB;
        }
    }
    private void AttackTypeC()
    {
        if (Target is Player player)
        {
            float randomOffset = ExtendedGame.Random.NextSingle() * MathF.PI;

            for (int i = 0; i < 8; i++)
            {
                float angle = i * (MathF.PI / 4) + randomOffset;
                Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 startPos = this.GlobalPosition + new Vector2(24, 0);
                Orb newOrb = new Orb(
                startPos,
                direction,
                400f,
                500f,
                Damage,
                this,
                HitboxManager
                );
                GameState.Instance.CurrentScene?.AddObject(newOrb);
            }
        }
    }
    private void AttackTypeD()
    {
        if (Target is Player player)
        {
            AttackTimer = AttackCooldown * 2f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPos = this.GlobalPosition + SpawnPoints[i];
                Grunt grunt = new Grunt();
                grunt.GlobalPosition = spawnPos;
                grunt.HitboxManager = HitboxManager;
                grunt.OnDeathEvent += deleteSpawnedEnemy;

                GameState.Instance.CurrentScene?.AddObject(grunt);
                _spawnedEnemies.Add(grunt);
            }
        }
    }
    private void deleteSpawnedEnemy(Enemy enemy)
    {
        enemy.RemoveSelf();
        _spawnedEnemies.Remove(enemy);
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
public enum AttackPattern
{
    PatternA,
    PatternB,
    PatternC,
    PatternD
}
public enum BossStage
{
    Stage1,
    Stage2,
    Stage3
}