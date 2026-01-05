namespace Engine.Network.Shared.Packet.Packets;

public class DisconnectPacket : Packet
{
    public override PacketType Type => PacketType.DisconnectPacket;

    public string Reason { get; set; } = "No Reason";

    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(Reason);
    }

    public override void Deserialize(BinaryReader br)
    {
        Reason = br.ReadString();
    }
}