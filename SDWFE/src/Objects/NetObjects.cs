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
    Grunt = 5,
    Turret = 6,
    WaveManager = 7,
} 