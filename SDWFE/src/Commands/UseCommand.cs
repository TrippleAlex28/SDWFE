using System.IO;
using Engine.Network.Shared.Command;
using Engine.Scene;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Commands;

public class UseCommand : NetCommand
{
    public override uint Type => (uint)NetCommands.Use;

    public string ItemName { get; set; }

    public UseCommand(string itemName)
    {
        ItemName = itemName;
    }

    public override void Apply(Scene scene, int clientId)
    {
        var obj = scene.GetPawn(clientId);
        if (obj is not Player player) return;

        ItemActionRegistry.GetUse(ItemDatabase.Instance.GetItemData(ItemName).UseActionId);
    }

    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(ItemName);
    }

    public override void Deserialize(BinaryReader br)
    {
        ItemName = br.ReadString();
    }
}