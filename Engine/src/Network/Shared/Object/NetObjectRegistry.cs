namespace Engine.Network.Shared.Object;

public static class NetObjectRegistry
{
    private static readonly Dictionary<uint, Func<NetObject>> _objectFactories = new();
    
    public static void Register<T>(uint typeId) where T : NetObject, new()
    {
        _objectFactories[typeId] = () => new T();
    }

    public static NetObject Create(uint typeId)
    {
        return _objectFactories[typeId]();
    }
}