namespace Engine.Input;

[Serializable]
public class SerializedInputConfig
{
    public List<SerializedProfile> Profiles { get; set; }

    public SerializedInputConfig()
    {
        Profiles = new List<SerializedProfile>();
    }
}

[Serializable]
public class SerializedProfile
{
    public string Name { get; set; } = string.Empty;
    public List<SerializedAction> Actions { get; set; }

    public SerializedProfile()
    {
        Actions = new List<SerializedAction>();
    }
}

[Serializable]
public class SerializedAction
{
    public string Name { get; set; } = string.Empty;
    public List<SerializedBinding> Bindings { get; set; }

    public SerializedAction()
    {
        Bindings = new List<SerializedBinding>();
    }
}

[Serializable]
public class SerializedBinding
{
    public string Type { get; set; } = string.Empty; // "Keyboard" "Mouse" "GamePad"
    public string Data { get; set; } = string.Empty; // Binding data
    
    public SerializedBinding() { }

    public SerializedBinding(string type, string data)
    {
        Type = type;
        Data = data;
    }
}