using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Hitbox;

/// <summary>
/// Manages all hitboxes in a scene and handles collision detection.
/// </summary>
public class HitboxManager
{
    private readonly List<StaticHitbox> _staticHitboxes = new();
    private readonly List<TriggerHitbox> _triggerHitboxes = new();
    
    private bool _debugEnabled = false;
    private bool _f3PreviouslyPressed = false;

    /// <summary>
    /// All registered static hitboxes.
    /// </summary>
    public IReadOnlyList<StaticHitbox> StaticHitboxes => _staticHitboxes;
    
    /// <summary>
    /// All registered trigger hitboxes.
    /// </summary>
    public IReadOnlyList<TriggerHitbox> TriggerHitboxes => _triggerHitboxes;
    
    /// <summary>
    /// Whether debug rendering is enabled. Toggle with F3.
    /// </summary>
    public bool DebugEnabled
    {
        get => _debugEnabled;
        set => _debugEnabled = value;
    }

    #region Registration

    /// <summary>
    /// Register a static hitbox.
    /// </summary>
    public void AddStatic(StaticHitbox hitbox)
    {
        _staticHitboxes.Add(hitbox);
    }

    /// <summary>
    /// Create and register a static hitbox.
    /// </summary>
    public StaticHitbox AddStatic(Rectangle bounds, HitboxLayer layer = HitboxLayer.Environment, HitboxLayer blocksLayers = HitboxLayer.All)
    {
        var hitbox = new StaticHitbox(bounds)
        {
            Layer = layer,
            BlocksLayers = blocksLayers
        };
        _staticHitboxes.Add(hitbox);
        return hitbox;
    }

    /// <summary>
    /// Remove a static hitbox.
    /// </summary>
    public bool RemoveStatic(StaticHitbox hitbox)
    {
        return _staticHitboxes.Remove(hitbox);
    }

    /// <summary>
    /// Register a trigger hitbox.
    /// </summary>
    public void AddTrigger(TriggerHitbox hitbox)
    {
        _triggerHitboxes.Add(hitbox);
    }

    /// <summary>
    /// Create and register a trigger hitbox.
    /// </summary>
    public TriggerHitbox AddTrigger(Rectangle bounds, HitboxLayer detectsLayers = HitboxLayer.All)
    {
        var hitbox = new TriggerHitbox(bounds)
        {
            DetectsLayers = detectsLayers
        };
        _triggerHitboxes.Add(hitbox);
        return hitbox;
    }

    /// <summary>
    /// Remove a trigger hitbox.
    /// </summary>
    public bool RemoveTrigger(TriggerHitbox hitbox)
    {
        hitbox.Clear();
        return _triggerHitboxes.Remove(hitbox);
    }

    /// <summary>
    /// Clear all hitboxes.
    /// </summary>
    public void Clear()
    {
        _staticHitboxes.Clear();
        foreach (var trigger in _triggerHitboxes)
            trigger.Clear();
        _triggerHitboxes.Clear();
    }

    #endregion

    #region Collision Queries

