namespace Engine.Network.Shared.Packet;

public static class PacketRegistry
{
    private static readonly Dictionary<PacketType, Func<Packet>> _packetFactories = new();

    /// <summary>
    /// Register a packet type 
    /// </summary>
    public static void Register<T>(PacketType id) where T : Packet, new()
    {
        if (_packetFactories.ContainsKey(id))
            throw new InvalidOperationException($"Packet ID \'{id}\' already registered");

        _packetFactories[id] = () => new T();
    }

    /// <summary>
    /// Create a packet instance from a packet ID
    /// </summary>
    public static Packet CreatePacket(PacketType id)
    {
        if (!_packetFactories.TryGetValue(id, out var factory))
            throw new InvalidOperationException($"Unknown Packet ID \'{id}\'");

        return factory();
    }

    /// <summary>
    /// Deserialize a packet from a byte array
    /// </summary>
    public static Packet Deserialize(byte[] data)
    {
        using (MemoryStream ms = new(data))
        using (BinaryReader br = new(ms))
        {
            PacketType packetId = (PacketType)br.ReadByte();
            Packet packet = CreatePacket(packetId);
            packet.Deserialize(br);

            return packet;
        }
    }
}