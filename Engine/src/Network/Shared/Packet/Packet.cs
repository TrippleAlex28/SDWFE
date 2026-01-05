namespace Engine.Network.Shared.Packet;

public enum PacketType : byte
{
    ConnectionRequestPacket = 1,    // Client to Server
    ConnectionAcceptPacket = 2,     // Server to Client
    EmptyUdpPacket = 3,             // Client to Server
    DisconnectPacket = 4,           // Both
    
    ChatPacket = 5,                 // Both
    CommandPacket = 6,              // Client to Server
    SnapshotPacket = 7,             // Server to Client
    SceneChangePacket = 8,          // Server to Client
}

public abstract class Packet
{
    /// <summary>
    /// Unique identifier for packet type (assigned automatically)
    /// </summary>
    public abstract PacketType Type { get; }

    public abstract void Serialize(BinaryWriter bw);

    public abstract void Deserialize(BinaryReader br);

    public byte[] ToBytes()
    {
        using (MemoryStream ms = new())
        using (BinaryWriter bw = new(ms))
        {
            bw.Write((byte)Type);
            Serialize(bw);
            return ms.ToArray();
        }
    }
}