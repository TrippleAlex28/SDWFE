using System;
using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;

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

    public Enemy(
        int maxHealth, 
        float attackRange, 
        float damage,
        float attackCooldown,
        GameObject? target = null
    )
    {
        this.ReplicatesOverNetwork = true;

        RegisterProperty(
            nameof(_targetNetId),
            () => _targetNetId ?? -1,
            (v) => _targetNetId = v
        );

        RegisterProperty(
            nameof(MaxHealth),
            () => MaxHealth,
            (v) => MaxHealth = v
        );
        
        RegisterProperty(
            nameof(CurrentHealth),
            () => CurrentHealth,
            (v) => CurrentHealth = v
        );
        
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;

        AttackRange = attackRange;
        Damage = damage;
        AttackCooldown = attackCooldown;
        
        Target = target;

        HitboxLayer = HitboxLayer.AllExceptEnemy;
        CollisionSize = new Vector2(16, 32);
        CollisionOffset = new Vector2(8, 16);
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
        
        // Update triggers if HitboxManager is available
        if (HitboxManager != null)
        {
            HitboxManager.UpdateTriggersForObject(this, CollisionBounds, HitboxLayer);
        }
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        
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
