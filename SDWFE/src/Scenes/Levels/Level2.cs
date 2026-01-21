using Engine;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Scenes.Levels;

public class Level2 : GameplayLevel
{
    public const string KEY = "Level2";

    public Level2() : base(KEY, KEY)
    {
        
    }
    public override void Enter()
    {
        LevelIndex = 2;
        base.Enter();
    }
    
    protected override void OnAllWavesCompleted()
    {
        var pawn = GetPawn(GameState.Instance.SessionManager.CurrentSession?.LocalClientId ?? -1);
        if (pawn is not Player player) return;

        InventoryItem unlockedWeapon = new InventoryItem(ItemSetup.SHOTGUN);
        if (!player.Inventory.AddItem(unlockedWeapon))
            player.Inventory.AddItemToVault(unlockedWeapon);
    }
}