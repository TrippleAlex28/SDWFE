namespace Engine.Network.Shared.Packet.Packets;

public class EmptyUdpPacket : Packet
{
    public override PacketType Type => PacketType.EmptyUdpPacket;

    public override void Serialize(BinaryWriter bw)
    {
    }

    public override void Deserialize(BinaryReader br)
    {
    }
}