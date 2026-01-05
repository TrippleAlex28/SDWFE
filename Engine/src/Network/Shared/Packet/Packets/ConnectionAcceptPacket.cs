namespace Engine.Network.Shared.Packet.Packets;

public class ConnectionAcceptPacket : Packet
{
    public override PacketType Type => PacketType.ConnectionAcceptPacket;
    
    // Client Assignment Data
    public int ClientId { get; set; }
    
    // Server Data
    public int ServerUdpPort { get; set; }

    // Session Data
    public uint CurrentSceneEpoch { get; set; }
    public string CurrentSceneKey { get; set; }
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(ClientId);
        
        bw.Write(ServerUdpPort);
        
        bw.Write(CurrentSceneEpoch);
        bw.Write(CurrentSceneKey);
    }

    public override void Deserialize(BinaryReader br)
    {
        ClientId = br.ReadInt32();
        
        ServerUdpPort = br.ReadInt32();

        CurrentSceneEpoch = br.ReadUInt32();
        CurrentSceneKey = br.ReadString();
    }
}