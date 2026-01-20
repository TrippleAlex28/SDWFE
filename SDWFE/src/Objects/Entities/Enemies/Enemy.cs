using System;
using Engine;
using Engine.Hitbox;
using Engine.Particle;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Objects.Entities.Enemies;

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Dead
}

public abstract class Enemy : GameObject
{
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0f;

    public EnemyState State { get; set; } = EnemyState.Idle;

    public GameObject? Target
    {
        get => _target;
        set
        {
            _target = value;
            _targetNetId = value?.NetworkId;
        }
    }
    private GameObject? _target;
    private int? _targetNetId;

    public event Action<Enemy>? OnDeathEvent;
    
    
    public float DetectionRange { get; set; } = 200f;

    public float AttackRange { get; }
    public float Damage { get; }
    public float AttackCooldown { get; }
    protected float AttackTimer = 0f;

    private Sprite _healthBackground;
    private Sprite _healthFilling;
    private Vector2 _healthBarOffset = new Vector2(16, 0);

    // Freeze 
    private bool _isFrozen;
    private float _freezeTimer;
    private ParticleSystem _freezePS = new();
    
    public Enemy(
        int maxHealth, 
        float attackRange, 
        float damage,
        float attackCooldown,
        Vector2? healthBarOffset = null,
        GameObject? target = null
    )
    {
        this.ReplicatesOverNetwork = true;
        
        RegisterProperty(
            0,
            nameof(IsVisible),
            () => IsVisible,
            (v) => IsVisible = v
        );
        
        RegisterProperty(
            1,
            nameof(GlobalPosition),
            () => GlobalPosition,
            (v) => GlobalPosition = v
        );
        
        RegisterProperty(
            2,
            nameof(Direction),
            () => Direction,
            (v) => Direction = v
        );
        
        RegisterProperty(
            3,
            nameof(Velocity),
            () => Velocity,
            (v) => Velocity = v
        );
        
        RegisterProperty(
            4,
            nameof(_targetNetId),
            () => _targetNetId ?? -1,
            (v) => _targetNetId = v
        );

        RegisterProperty(
            5,
            nameof(MaxHealth),
            () => MaxHealth,
            (v) => MaxHealth = v
        );
        
        RegisterProperty(
            6,
            nameof(CurrentHealth),
            () => CurrentHealth,
            (v) => CurrentHealth = v
        );
        
        _healthBarOffset = healthBarOffset ?? _healthBarOffset;
        
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;

        AttackRange = attackRange;
        Damage = damage;
        AttackCooldown = attackCooldown;
        
        Target = target;

        HitboxLayer = HitboxLayer.AllExceptEnemy;
        CollisionSize = new Vector2(16, 8);
        CollisionOffset = new Vector2(16, 24);

        _healthBackground = new Sprite(EngineResources.BlankSquare);
        _healthFilling = new Sprite(EngineResources.BlankSquare);
        AddChild(_healthBackground);
        AddChild(_healthFilling);

        _freezePS.AddEmitter(ParticlePresets.CreateFreezeMist());
        _freezePS.AddEmitter(ParticlePresets.CreateFreeze());
        _freezePS.Stop();
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

        if (!IsAlive)
        {
            State = EnemyState.Dead;
            return;
        }
        
        // Update received network target
        if (_targetNetId.HasValue && (Target == null || _targetNetId != Target.NetworkId))
        {
            Target = GameState.Instance.CurrentScene?.GetObject(_targetNetId.Value);
        }
        
        // Update target based on Scene
        Target = GetBestTarget();
        
        // Update attack timer
        if (AttackTimer > 0f)
        {
            AttackTimer -= gameTime.DeltaSeconds();
        }
        
        // Separate overlapping enemies to prevent sticking
        SeparateFromOverlappingEnemies();
        
        // Update triggers if HitboxManager is available
        if (HitboxManager != null)
        {
            HitboxManager.UpdateTriggersForObject(this, CollisionBounds, HitboxLayer);
        }

        // Draw health bar above enemy
        Vector2 healthBarPos = GlobalPosition + _healthBarOffset;
        float healthPercent = (float)CurrentHealth / MaxHealth;
        
        // Health (green)
        _healthFilling.GlobalPosition = healthBarPos;
        _healthFilling.Scale = new Vector2(16 * healthPercent, 4);
        _healthFilling.Color = Color.LimeGreen;
        _healthFilling.BaseDrawLayer = 0.95f;

        // Background (red)
        _healthBackground.GlobalPosition = healthBarPos;
        _healthBackground.Scale = new Vector2(16, 4);
        _healthBackground.Color = Color.Red;
        _healthBackground.BaseDrawLayer = 0.94f;

        if (_isFrozen)
            _freezeTimer -= gameTime.DeltaSeconds();
        if (_freezeTimer <= 0f)
        {
            _isFrozen = false;
            _freezePS.Restart();
        }
        
        _freezePS.Update(gameTime.DeltaSeconds());
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        _freezePS.Draw(spriteBatch);
        
        base.DrawSelf(spriteBatch);
    }

