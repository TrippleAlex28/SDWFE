using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

public struct PortalData
{
    public Vector2 Position;
    
    // After which wave the portal appears
    public int WaveNumber;
    
    // Where the portal leads to
    public int LevelIndex;

    public string LevelName => LevelNames.GetLevelName(LevelIndex);
}
public struct EnemyData
{
    public Vector2 Position;
    public int EnemyType;
    public int WaveNumber;
}

public struct DoorData
{
    public Vector2 Position;
    public int WaveNumber;
}




public static class LevelNames
{
    private static readonly Dictionary<int, string> LevelMap = new()
    {
        { -1, "Hub" },
        { 0, "Level1" },
        { 1, "Level2" },
        { 2, "Level3" },
    };

    /// <summary>
    /// Gets the level name for the given index.
    /// Returns "level_{index}" as fallback if not found in mapping.
    /// </summary>
    public static string GetLevelName(int levelIndex)
    {
        if (LevelMap.TryGetValue(levelIndex, out var levelName))
            return levelName;
        
        // Fallback: return "level_{index}"
        return $"Hub";
    }

    /// <summary>
    /// Try to get level name. Returns false if not found.
    /// </summary>
    public static bool TryGetLevelName(int levelIndex, out string levelName)
    {
        return LevelMap.TryGetValue(levelIndex, out levelName!);
    }
}