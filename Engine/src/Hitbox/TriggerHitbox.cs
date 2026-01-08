using Microsoft.Xna.Framework;

namespace Engine.Hitbox;

/// <summary>
/// The side from which an object entered or exited a trigger.
/// </summary>
[Flags]
public enum TriggerSide
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Top = 1 << 2,
    Bottom = 1 << 3,
}

/// <summary>
/// A trigger hitbox that detects when objects enter or exit its bounds.
/// Fires events but does not block movement.
/// </summary>
public class TriggerHitbox : IHitbox
{
    private Rectangle _bounds;
    private readonly HashSet<object> _currentlyInside = new();
    private readonly Dictionary<object, Rectangle> _lastKnownBounds = new();
    
    /// <summary>
    /// Fired when an object enters the trigger. Includes entry side.
    /// </summary>
    public event Action<TriggerHitbox, object, TriggerSide>? OnEnter;
    
    /// <summary>
    /// Fired when an object exits the trigger. Includes exit side.
    /// </summary>
    public event Action<TriggerHitbox, object, TriggerSide>? OnExit;
    
    /// <summary>
    /// Fired every frame while an object stays inside the trigger.
    /// </summary>
    public event Action<TriggerHitbox, object>? OnStay;
    
    /// <summary>
    /// The bounding rectangle in world space.
    /// </summary>
    public Rectangle Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }
    
    /// <summary>
    /// The layer this trigger belongs to.
    /// </summary>
    public HitboxLayer Layer { get; set; } = HitboxLayer.Trigger;
    
    /// <summary>
    /// Layers that this trigger will detect.
    /// </summary>
    public HitboxLayer DetectsLayers { get; set; } = HitboxLayer.All;
    
    /// <summary>
    /// Whether this trigger is currently active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Optional owner object.
    /// </summary>
    public object? Owner { get; set; }
    
    /// <summary>
    /// Objects currently inside this trigger.
    /// </summary>
    public IReadOnlyCollection<object> CurrentlyInside => _currentlyInside;

    public TriggerHitbox(Rectangle bounds)
    {
        _bounds = bounds;
    }

    public TriggerHitbox(int x, int y, int width, int height)
        : this(new Rectangle(x, y, width, height))
    {
    }

    public TriggerHitbox(Vector2 position, int width, int height)
        : this(new Rectangle((int)position.X, (int)position.Y, width, height))
    {
    }

    /// <summary>
    /// Check if this hitbox intersects with a rectangle.
    /// </summary>
    public bool Intersects(Rectangle rect)
    {
        return IsEnabled && _bounds.Intersects(rect);
    }

    /// <summary>
    /// Check if this hitbox intersects with another hitbox.
    /// </summary>
    public bool Intersects(IHitbox other)
    {
        return IsEnabled && other.IsEnabled && _bounds.Intersects(other.Bounds);
    }

    /// <summary>
    /// Check if this trigger detects a specific layer.
    /// </summary>
    public bool DetectsLayer(HitboxLayer layer)
    {
        return (DetectsLayers & layer) != 0;
    }

    /// <summary>
    /// Update trigger state for an object.
    /// Call this each frame for objects that might interact with this trigger.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <param name="objBounds">The object's current bounds.</param>
    /// <param name="objLayer">The object's layer.</param>
    public void UpdateObject(object obj, Rectangle objBounds, HitboxLayer objLayer)
    {
        if (!IsEnabled) return;
        if (!DetectsLayer(objLayer)) return;

        bool isInside = _bounds.Intersects(objBounds);
        bool wasInside = _currentlyInside.Contains(obj);

        if (isInside && !wasInside)
        {
            // Object just entered
            _currentlyInside.Add(obj);
            _lastKnownBounds[obj] = objBounds;
            TriggerSide entrySide = CalculateEntrySide(objBounds);
            OnEnter?.Invoke(this, obj, entrySide);
        }
        else if (!isInside && wasInside)
        {
            // Object just exited
            _currentlyInside.Remove(obj);
            TriggerSide exitSide = CalculateExitSide(objBounds, _lastKnownBounds.GetValueOrDefault(obj, objBounds));
            _lastKnownBounds.Remove(obj);
            OnExit?.Invoke(this, obj, exitSide);
        }
        else if (isInside && wasInside)
        {
            // Object is staying inside - update last known bounds
            _lastKnownBounds[obj] = objBounds;
            OnStay?.Invoke(this, obj);
        }
    }

    /// <summary>
    /// Calculate which side an object entered from based on current position.
    /// </summary>
    private TriggerSide CalculateEntrySide(Rectangle objBounds)
    {
        TriggerSide side = TriggerSide.None;
        
        // Check horizontal entry
        float objCenterX = objBounds.X + objBounds.Width / 2f;
        float triggerCenterX = _bounds.X + _bounds.Width / 2f;
        
        if (objCenterX < triggerCenterX)
            side |= TriggerSide.Left;
        else if (objCenterX > triggerCenterX)
            side |= TriggerSide.Right;
        
        // Check vertical entry
        float objCenterY = objBounds.Y + objBounds.Height / 2f;
        float triggerCenterY = _bounds.Y + _bounds.Height / 2f;
        
        if (objCenterY < triggerCenterY)
            side |= TriggerSide.Top;
        else if (objCenterY > triggerCenterY)
            side |= TriggerSide.Bottom;
        
        return side;
    }

    /// <summary>
    /// Calculate which side an object exited from based on current and last position.
    /// </summary>
    private TriggerSide CalculateExitSide(Rectangle currentBounds, Rectangle lastBounds)
    {
        TriggerSide side = TriggerSide.None;
        
        float currentCenterX = currentBounds.X + currentBounds.Width / 2f;
        float currentCenterY = currentBounds.Y + currentBounds.Height / 2f;
        
        // Determine exit side based on where object is now relative to trigger
        if (currentBounds.Right <= _bounds.Left)
            side |= TriggerSide.Left;
        else if (currentBounds.Left >= _bounds.Right)
            side |= TriggerSide.Right;
        
        if (currentBounds.Bottom <= _bounds.Top)
            side |= TriggerSide.Top;
        else if (currentBounds.Top >= _bounds.Bottom)
            side |= TriggerSide.Bottom;
        
        return side;
    }

    /// <summary>
    /// Force remove an object from tracking (e.g., when object is destroyed).
    /// </summary>
    public void ForceRemove(object obj)
    {
        if (_currentlyInside.Remove(obj))
        {
            _lastKnownBounds.Remove(obj);
            OnExit?.Invoke(this, obj, TriggerSide.None);
        }
    }

    /// <summary>
    /// Clear all tracked objects.
    /// </summary>
    public void Clear()
    {
        foreach (var obj in _currentlyInside.ToList())
        {
            OnExit?.Invoke(this, obj, TriggerSide.None);
        }
        _currentlyInside.Clear();
        _lastKnownBounds.Clear();
    }

    /// <summary>
    /// Set position of the trigger.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        _bounds.X = (int)position.X;
        _bounds.Y = (int)position.Y;
    }

    /// <summary>
    /// Set position of the trigger.
    /// </summary>
    public void SetPosition(int x, int y)
    {
        _bounds.X = x;
        _bounds.Y = y;
    }
}
