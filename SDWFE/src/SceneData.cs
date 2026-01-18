public static class SceneData
{
    public static int levelIndex = -1;
    public static bool hasSeenIntro = false;
    public static int levelsUnlocked = 6;
    public static string levelName => LevelNames.GetLevelName(levelIndex);
}