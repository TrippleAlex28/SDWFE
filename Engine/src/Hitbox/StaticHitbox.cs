using Microsoft.Xna.Framework;

namespace Engine.Hitbox;

/// <summary>
/// A solid hitbox that blocks other hitboxes from passing through.
/// Supports layer-based filtering to allow certain layers to pass.
/// </summary>
public class StaticHitbox : IHitbox
{
    private Rectangle _bounds;
    
    /// <summary>
    /// The bounding rectangle in world space.
    /// </summary>
    public Rectangle Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }
    
    /// <summary>
    /// The layer this hitbox belongs to.
    /// </summary>
    public HitboxLayer Layer { get; set; } = HitboxLayer.Environment;
    
    /// <summary>
    /// Layers that this hitbox will block.
    /// Other layers will pass through.
    /// </summary>
    public HitboxLayer BlocksLayers { get; set; } = HitboxLayer.All;
    
    /// <summary>
    /// Whether this hitbox is currently active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Optional owner object.
    /// </summary>
    public object? Owner { get; set; }

    public StaticHitbox(Rectangle bounds)
    {
        _bounds = bounds;
    }

    public StaticHitbox(int x, int y, int width, int height)
        : this(new Rectangle(x, y, width, height))
    {
    }

    public StaticHitbox(Vector2 position, int width, int height)
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
    /// Check if this hitbox blocks a specific layer.
    /// </summary>
    public bool BlocksLayer(HitboxLayer layer)
    {
        return (BlocksLayers & layer) != 0;
    }

    /// <summary>
    /// Check if this hitbox would block movement from a given layer.
    /// Returns true if collision should occur.
    /// </summary>
    public bool CheckCollision(FloatRect movingRect, HitboxLayer movingLayer)
    {
        if (!IsEnabled) return false;
        if (!BlocksLayer(movingLayer)) return false;
        return new FloatRect(_bounds).Intersects(movingRect);
    }

    /// <summary>
    /// Set position of the hitbox.
    /// </summary>
    public void SetPosition(Vector2 position)
    {
        _bounds.X = (int)position.X;
        _bounds.Y = (int)position.Y;
    }

    /// <summary>
    /// Set position of the hitbox.
    /// </summary>
    public void SetPosition(int x, int y)
    {
        _bounds.X = x;
        _bounds.Y = y;
    }
}