    /// <summary>
    /// Check if a rectangle collides with any static hitbox.
    /// </summary>
    public bool CheckStaticCollision(Rectangle rect, HitboxLayer layer, object? ignoreOwner = null)
    {
        foreach (var hitbox in _staticHitboxes)
        {
            if (ignoreOwner != null && ReferenceEquals(hitbox.Owner, ignoreOwner))
                continue;

            if (hitbox.CheckCollision(rect, layer))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get all static hitboxes that a rectangle collides with.
    /// </summary>
    public List<StaticHitbox> GetStaticCollisions(Rectangle rect, HitboxLayer layer, object? ignoreOwner = null)
    {
        var result = new List<StaticHitbox>();
        foreach (var hitbox in _staticHitboxes)
        {
            if (ignoreOwner != null && ReferenceEquals(hitbox.Owner, ignoreOwner))
                continue;

            if (hitbox.CheckCollision(rect, layer))
                result.Add(hitbox);
        }
        return result;
    }

    /// <summary>
    /// Try to move a rectangle, stopping at collision.
    /// Returns the valid position after collision resolution.
    /// </summary>
    public Vector2 MoveAndSlide(Rectangle currentBounds, Vector2 velocity, HitboxLayer layer, out bool hitX, out bool hitY, object? ignoreOwner = null)
    {
        hitX = false;
        hitY = false;

        Vector2 newPos = new Vector2(currentBounds.X, currentBounds.Y);

        // Try X movement
        if (velocity.X != 0)
        {
            Rectangle testRect = new Rectangle(
                (int)(currentBounds.X + velocity.X),
                currentBounds.Y,
                currentBounds.Width,
                currentBounds.Height
            );

            if (!CheckStaticCollision(testRect, layer, ignoreOwner))
            {
                newPos.X += (int)velocity.X;
            }
            else
            {
                hitX = true;
                // Slide along collision
                newPos.X = ResolveCollisionX(currentBounds, velocity.X, layer, ignoreOwner);
            }
        }

        // Try Y movement
        if (velocity.Y != 0)
        {
            Rectangle testRect = new Rectangle(
                (int)newPos.X,
                (int)MathF.Ceiling(currentBounds.Y + velocity.Y),
                currentBounds.Width,
                currentBounds.Height
            );

            if (!CheckStaticCollision(testRect, layer, ignoreOwner))
            {
                newPos.Y += (int)velocity.Y;
            }
            else
            {
                hitY = true;
                // Slide along collision
                newPos.Y = ResolveCollisionY(new Rectangle((int)newPos.X, currentBounds.Y, currentBounds.Width, currentBounds.Height), velocity.Y, layer, ignoreOwner);
            }
        }

        return newPos;
    }

    private float ResolveCollisionX(Rectangle bounds, float velocityX, HitboxLayer layer, object? ignoreOwner)
    {
        int step = velocityX > 0 ? 1 : -1;
        float newX = bounds.X;

        for (int i = 0; i < Math.Abs(velocityX); i++)
        {
            Rectangle test = new Rectangle((int)newX + step, bounds.Y, bounds.Width, bounds.Height);
            if (CheckStaticCollision(test, layer, ignoreOwner))
                break;
            newX += step;
        }

        return newX;
    }

    private float ResolveCollisionY(Rectangle bounds, float velocityY, HitboxLayer layer, object? ignoreOwner)
    {
        int step = velocityY > 0 ? 1 : -1;
        float newY = bounds.Y;

        for (int i = 0; i < Math.Abs(velocityY); i++)
        {
            Rectangle test = new Rectangle(bounds.X, (int)newY + step, bounds.Width, bounds.Height);
            if (CheckStaticCollision(test, layer, ignoreOwner))
                break;
            newY += step;
        }

        return newY;
    }

    #endregion

    #region Trigger Updates

    /// <summary>
    /// Update all triggers for a specific object.
    /// Call this each frame for objects that interact with triggers.
    /// </summary>
    public void UpdateTriggersForObject(object obj, Rectangle objBounds, HitboxLayer objLayer)
    {
        // Use ToList() to iterate over a copy, preventing "collection modified" errors
        // if triggers are removed during update (e.g., in event handlers)
        foreach (var trigger in _triggerHitboxes.ToList())
        {
            trigger.UpdateObject(obj, objBounds, objLayer);
        }
    }

    /// <summary>
    /// Force remove an object from all triggers.
    /// Call when an object is destroyed.
    /// </summary>
    public void RemoveObjectFromTriggers(object obj)
    {
        foreach (var trigger in _triggerHitboxes)
        {
            trigger.ForceRemove(obj);
        }
    }

    #endregion

    #region Debug Rendering

    /// <summary>
    /// Update debug state (handles F3 key toggle).
    /// Call this in your Update method.
    /// </summary>
    public void UpdateDebug()
    {
        var keyboardState = Keyboard.GetState();
        bool f3Pressed = keyboardState.IsKeyDown(Keys.F3);
        
        if (f3Pressed && !_f3PreviouslyPressed)
        {
            _debugEnabled = !_debugEnabled;
        }
        
        _f3PreviouslyPressed = f3Pressed;
    }

    /// <summary>
    /// Draw debug visualization of all hitboxes.
    /// Call this in your Draw method when debug is enabled.
    /// Static hitboxes are drawn in red, trigger hitboxes in green.
    /// </summary>
    public void DrawDebug(SpriteBatch spriteBatch)
    {
        if (!_debugEnabled) return;

        // Draw static hitboxes in red with semi-transparency
        foreach (var hitbox in _staticHitboxes)
        {
            if (!hitbox.IsEnabled) continue;
            
            Color color = new Color(255, 0, 0, 128); // Red, semi-transparent
            spriteBatch.Draw(
                EngineResources.BlankSquare,
                hitbox.Bounds,
                null,
                color,
                0f, Vector2.Zero, SpriteEffects.None, 0.999f
            );
        }

        // Draw trigger hitboxes in green with semi-transparency
        foreach (var trigger in _triggerHitboxes)
        {
            if (!trigger.IsEnabled) continue;
            
            Color color = new Color(0, 255, 0, 128); // Green, semi-transparent
            spriteBatch.Draw(
                EngineResources.BlankSquare,
                trigger.Bounds,
                null,
                color,
                0f, Vector2.Zero, SpriteEffects.None, 0.999f
            );
        }
    }

    #endregion
}
