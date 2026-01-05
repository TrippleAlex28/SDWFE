using System;
using System.IO;
using Engine;
using Engine.Network;
using Engine.Network.Shared.Command;
using Engine.Scene;
using Microsoft.Xna.Framework;

namespace SDWFE.Commands;

public class WalkCommand : NetCommand
{
    public override uint Type => (uint)NetCommands.Move;
    
    public Vector2 Direction { get; private set; }

    public WalkCommand() { }

    public WalkCommand(Vector2 direction)
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
        if (obj == null) return;
        
        obj.Direction = this.Direction;
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