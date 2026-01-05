namespace Engine.Network.Shared.Packet.Packets;

public class ChatPacket : Packet
{
    public override PacketType Type => PacketType.ChatPacket;

    public int ClientId { get; set; }
    public string Message { get; set; } = "Message";
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(ClientId);
        bw.Write(Message);
    }

    public override void Deserialize(BinaryReader br)
    {
        ClientId = br.ReadInt32();
        Message = br.ReadString();
    }
}