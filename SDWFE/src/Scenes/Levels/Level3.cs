using Engine;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Scenes.Levels;

public class Level3 : GameplayLevel
{
    public const string KEY = "Level3";

    public Level3() : base(KEY, KEY)
    {
        
    }
    public override void Enter()
    {
        LevelIndex = 3;
        base.Enter();
    }
    
    protected override void OnAllWavesCompleted()
    {
        var pawn = GetPawn(GameState.Instance.SessionManager.CurrentSession?.LocalClientId ?? -1);
        if (pawn is not Player player) return;

        InventoryItem unlockedWeapon = new InventoryItem(ItemSetup.ASSAULT_RIFLE);
        if (!player.Inventory.AddItem(unlockedWeapon))
            player.Inventory.AddItemToVault(unlockedWeapon);
    }
}