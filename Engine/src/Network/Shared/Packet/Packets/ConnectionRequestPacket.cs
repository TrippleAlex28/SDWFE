namespace Engine.Network.Shared.Packet.Packets;

public class ConnectionRequestPacket : Packet
{
    public override PacketType Type => PacketType.ConnectionRequestPacket;

    // Client Data
    public string ClientName { get; set; } = "Client Name";
    public int ClientUdpPort { get; set; } = 0;

    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(ClientName);
        bw.Write(ClientUdpPort);
    }

    public override void Deserialize(BinaryReader br)
    {
        ClientName = br.ReadString();
        ClientUdpPort = br.ReadInt32();
    }
}