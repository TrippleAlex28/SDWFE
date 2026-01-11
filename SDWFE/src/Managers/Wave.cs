using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Tiles;

#nullable enable

namespace SDWFE.Managers;

/// <summary>
/// Represents a single wave in the game, containing enemy spawn info and associated objects.
/// </summary>
public class Wave
{
    /// <summary>
    /// The wave number (1-indexed).
    /// </summary>
    public int WaveNumber { get; set; }

    /// <summary>
    /// The total number of enemies that spawn in this wave.
    /// </summary>
    public int EnemyCount { get; set; }

    /// <summary>
    /// Positions where enemies spawn for this wave.
    /// </summary>
    public List<Vector2> EnemySpawnPositions { get; set; } = new();

    /// <summary>
    /// List of door indices that open when this wave is complete.
    /// </summary>
    public List<int> DoorIndices { get; set; } = new();

    /// <summary>
    /// Portal position if this wave has a portal (null if none).
    /// </summary>
    public Vector2? PortalPosition { get; set; }

    /// <summary>
    /// Whether this is the final wave (portal appears when complete).
    /// </summary>
    public bool IsFinalWave { get; set; }

    public Wave(int waveNumber)
    {
        WaveNumber = waveNumber;
    }

    /// <summary>
    /// Creates a default wave with just enemy count (no doors/portals).
    /// </summary>
    public static Wave CreateSimple(int waveNumber, int enemyCount, bool isFinalWave = false)
    {
        return new Wave(waveNumber)
        {
            EnemyCount = enemyCount,
            IsFinalWave = isFinalWave
        };
    }
}