using System;
using System.IO;
using Engine;
using Engine.Network;
using Engine.Network.Shared.Command;
using Engine.Scene;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Commands;

public class LeapCommand : NetCommand
{
    public override uint Type => (uint)NetCommands.Leap;
    
    public Vector2 Direction { get; private set; }

    public LeapCommand() { }

    public LeapCommand(Vector2 direction)
    {
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
        
        this.Direction.Normalize();
        
        player.Leap(this.Direction);
    }

    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(Direction);
    }

    public override void Deserialize(BinaryReader br)
    {
        Direction = br.ReadVector2();
    }
}