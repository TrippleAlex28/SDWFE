namespace SDWFE.Objects;

/// <summary>
/// Don't use 0, Reserved for GameObject, although I don't think it'll ever be used
/// </summary>
public enum NetObjects : uint
{
    Player = 1,
    GenericBullet = 2,
    ShotgunBullet = 3,
    FireworkRocket = 4,
    Arrow = 5,
    Grunt = 6,
    Turret = 7,
    Orb = 8,
    Boss = 9,
    GameplayLevelManager = 10,
} 