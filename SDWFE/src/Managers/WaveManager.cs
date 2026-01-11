using System;
using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Tilemap;
using SDWFE.Objects.Tiles;

#nullable enable

namespace SDWFE.Managers;

/// <summary>
/// Manages wave progression, enemy spawning, and door/portal activation.
/// </summary>
public class WaveManager : GameObject
{
    private readonly List<Wave> _waves;
    private readonly Dictionary<int, RoomDoor> _doorsById;
    private readonly HitboxManager _hitboxManager;
    
    private int _currentWaveIndex = -1;
    private int _enemiesRemaining = 0;
    private Portal? _activePortal;
    private bool _waveInProgress = false;

    /// <summary>
    /// The current wave number (1-indexed). Returns 0 if no wave started.
    /// </summary>
    public int CurrentWaveNumber => _currentWaveIndex >= 0 ? _waves[_currentWaveIndex].WaveNumber : 0;

    /// <summary>
    /// Number of enemies still alive in the current wave.
    /// </summary>
    public int EnemiesRemaining => _enemiesRemaining;

    /// <summary>
    /// Total number of waves.
    /// </summary>
    public int TotalWaves => _waves.Count;

    /// <summary>
    /// Whether all waves have been completed.
    /// </summary>
    public bool AllWavesComplete => _currentWaveIndex >= _waves.Count - 1 && _enemiesRemaining <= 0 && _waveInProgress;

    /// <summary>
    /// Whether a wave is currently in progress.
    /// </summary>
    public bool WaveInProgress => _waveInProgress;

    /// <summary>
    /// Event fired when a wave starts.
    /// </summary>
    public event Action<int>? OnWaveStarted;

    /// <summary>
    /// Event fired when a wave is completed (all enemies killed).
    /// </summary>
    public event Action<int>? OnWaveCompleted;

    /// <summary>
    /// Event fired when all waves are completed.
    /// </summary>
    public event Action? OnAllWavesCompleted;

    public WaveManager(List<Wave> waves, Dictionary<int, RoomDoor> doorsById, HitboxManager hitboxManager)
    {
        _waves = waves;
        _doorsById = doorsById;
        _hitboxManager = hitboxManager;
    }

    /// <summary>
    /// Starts the first wave.
    /// </summary>
    public void StartWaves()
    {
        if (_waves.Count == 0)
        {
            Console.WriteLine("[WaveManager] No waves to start.");
            return;
        }

        _currentWaveIndex = -1;
        StartNextWave();
    }

    /// <summary>
    /// Advances to the next wave if current wave is complete.
    /// </summary>
    public void StartNextWave()
    {
        if (_currentWaveIndex >= _waves.Count - 1)
        {
            Console.WriteLine("[WaveManager] All waves completed!");
            OnAllWavesCompleted?.Invoke();
            return;
        }

        _currentWaveIndex++;
        var wave = _waves[_currentWaveIndex];
        _enemiesRemaining = wave.EnemyCount;
        _waveInProgress = true;

        Console.WriteLine($"[WaveManager] Starting Wave {wave.WaveNumber} with {wave.EnemyCount} enemies.");
        OnWaveStarted?.Invoke(wave.WaveNumber);

        // TODO: Spawn actual enemies at wave.EnemySpawnPositions
        // For now, enemies are simulated via OnEnemyKilled()

        // If wave has 0 enemies, complete immediately
        if (_enemiesRemaining <= 0)
        {
            CompleteCurrentWave();
        }
    }

    /// <summary>
    /// Called when an enemy is killed. Decrements enemy count and checks for wave completion.
    /// </summary>
    public void OnEnemyKilled()
    {
        if (!_waveInProgress || _enemiesRemaining <= 0)
            return;

        _enemiesRemaining--;
        Console.WriteLine($"[WaveManager] Enemy killed. {_enemiesRemaining} remaining.");

        if (_enemiesRemaining <= 0)
        {
            CompleteCurrentWave();
        }
    }

    /// <summary>
    /// Simulates killing an enemy (for testing without real enemies).
    /// </summary>
    public void SimulateEnemyKill()
    {
        OnEnemyKilled();
    }

    private void CompleteCurrentWave()
    {
        if (_currentWaveIndex < 0 || _currentWaveIndex >= _waves.Count)
            return;

        var wave = _waves[_currentWaveIndex];
        _waveInProgress = false;

        Console.WriteLine($"[WaveManager] Wave {wave.WaveNumber} completed!");
        OnWaveCompleted?.Invoke(wave.WaveNumber);

        // Open doors associated with this wave
        foreach (var doorIndex in wave.DoorIndices)
        {
            if (_doorsById.TryGetValue(doorIndex, out var door))
            {
                door.Open();
                Console.WriteLine($"[WaveManager] Opened door {doorIndex}.");
            }
        }

        // If final wave, spawn portal
        if (wave.IsFinalWave && wave.PortalPosition.HasValue)
        {
            SpawnPortal(wave.PortalPosition.Value);
            OnAllWavesCompleted?.Invoke();
        }
        else if (_currentWaveIndex < _waves.Count - 1)
        {
            // Auto-start next wave (or you could require manual trigger)
            // For now we wait for player to walk through door
        }
    }

    private void SpawnPortal(Vector2 position)
    {
        Console.WriteLine($"[WaveManager] Spawning portal at {position}");
        _activePortal = new Portal(position, _hitboxManager);
        AddChild(_activePortal);
    }

    /// <summary>
    /// Creates a WaveManager with default test waves.
    /// </summary>
    public static WaveManager CreateTestWaves(Dictionary<int, RoomDoor> doorsById, HitboxManager hitboxManager, Vector2? portalPosition = null)
    {
        var waves = new List<Wave>
        {
            new Wave(1) { EnemyCount = 3, DoorIndices = new List<int> { 0 } },
            new Wave(2) { EnemyCount = 5, DoorIndices = new List<int> { 1 } },
            new Wave(3) 
            { 
                EnemyCount = 7, 
                IsFinalWave = true, 
                PortalPosition = portalPosition ?? new Vector2(200, 200) 
            }
        };

        return new WaveManager(waves, doorsById, hitboxManager);
    }
}