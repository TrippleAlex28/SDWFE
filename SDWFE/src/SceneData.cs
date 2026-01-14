using System;
using System.Reflection.PortableExecutable;
using Engine;
using SDWFE.Objects;

public class SceneData : GameObject
{
    public override uint TypeId => (uint)NetObjects.WaveManager;
    private static int _levelIndex = -1;
    public static int LevelIndex
    {
        get => _levelIndex;
        set => _levelIndex = value;
    }

    public SceneData(){
        this.ReplicatesOverNetwork = true;

        RegisterProperty(
            nameof(LevelIndex),
            () => {
                Console.WriteLine($"[NET] Sending LevelIndex: {LevelIndex}");
                return LevelIndex;
            },
            (v) => {
                Console.WriteLine($"[NET] Received LevelIndex: {v}");
                LevelIndex = v;
            }
        );
    }
    public static bool hasSeenIntro = false;

    public static string LevelName => LevelNames.GetLevelName(LevelIndex);

    
}