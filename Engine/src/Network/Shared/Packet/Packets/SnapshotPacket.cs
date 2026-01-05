using Engine.Network.Shared.Object;

namespace Engine.Network.Shared.Packet.Packets;

public struct ReplicatedObjectData
{
    public uint TypeId;
    
    public int NetworkId;
    public int OwningClientId;

    public byte[] NetPropertyBlob;
    public List<ReplicatedObjectData> Children;
}

public class SnapshotPacket : Packet
{
    public override PacketType Type => PacketType.SnapshotPacket;

    public uint Tick { get; set; }
    public uint SceneEpoch { get; set; }

    public Dictionary<int, uint> LastProcessedSequencePerClient { get; set; } = new();
    
    public List<ReplicatedObjectData> ReplicatedObjects { get; set; } = new();
    
    public override void Serialize(BinaryWriter bw)
    {
        bw.Write(Tick);
        bw.Write(SceneEpoch);
        
        bw.Write(LastProcessedSequencePerClient.Count);
        foreach (var kvp in LastProcessedSequencePerClient)
        {
            bw.Write(kvp.Key);
            bw.Write(kvp.Value);
        }

        // Serialize all replicated root children
        bw.Write(ReplicatedObjects.Count);
        foreach (var obj in ReplicatedObjects)
        {
            SerializeTree(obj, bw);
        }
    }

    public override void Deserialize(BinaryReader br)
    {
        Tick = br.ReadUInt32();
        SceneEpoch = br.ReadUInt32();

        int ackCount = br.ReadInt32();
        for (int i = 0; i < ackCount; i++)
        {
            int clientId = br.ReadInt32();
            uint seq = br.ReadUInt32();
            this.LastProcessedSequencePerClient[clientId] = seq;
        }

        int objCount = br.ReadInt32();
        ReplicatedObjects = new List<ReplicatedObjectData>();
        for (int i = 0; i < objCount; i++)
        {
            ReplicatedObjects.Add(DeserializeTree(br));
        }
    }

    private void SerializeTree(ReplicatedObjectData node, BinaryWriter bw)
    {
        bw.Write(node.TypeId);
        
        bw.Write(node.NetworkId);
        bw.Write(node.OwningClientId);

        bw.Write(node.NetPropertyBlob.Length);
        bw.Write(node.NetPropertyBlob);

        int childCount = node.Children?.Count ?? 0;
        bw.Write(childCount);
        if (node.Children != null)
        {
            foreach (var c in node.Children)
            {
                SerializeTree(c, bw);
            }
        }
    }

    private ReplicatedObjectData DeserializeTree(BinaryReader br)
    {
        var node = new ReplicatedObjectData
        {
            TypeId = br.ReadUInt32(),
            NetworkId = br.ReadInt32(),
            OwningClientId = br.ReadInt32()
        };

        int blobLength = br.ReadInt32();
        node.NetPropertyBlob = br.ReadBytes(blobLength);

        int childCount = br.ReadInt32();
        node.Children = new List<ReplicatedObjectData>();
        for (int i = 0; i < childCount; i++)
        {
            node.Children.Add(DeserializeTree(br));
        }

        return node;
    }
}