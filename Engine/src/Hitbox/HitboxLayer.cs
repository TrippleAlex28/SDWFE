namespace Engine.Hitbox;

/// <summary>
/// Layers for hitbox collision filtering.
/// Use as flags to combine multiple layers.
/// </summary>
[Flags]
public enum HitboxLayer
{
    None = 0,
    Default = 1 << 0,
    Player = 1 << 1,
    Enemy = 1 << 2,
    Projectile = 1 << 3,
    Environment = 1 << 4,
    Trigger = 1 << 5,
    Item = 1 << 6,
    NPC = 1 << 7,
    
    // Common combinations
    All = ~0,
    AllExceptPlayer = All & ~Player,
    AllExceptEnemy = All & ~Enemy,
    Characters = Player | Enemy | NPC,
}
