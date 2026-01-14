using System.Reflection.PortableExecutable;

public static class SceneData
{
    public static int LevelIndex = -1;
    public static bool hasSeenIntro = false;

    public static string LevelName => LevelNames.GetLevelName(LevelIndex);
    
}