using System;
using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
public struct NPCData
{
    public Vector2 Position;
    public int NPCType;
    
    /// <summary>
    /// Get the root node name for this NPC type (e.g., "fireman_root", "guard_root")
    /// </summary>
    public string RootNode => NPCTypes.GetRootNode(NPCType);
    public Texture2D Texture => NPCTypes.GetTexture(NPCType);
}



public static class LevelNames
{
    private static readonly Dictionary<int, string> LevelMap = new()
    {
        { -1, "Hub" },
        { 0, "Level1" },
        { 1, "Level2" },
        { 2, "Level3" },
        { 3, "Level4" },
        { 4, "Level5" },
        { 5, "Level6" },
        { 6, "BossLevel"},
        
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

/// <summary>
/// Maps NPC type IDs to their root node names and textures.
/// Add your NPC type mappings here.
/// </summary>
public static class NPCTypes
{
    private static readonly Dictionary<int, string> NPCTypeMap = new()
    {
        // Add your NPC types here:
        { 0, "anwei_root" },
        { 1, "archer_root" },
        { 2, "wizard_root"}
    };

    private static readonly Dictionary<int, Texture2D> NPCTextureMap = new()
    {
        // Add your NPC textures here:
        { 0, ExtendedGame.AssetManager.LoadTexture("32x32 Han_Soldier_Idle", "Entities/NPC/") },
        { 1, ExtendedGame.AssetManager.LoadTexture("32x32 Khitan_Raider_Idle", "Entities/NPC/") },
        { 2, ExtendedGame.AssetManager.LoadTexture("32x32 Khitan_Mage_Idle", "Entities/NPC/") }
    };
    
    public static Texture2D GetTexture(int npcType)
    {
        if (NPCTextureMap.TryGetValue(npcType, out var texture))
            return texture;
        
        return ExtendedGame.AssetManager.LoadTexture("32x32 Han_Soldier_Idle", "Entities/NPC/");
    }

    public static string GetRootNode(int npcType)
    {
        if (NPCTypeMap.TryGetValue(npcType, out var rootNode))
            return rootNode;
        
        // Fallback: return "npc_{typeId}"
        return $"fireman_root";
    }

    /// <summary>
    /// Try to get root node name. Returns false if not found.
    /// </summary>
    public static bool TryGetRootNode(int npcType, out string rootNode)
    {
        return NPCTypeMap.TryGetValue(npcType, out rootNode!);
    }
}