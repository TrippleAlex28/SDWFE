using System.Runtime.CompilerServices;

namespace Engine.Network.Shared.Object;

public abstract class NetObject
{
    public abstract uint TypeId { get; }
    
    public bool ReplicatesOverNetwork { get; set; } = false;
    public int NetworkId { get; set; } = -1;
    public int OwningClientId { get; set; } = -1;

    private readonly List<NetProperty> _properties = new();
    private readonly Dictionary<ushort, NetProperty> _propertyById = new();

    protected NetProperty<T> RegisterProperty<T>(ushort id, string debugName, Func<T> getter, Action<T> setter)
    {
        if (_propertyById.ContainsKey(id))
            throw new InvalidOperationException($"Duplicate NetProperty id {id} on {GetType().Name}");
        
        var p = new NetProperty<T>(id, debugName, getter, setter);
        _properties.Add(p);
        _propertyById.Add(id, p);
        return p;
    }

    public bool IsLocallyOwned()
    {
        var session = GameState.Instance.SessionManager.CurrentSession;
        if (session == null) return true;
        return OwningClientId == session.LocalClientId;
    }

    public bool HasAuthority()
    {
        var sessionManager = GameState.Instance.SessionManager;
        if (sessionManager.CurrentSession == null || sessionManager.IsSingleplayer) return true;
        if (sessionManager.IsHost) return true;
        return IsLocallyOwned();
    }
    
    public void ClearDirty()
    {
        foreach (var p in _properties)
        {
            p.ClearDirty();
        }
    }

    public void UpdateDirty()
    {
        foreach (var p in _properties)
        {
            p.UpdateDirty();
        }
    }

    public void Serialize(BinaryWriter bw, bool onlyDirty = false)
    {
        List<NetProperty> list = onlyDirty ? _properties.Where(p => p.IsDirty).ToList() : _properties;

        bw.Write((ushort)list.Count);

        foreach (var p in list)
        {
            bw.Write(p.Id);
            p.Serialize(bw);
        }
    }

    public void Deserialize(BinaryReader br)
    {
        ushort count = br.ReadUInt16();

        for (ushort i = 0; i < count; i++)
        {
            ushort id = br.ReadUInt16();
            if (_propertyById.TryGetValue(id, out var p))
            {
                p.Deserialize(br);
            }
            else
            {
                // ===== ALDUS CHATGPT ======
                // IMPORTANT: you must still advance the stream if you don't know the property.
                // With the current format (id then raw value), you *cannot* skip unknown types safely.
                // So: either treat as protocol mismatch error, or add a length prefix per property (see note below).
                throw new InvalidOperationException($"Unknown property id {id} for {GetType().Name}");
            }
        }
    }
}