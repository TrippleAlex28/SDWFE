using Microsoft.Xna.Framework;

namespace Engine.Hitbox;

/// <summary>
/// Common interface for all hitbox types.
/// </summary>
public interface IHitbox
{
    /// <summary>
    /// The bounding rectangle of this hitbox in world space.
    /// </summary>
    Rectangle Bounds { get; }
    
    /// <summary>
    /// The layer this hitbox belongs to.
    /// </summary>
    HitboxLayer Layer { get; set; }
    
    /// <summary>
    /// Whether this hitbox is currently active.
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Optional owner object for identification.
    /// </summary>
    object? Owner { get; set; }
    
    /// <summary>
    /// Check if this hitbox intersects with a rectangle.
    /// </summary>
    bool Intersects(Rectangle rect);
    
    /// <summary>
    /// Check if this hitbox intersects with another hitbox.
    /// </summary>
    bool Intersects(IHitbox other);
}
