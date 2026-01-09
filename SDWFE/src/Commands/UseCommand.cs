using System;
using System.IO;
using Engine;
using Engine.Network;
using Engine.Network.Shared.Command;
using Engine.Scene;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Item;

namespace SDWFE.Commands;

public class UseCommand : NetCommand
{
    public override uint Type => (uint)NetCommands.Use;

    public string ItemName { get; set; }
    public Vector2 Direction { get; set; }

    public UseCommand()
    {
        
    }
    
    public UseCommand(string itemName, Vector2 direction)
    {
        ItemName = itemName;
        
        if (direction.LengthSquared() > 0f)
        {
            Direction = direction.Normalized();
        }
        else
        {
            Direction = Vector2.Zero;
        }
    }

    public override void Apply(Scene scene, int clientId)
    {
        var obj = scene.GetPawn(clientId);
        if (obj is not Player player) return;

        var itemData = ItemDatabase.Instance.GetItemData(ItemName); 
        if (itemData.ItemType == ItemType.Weapon)
            itemData = ItemDatabase.Instance.GetWeaponData(ItemName);
        
        if (itemData.UseActionId == null) return;
        
        ItemActionRegistry.GetUse(itemData.UseActionId)(player, itemData, Direction);
    }

    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(ItemName);
        bw.Write(Direction);
    }

    public override void Deserialize(BinaryReader br)
    {
        ItemName = br.ReadString();
        Direction = br.ReadVector2();
    }
}