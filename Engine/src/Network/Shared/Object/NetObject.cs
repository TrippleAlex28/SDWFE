using System.Runtime.CompilerServices;

namespace Engine.Network.Shared.Object;

public abstract class NetObject
{
    public abstract uint TypeId { get; }
    
    public bool ReplicatesOverNetwork { get; set; } = false;
    public int NetworkId { get; set; } = -1;
    public int OwningClientId { get; set; } = -1;

    private readonly List<NetProperty> _properties = new();

    public NetObject() : base()
    {
        
    }

    protected NetProperty<T> RegisterProperty<T>(string name, Func<T> getter, Action<T> setter)
    {
        NetProperty<T> p = new(name, getter, setter);
        _properties.Add(p);
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

        bw.Write(list.Count);

        foreach (var p in list)
        {
            bw.Write(p.Name);
            p.Serialize(bw);
        }
    }

    public void Deserialize(BinaryReader br)
    {
        int count = br.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            string name = br.ReadString();
            NetProperty? p = _properties.FirstOrDefault(p => p.Name == name);
            p?.Deserialize(br);
        }
    }
}