    public void Freeze(float duration)
    {
        _isFrozen = true;
        _freezeTimer = duration;
        _freezePS.Restart();
    }
    
    /// <summary>
    /// Separate this enemy from any overlapping enemies to prevent them from getting stuck.
    /// </summary>
    private void SeparateFromOverlappingEnemies()
    {
        if (GameState.Instance.CurrentScene == null)
            return;

        Vector2 separationForce = Vector2.Zero;

        foreach (var obj in GameState.Instance.CurrentScene.SceneObjects)
        {
            // Skip self and non-enemies
            if (obj == this || obj is not Enemy otherEnemy || !otherEnemy.IsAlive)
                continue;

            // Check if overlapping
            if (CollisionBounds.Intersects(otherEnemy.CollisionBounds))
            {
                // Calculate direction away from other enemy
                Vector2 directionAway = (this.GlobalPosition - otherEnemy.GlobalPosition);
                
                if (directionAway.LengthSquared() > 0)
                {
                    directionAway.Normalize();
                    separationForce += directionAway;
                }
                else
                {
                    // If exactly on top, push in a random direction
                    separationForce += new Vector2((float)Math.Cos(this.GlobalPosition.X), (float)Math.Sin(this.GlobalPosition.Y));
                }
            }
        }

        // Apply separation force if needed
        if (separationForce.LengthSquared() > 0)
        {
            separationForce.Normalize();
            this.GlobalPosition += separationForce * 2f; // Push apart by 2 pixels
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || this is Boss boss && boss.IsImmortal) return;
        
        DamageNumber damageNumber = new DamageNumber(amount, Color.Red, 1f);

        var random = new Random();
        float randomYOffset = (float)(random.NextDouble() * 10 - 5);
        float randomXOffset = (float)(random.NextDouble() * 10 - 5);
        damageNumber.GlobalPosition = this.GlobalPosition + new Vector2(16, 16)+new Vector2(randomXOffset, randomYOffset);
        GameState.Instance.CurrentScene?.AddObject(damageNumber);
        CurrentHealth = Math.Max(0, CurrentHealth - amount);

        if (!IsAlive)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        State = EnemyState.Dead;
        Velocity = 0f;
        OnDeathEvent?.Invoke(this);
        RemoveFromParent();
    }
    
    public void Heal(int amount)
    {
        if (!IsAlive) return;

        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }

    protected bool IsTargetInRange(GameObject? target, float range)
    {
        if (target == null) return false;

        float dSquared = Vector2.DistanceSquared(GlobalPosition, target.GlobalPosition);
        return dSquared <= range * range;
    }

    protected GameObject? GetBestTarget()
    {
        GameObject? target = null;
        float bestDistance = float.MaxValue;
        foreach (var obj in GameState.Instance.CurrentScene!.SceneObjects)
        {
            // Only target Player objects
            if (obj is not SDWFE.Objects.Entities.PlayerEntity.Player player || player.State == LifeState.Dead)
                continue;
                
            if (bestDistance > GetDistanceToTarget(obj))
            {
                target = obj;
            }
        }

        return target;
    }
    
    protected float GetDistanceToTarget(GameObject? target)
    {
        return target == null ? float.MaxValue : Vector2.Distance(GlobalPosition, target.GlobalPosition);
    }

    protected virtual bool TryAttack()
    {
        if (!HasAuthority() || !(AttackTimer <= 0f) || !IsTargetInRange(Target, AttackRange)) return false;
        
        Attack();
        AttackTimer = AttackCooldown;
        return true;
    }

    protected virtual void Attack()
    {
        
    }
    
    protected override void ExitSelf()
    {
        base.ExitSelf();

        HitboxManager?.RemoveObjectFromTriggers(this);
    }
}
