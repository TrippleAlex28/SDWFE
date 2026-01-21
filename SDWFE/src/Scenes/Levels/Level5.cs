using Engine;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Scenes.Levels;

public class Level5 : GameplayLevel
{
    public const string KEY = "Level5";
    private bool _initialized = false;
    public Level5() : base(KEY, KEY)
    {
        
    }
    public override void Enter()
    {
        LevelIndex = 5;
        base.Enter();
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_initialized) { return;}
        foreach (var playerobj in GetAllPawns())
        {
            if (playerobj is Player player)
            {
                player.ElevationLevel = 1;
            }
        }
        _initialized = true;
    }
    
    protected override void OnAllWavesCompleted()
    {
        var pawn = GetPawn(GameState.Instance.SessionManager.CurrentSession?.LocalClientId ?? -1);
        if (pawn is not Player player) return;

        InventoryItem unlockedWeapon = new InventoryItem(ItemSetup.FIREWORK_LAUNCHER);
        if (!player.Inventory.AddItem(unlockedWeapon))
            player.Inventory.AddItemToVault(unlockedWeapon);
    }
}