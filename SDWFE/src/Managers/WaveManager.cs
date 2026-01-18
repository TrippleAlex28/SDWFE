using System;
using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Engine.Scene;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Tilemap;
using SDWFE.Objects.Tiles;

#nullable enable

namespace SDWFE.Managers;

/// <summary>
/// Manages wave progression, enemy spawning, and door/portal activation.
/// </summary>
public class WaveManager : GameObject
{
    public Boss bossEntity;
    private readonly List<Wave> _waves;
    private List<RoomDoor> _doors = new();
    private List<Portal> _portals = new();
    private List<Enemy> _enemies = new();
    
    private HitboxManager _hitboxManager;
    private int _currentWaveIndex = -1;
    private int _enemiesRemaining = 0;
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

    private Scene _scene;
    
    public WaveManager(Scene scene, List<PortalData> portals, List<DoorData> doors, List<EnemyData> enemies, HitboxManager hitboxManager)
    {
        _scene = scene;
        
        _hitboxManager = hitboxManager;
        _waves = InitializeWaves(portals, doors, enemies);
    }
    private List<Wave> InitializeWaves(List<PortalData> portals, List<DoorData> doors, List<EnemyData> enemies){
        List<Wave> waves = new List<Wave>();
        int totalWaves = 0;
        portals.ForEach(portalData =>
        {
            if (portalData.WaveNumber > totalWaves)
                totalWaves = portalData.WaveNumber;
        });

        for (int i = 0; i < totalWaves; i++)
        {
            Wave newWave = new Wave(i + 1);
            // Add enemies for this wave
            enemies.ForEach(enemyData =>
            {
                if (enemyData.WaveNumber == i + 1)
                {
                    newWave.EnemyCount++;
                    newWave.EnemyData.Add(enemyData);
                }
            });

            // Add doors for this wave
            doors.ForEach(doorData =>
            {
                if (doorData.WaveNumber == i + 1)
                {
                    newWave.DoorsToOpen.Add(doorData);
                }
            });

            // Add portals for this wave
            portals.ForEach(portalData =>
            {
                if (portalData.WaveNumber == i + 1)
                {
                    newWave.PortalData.Add(portalData);
                }
            });
            waves.Add(newWave);
        }
        
        
        return waves;
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
    public void BuildWave()
    {
        Wave wave = _waves[_currentWaveIndex];
        foreach (var doorData in wave.DoorsToOpen)
        {
            var door = new RoomDoor(doorData.Position, _hitboxManager);
            _doors.Add(door);
            AddChild(door);
        }
        foreach (var portalData in wave.PortalData)
        {
            var portal = new Portal(portalData, _hitboxManager);
            portal.IsVisible = false;
            _portals.Add(portal);
            AddChild(portal);
        }
        foreach (var enemyData in wave.EnemyData)
        {
            Enemy enemy;
            switch (enemyData.EnemyType)
            {
                case 0:
                    enemy = new Grunt();
                    break;
                case 1:
                    enemy = new Turret();
                    break;
                case 2:
                    enemy = new Boss();
                    bossEntity = (Boss)enemy;
                    break;
                default:
                    enemy = new Turret(); // Default to Grunt if unknown type
                    break;
            }
            enemy.OnDeathEvent += (Enemy e) => {
                OnEnemyKilled(e);
            };
            enemy.GlobalPosition = enemyData.Position;
            enemy.HitboxManager = _hitboxManager;
            _enemies.Add(enemy);
            
            _scene.AddObject(enemy);
        }
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

        _enemiesRemaining = wave.EnemyData.Count;
        _waveInProgress = true;

        Console.WriteLine($"[WaveManager] Starting Wave {wave.WaveNumber} with {wave.EnemyCount} enemies.");
        OnWaveStarted?.Invoke(wave.WaveNumber);

        // TODO: Spawn actual enemies at wave.EnemySpawnPositions
        // For now, enemies are simulated via OnEnemyKilled()
        BuildWave();

        // If wave has 0 enemies, complete immediately
        if (_enemiesRemaining <= 0)
        {
            CompleteCurrentWave();
        }
    }

    /// <summary>
    /// Called when an enemy is killed. Decrements enemy count and checks for wave completion.
    /// </summary>
    public void OnEnemyKilled(Enemy e)
    {
        if (!_waveInProgress || _enemiesRemaining <= 0)
            return;

        e.IsVisible = false;
        
        _enemiesRemaining--;
        Console.WriteLine($"[WaveManager] Enemy killed. {_enemiesRemaining} remaining.");

        if (_enemiesRemaining <= 0)
        {
            CompleteCurrentWave();
        }
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
        foreach (RoomDoor door in _doors)
        {
            door.Open();
            Console.WriteLine($"[WaveManager] Opened door at {door.GlobalPosition}.");
        }
        // Open portals associated with this wave
        foreach (Portal portal in _portals)
        {
            portal.IsVisible = true;
            SceneData.levelsUnlocked = Math.Max(SceneData.levelsUnlocked, GameState.Instance.CurrentScene?.LevelIndex + 1 ?? 0);
            Console.WriteLine($"[WaveManager] Activated portal at {portal.GlobalPosition}.");
        }

        if (_currentWaveIndex < _waves.Count - 1)
        {
            StartNextWave();
        }
    }
}
