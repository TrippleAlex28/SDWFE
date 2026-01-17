using System;
using System.Linq;
using System.Threading.Tasks;
using Engine;
using Engine.Hitbox;
using Engine.Particle;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.Enemies;

namespace SDWFE.Objects.Projectiles;

public abstract class Projectile : GameObject
{
    // Store owner to ignore with collision
    private readonly GameObject? _owner;

    private readonly ParticleSystem _projectileTrail = new();
    private readonly ParticleEmitter? _projectileEmitter;
    private readonly ParticleSystem _collisionEffect = new();
    private readonly ParticleEmitter? _collisionEmitter;
    private readonly HitboxManager? _hitboxManager;
    protected readonly Sprite Sprite;

    protected bool Collided = false;

    private bool _removeRequested = false;
    private float _removeTimer = 0f;
    
    /// <summary>
    /// Empty constructor, should ONLY be used for network object instantiation
    /// </summary>
    public Projectile()
    {
        
    }
    
    public Projectile(
        Vector2 startPos, 
        Vector2 direction, 
        float velocity, 
        Texture2D texture, 
        GameObject? owner = null, 
        ParticleEmitter? projectileEmitter = null, 
        ParticleEmitter? collisionEmitter = null,
        HitboxManager? hitboxManager = null
    )
    {
        this.GlobalPosition = startPos;
        this.Direction = direction;
        this.Velocity = velocity;

        Sprite = new Sprite(texture);
        AddChild(Sprite);
        
        this._owner = owner;
        this._projectileEmitter = projectileEmitter;
        this._collisionEmitter = collisionEmitter;
        this._hitboxManager = hitboxManager;
    }
    
    private TriggerHitbox? _trigger;
    
    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        _trigger = new TriggerHitbox(
            new Rectangle(
                (int)this.GlobalPosition.X, 
                (int)this.GlobalPosition.Y, 
                Sprite.SourceRectangle.Width, 
                Sprite.SourceRectangle.Height
            ))
        {
            Owner = this,
            Layer = HitboxLayer.Projectile,
            DetectsLayers = HitboxLayer.Environment | HitboxLayer.Enemy | HitboxLayer.Player,
        };

        _hitboxManager?.AddTrigger(_trigger);

        _trigger.OnEnter += (hitbox, otherObject, side) =>
        {
            // Ignore self and owner
            if (otherObject == this || otherObject == _owner)
                return;
            
            if (otherObject is GameObject otherGameObject)
                OnCollision(otherGameObject);
        };
        
        if (_projectileEmitter != null)
            _projectileTrail.AddEmitter(_projectileEmitter);
        
        if (_collisionEmitter != null)
            _collisionEffect.AddEmitter(_collisionEmitter);
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        if (_removeRequested)
        {
            Sprite.IsVisible = false;
            _projectileEmitter?.Stop();
            _removeTimer -= gameTime.DeltaSeconds();
            if (_removeTimer <= 0f)
            {
                this.RemoveFromParent();
                return;
            }
        }
        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 1));
        
        if (_projectileEmitter != null)
            _projectileEmitter.Position = this.GlobalPosition - this.Displacement * gameTime.DeltaSeconds();
            
        if (_collisionEmitter != null)
            _collisionEmitter.Position = this.GlobalPosition;
        
        _projectileTrail.Update(gameTime.DeltaSeconds());
        
        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 0));
        Rectangle hitbox = new Rectangle(
            (int)GlobalPosition.X, 
            (int)GlobalPosition.Y, 
            1, 
            1
        );
        
        // Update our trigger's bounds to match projectile position
        if (_trigger != null)
        {
            _trigger.Bounds = hitbox;
        }

        // Update trigger interactions (enemy / player / environment) via HitboxManager
        if (_hitboxManager != null && _trigger != null && !Collided)
        {
            _hitboxManager.UpdateTriggersForObject(this, _trigger.Bounds, HitboxLayer.Projectile);
        }
        
        // Check static collisions (environment or static enemy bodies)
        if (_hitboxManager != null && !Collided)
        {
            var collisions = _hitboxManager.GetStaticCollisions(hitbox, HitboxLayer.Projectile, ignoreOwner: _owner ?? this);

            if (collisions.Count > 0)
            {
                // Prefer hitting an enemy owner if present; otherwise treat as environment
                var enemy = collisions
                    .Select(c => c.Owner)
                    .OfType<GameObject>()
                    .FirstOrDefault();

                if (enemy != null)
                    OnCollision(enemy);
                else
                    OnCollision(null!); // Hit environment or non-enemy static
            }
        }
        
        if (Collided)
            _collisionEffect.Update(gameTime.DeltaSeconds());
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        _projectileTrail.Draw(spriteBatch);
        
        if (Collided)
            _collisionEffect.Draw(spriteBatch);
    }

    public virtual void OnCollision(GameObject other)
    {
        Collided = true;
        Velocity = 0f;
        
        // Remove the trigger since we've collided
        if (_trigger != null && _hitboxManager != null)
        {
            _hitboxManager.RemoveTrigger(_trigger);
            _trigger = null;
        }

        if (_collisionEmitter != null)
        {
            bool isInfinite = _collisionEmitter.Config.Duration.IsApproximatelyEqual(-1f);
            if (isInfinite)
            {
                Console.WriteLine("Projectile Collision Effect shouldn't have an infinite duration!!!");
            }
            
            float maxVisible = Math.Max(_collisionEmitter.Config.Duration, _collisionEmitter.Config.LifetimeMax);

            _removeRequested = true;
            _removeTimer = isInfinite ? 0f : maxVisible;
        }
        else
        {
            _removeRequested = true;
            _removeTimer = 0f;
        }
    }
}