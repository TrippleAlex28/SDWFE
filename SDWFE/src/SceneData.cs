public static class SceneData
{
    public static int levelIndex = -1;
    public static bool hasSeenIntro = false;

    public static string levelName => LevelNames.GetLevelName(levelIndex);
}