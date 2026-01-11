using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Pathfinding;

namespace SDWFE.Objects.Entities.Enemies;

public abstract class ChasingEnemy : Enemy
{
    public float MoveSpeed { get; }

    public float PathRecalculateInterval { get; } = 0.5f;
    public float WaypointReachedDistance { get; set; } = 8f;
    public int MaxPathfindingDistance { get; set; }= 512;
    public const int PATHFINDING_GRID_SIZE = 16;

    private List<Vector2> _currentPath = new();
    private int _currentWaypointIndex = 0;
    private float _pathRecalculateTimer = 0f;
    private Pathfinder? _pathfinder;
    
    public ChasingEnemy(
        int maxHealth,
        float moveSpeed,
        float attackRange,
        float damage,
        float attackCooldown,
        GameObject? target = null
    ) : base(maxHealth, attackRange, damage, attackCooldown, target)
    {
        MoveSpeed = moveSpeed;
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();

        if (HitboxManager != null)
        {
            _pathfinder = new Pathfinder(HitboxManager, PATHFINDING_GRID_SIZE);
        }
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

        if (!IsAlive || HitboxManager == null || _pathfinder == null)
        {
            Velocity = 0f;
            return;
        }

        float deltaTime = gameTime.DeltaSeconds();
        
        // update state based on target
        if (Target != null && IsTargetInRange(Target, DetectionRange))
        {
            if (IsTargetInRange(Target, AttackRange))
            {
                State = EnemyState.Attack;
            }
            else
            {
                State = EnemyState.Chase;
            }
        }
        else
        {
            State = EnemyState.Idle;
        }
        
        // Behavior based on state
        switch (State)
        {
            case EnemyState.Idle:
                Velocity = 0f;
                _currentPath.Clear();
                break;
            case EnemyState.Chase:
                UpdateChasing(deltaTime);
                break;
            case EnemyState.Attack:
                Velocity = 0f;
                _currentPath.Clear();
                TryAttack();
                break;
            case EnemyState.Dead:
                Velocity = 0f;
                _currentPath.Clear();
                break;
        }
    }

    private void UpdateChasing(float deltaTime)
    {
        if (Target == null || _pathfinder == null) return;

        // Recalculate path periodically
        _pathRecalculateTimer -= deltaTime;
        if (_pathRecalculateTimer <= 0f)
        {
            CalculatePathToTarget();
            _pathRecalculateTimer = PathRecalculateInterval;
        }
        
        // Follow the path
        if (_currentPath.Count > 0 && _currentWaypointIndex < _currentPath.Count)
        {
            Vector2 targetWaypoint = _currentPath[_currentWaypointIndex];
            Vector2 directionToWaypoint = targetWaypoint - GlobalPosition;
            float distanceToWaypoint = directionToWaypoint.Length();

            if (distanceToWaypoint <= WaypointReachedDistance)
            {
                // Reached waypoint, move to next
                _currentWaypointIndex++;
                
                if (_currentWaypointIndex >= _currentPath.Count)
                {
                    // Reached end of path
                    Velocity = 0f;
                }
            }
            else
            {
                // Move towards waypoint
                Direction = directionToWaypoint;
                Velocity = MoveSpeed;
            }
        }
        else
        {
            // No valid path, try moving directly towards target
            Vector2 directionToTarget = Target.GlobalPosition - GlobalPosition;
            Direction = directionToTarget;
            Velocity = MoveSpeed * 0.5f; // Move slower when no path
        }
    }
    
    private void CalculatePathToTarget()
    {
        if (Target == null || _pathfinder == null) return;

        Vector2 start = GlobalPosition;
        Vector2 end = Target.GlobalPosition;

        // Check if we need to pathfind or can go directly
        if (CanMoveDirectly(start, end))
        {
            _currentPath = new List<Vector2> { end };
            _currentWaypointIndex = 0;
        }
        else
        {
            // Use A* pathfinding
            _currentPath = _pathfinder.FindPath(start, end, HitboxLayer, MaxPathfindingDistance);
            _currentWaypointIndex = 0;
        }
    }
    
    private bool CanMoveDirectly(Vector2 start, Vector2 end)
    {
        if (HitboxManager == null) return true;

        // Simple line-of-sight check
        Vector2 direction = end - start;
        float distance = direction.Length();
        
        if (distance < 1f) return true;

        direction.Normalize();
        int steps = (int)(distance / PATHFINDING_GRID_SIZE);

        for (int i = 1; i < steps; i++)
        {
            Vector2 checkPos = start + direction * (i * PATHFINDING_GRID_SIZE);
            Rectangle checkBounds = new Rectangle(
                (int)checkPos.X - (int)CollisionSize.X / 2,
                (int)checkPos.Y - (int)CollisionSize.Y / 2,
                (int)CollisionSize.X,
                (int)CollisionSize.Y
            );

            if (HitboxManager.CheckStaticCollision(checkBounds, HitboxLayer))
            {
                return false;
            }
        }

        return true;
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        // Draw path for debugging
        #if DEBUG
        if (_currentPath.Count > 0)
        {
            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                // Draw line between waypoints (requires additional drawing code)
            }
        }
        #endif
    }
}