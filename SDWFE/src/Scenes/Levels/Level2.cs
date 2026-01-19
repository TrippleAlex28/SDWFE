namespace SDWFE.Scenes.Levels;

public class Level2 : GameplayLevel
{
    public const string KEY = "Level2";

    public Level2() : base(KEY, KEY)
    {
        
    }
    public override void Enter()
    {
        LevelIndex = 2;
        base.Enter();
    }
}