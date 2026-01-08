using System.Text.Json;

public static class JsonManager
{
    public static void Save<T>(T obj, String path)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(obj, options);
        File.WriteAllText(path, json);
    }
    
    public static T Load<T>(string path) where T : new()
    {
        if (!File.Exists(path)) return new T();
        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json) ?? new T();
    }
}