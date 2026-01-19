namespace SDWFE.Scenes.Levels;

public class Level3 : GameplayLevel
{
    public const string KEY = "Level3";

    public Level3() : base(KEY, KEY)
    {
        
    }
    public override void Enter()
    {
        LevelIndex = 3;
        base.Enter();
    }
}