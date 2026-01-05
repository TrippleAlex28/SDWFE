namespace Engine.Network.Shared.Command;

public abstract class NetCommand
{
    public abstract uint Type { get; }
    
    public uint SequenceNumber { get; set; }
    public uint Tick { get; set; }

    public abstract void Apply(Scene.Scene scene, int clientId);
    
    public abstract void Serialize(BinaryWriter bw);
    public abstract void Deserialize(BinaryReader br);

    public byte[] ToBytes()
    {
        using (MemoryStream ms = new())
        using (BinaryWriter bw = new(ms))
        {
            bw.Write(Type);
            bw.Write(SequenceNumber);
            bw.Write(Tick);
            Serialize(bw);
            return ms.ToArray();
        }
    }
}