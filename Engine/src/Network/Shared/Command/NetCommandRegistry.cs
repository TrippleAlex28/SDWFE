namespace Engine.Network.Shared.Command;

public static class NetCommandRegistry
{
    private static readonly Dictionary<uint, Func<NetCommand>> _commandFactories = new();

    /// <summary>
    /// Register a command type 
    /// </summary>
    public static void Register<T>(uint id) where T : NetCommand, new()
    {
        if (_commandFactories.ContainsKey(id))
            throw new InvalidOperationException($"NetCommand ID \'{id}\' already registered");

        _commandFactories[id] = () => new T();
    }

    /// <summary>
    /// Create a command instance from a command ID
    /// </summary>
    public static NetCommand CreateCommand(uint id)
    {
        if (!_commandFactories.TryGetValue(id, out var factory))
            throw new InvalidOperationException($"Unknown NetCommand ID \'{id}\'");

        return factory();
    }

    /// <summary>
    /// Deserialize a command from a byte array
    /// </summary>
    public static NetCommand Deserialize(byte[] data)
    {
        using (MemoryStream ms = new(data))
        using (BinaryReader br = new(ms))
        {
            return Deserialize(br);
        }
    }

    public static NetCommand Deserialize(BinaryReader br)
    {
        uint commandId = br.ReadUInt32();
        NetCommand command = CreateCommand(commandId);
        command.SequenceNumber = br.ReadUInt32();
        command.Tick = br.ReadUInt32();
        command.Deserialize(br);

        return command;
    }
}