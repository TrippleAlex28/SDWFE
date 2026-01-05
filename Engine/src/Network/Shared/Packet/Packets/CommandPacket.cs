using Engine.Network.Shared.Command;

namespace Engine.Network.Shared.Packet.Packets;

public class CommandPacket : Packet
{
    public override PacketType Type => PacketType.CommandPacket;

    public int ClientId { get; set; }
    public List<NetCommand> Commands { get; set; } = new();
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(ClientId);
        
        bw.Write(Commands.Count);
        foreach (var c in Commands)
        {
            bw.Write(c.ToBytes());
        }
    }

    public override void Deserialize(BinaryReader br)
    {
        ClientId = br.ReadInt32();

        int count = br.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            NetCommand c = NetCommandRegistry.Deserialize(br);
            Commands.Add(c);
        }
    }
}