using System.Buffers.Text;
using Engine;

namespace SDWFE.Objects;

public enum LevelFailReason : byte
{
    None = 0,
    AllDead = 1,
}

public class GameplayLevelManager : GameObject
{
    public bool LevelFailed { get; set; } = false;
    public LevelFailReason FailReason { get; set; } = LevelFailReason.None;
    
    public GameplayLevelManager() : base()
    {
        this.ReplicatesOverNetwork = true;

        RegisterProperty(
            0,
            nameof(LevelFailed),
            () => LevelFailed,
            (v) => LevelFailed = v
        );
        
        RegisterProperty(
            1,
            nameof(FailReason),
            () => (byte)FailReason,
            (v) => FailReason = (LevelFailReason)v
        );
    }
}