using Microsoft.Xna.Framework;

namespace Engine.Network.Shared.Object;

public abstract class NetProperty
{
    public string Name { get; protected set; }
    protected bool isDirty;
    public bool IsDirty => isDirty;

    public void ClearDirty()
    {
        isDirty = false;
    }

    public abstract void UpdateDirty();
    
    public abstract void Serialize(BinaryWriter bw);
    public abstract void Deserialize(BinaryReader br);
}

public class NetProperty<T> : NetProperty
{
    private T _lastValue;
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;
    
    public NetProperty(
        string name,
        Func<T> getter,
        Action<T> setter
    )
    {
        Name = name;
        _getter = getter;
        _setter = setter;
        _lastValue = _getter();
    }

    public override void UpdateDirty()
    {
        T current = _getter();
        if (!EqualityComparer<T>.Default.Equals(current, _lastValue))
        {
            _lastValue = current;
            isDirty = true;
        }
    }

    public override void Serialize(BinaryWriter bw)
    {
        object? value = _getter();

        switch(default(T))
        {
            case bool:
                bw.Write((bool)value!);
                break;
            case int:
                bw.Write((int)value!);
                break;
            case float:
                bw.Write((float)value!);
                break;
            case string:
                bw.Write((string)value!);
                break;
            case Vector2:
                bw.Write((Vector2)value!);
                break;
            default:
                throw new NotSupportedException($"NetProperty<T> - T: {typeof(T)} not supported!");
        }
    }

    public override void Deserialize(BinaryReader br)
    {
        T value;

        switch(default(T))
        {
            case bool:
                value = (T)(object)br.ReadBoolean();
                break;
            case int:
                value = (T)(object)br.ReadInt32();
                break;
            case float:
                value = (T)(object)br.ReadSingle();
                break;
            case string:
                value = (T)(object)br.ReadString();
                break;
            case Vector2:
                value = (T)(object)br.ReadVector2();
                break;
            default:
                throw new NotSupportedException($"NetProperty<T> - T: {typeof(T)} not supported!");
        }

        _setter(value);
        _lastValue = value;
    }
}