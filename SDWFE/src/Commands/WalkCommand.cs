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
        
        // This is for the stairs logic, if you don't like it, change it back
        if (obj.IsOnStairs && obj.StairDirection.LengthSquared() > 0f && this.Direction.LengthSquared() > 0f)
        {
            // Dot product to determine up vs down the stairs
            float dot = Vector2.Dot(this.Direction, obj.StairDirection);
            
            if (Math.Abs(dot) > 0.1f)
            {
                // Directly assign the stair direction vector (sign based on dot)
                Vector2 stairDir = obj.StairDirection;
                if (dot < 0) stairDir = new Vector2(-stairDir.X, -stairDir.Y);
                
                // Bypass the normalizing setter by setting the backing field value directly
                obj.Direction = stairDir;
            }
            else
            {
                obj.Direction = Vector2.Zero;
            }
        }
        else
        {
            obj.Direction = this.Direction;
        }
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