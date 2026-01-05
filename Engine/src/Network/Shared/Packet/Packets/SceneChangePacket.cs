namespace Engine.Network.Shared.Packet.Packets;

public class SceneChangePacket : Packet
{
    public override PacketType Type => PacketType.SceneChangePacket;

    public uint SceneEpoch { get; set; }
    public string SceneKey { get; set; }

    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(SceneEpoch);
        bw.Write(SceneKey);
    }

    public override void Deserialize(BinaryReader br)
    {
        SceneEpoch = br.ReadUInt32();
        SceneKey = br.ReadString();
    }
}