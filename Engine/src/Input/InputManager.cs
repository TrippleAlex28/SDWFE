using System.Text.Json;
using Engine.Input.Binding;
using Engine.Input.Binding.Bindings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input;

/// <summary>
/// Singleton manager for handling input profiles and input state
/// </summary>
public sealed class InputManager
{
    private static InputManager? _instance;
    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new InputManager();
            return _instance;
        }
    }

    private Dictionary<string, InputProfile> _profiles;
    private InputProfile _activeProfile;
    private InputState _inputState;
    
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };
    
    private InputManager()
    {
        _profiles = new Dictionary<string, InputProfile>();
        _inputState = new InputState();
    }

    /// <summary>
    /// Updates input state
    /// </summary>
    public void Update()
    {
        _inputState.Update();
    }
    
    public InputState GetInputState()
    {
        return _inputState;
    }

    #region Profile 
    
    /// <summary>
    /// Registers a new input profile
    /// </summary>
    public void RegisterProfile(InputProfile profile)
    {
        _profiles[profile.Name] = profile;
            
        // Set as active if this is the first profile
        if (_activeProfile == null)
            _activeProfile = profile;
    }

    /// <summary>
    /// Gets a profile by name
    /// </summary>
    public InputProfile GetProfile(string profileName)
    {
        _profiles.TryGetValue(profileName, out var profile);
        return profile;
    }

    /// <summary>
    /// Sets the active input profile
    /// </summary>
    public void SetActiveProfile(string profileName)
    {
        if (_profiles.TryGetValue(profileName, out var profile))
        {
            _activeProfile = profile;
        }
        else
        {
            throw new ArgumentException($"Profile '{profileName}' not found");
        }
    }

    /// <summary>
    /// Gets the currently active profile
    /// </summary>
    public InputProfile GetActiveProfile()
    {
        return _activeProfile;
    }

    /// <summary>
    /// Gets all registered profiles
    /// </summary>
    /// <returns></returns>
    public IEnumerable<InputProfile> GetAllProfiles()
    {
        return _profiles.Values;
    }
    
    #endregion
    
    #region Actions 
    
    /// <summary>
    /// Convenience method to check if an action in the active profile is pressed
    /// </summary>
    public bool IsActionPressed(string actionName)
    {
        return _activeProfile?.IsActionPressed(actionName, _inputState) ?? false;
    }

    /// <summary>
    /// Convenience method to check if an action in the active profile is held down
    /// </summary>
    public bool IsActionDown(string actionName)
    {
        return _activeProfile?.IsActionDown(actionName, _inputState) ?? false;
    }

    /// <summary>
    /// Convenience method to check if an action in the active profile is released
    /// </summary>
    public bool IsActionReleased(string actionName)
    {
        return _activeProfile?.IsActionReleased(actionName, _inputState) ?? false;
    }
    
    #endregion
    
    #region Mouse

    public Point MousePosition => new Point(_inputState.CurrentMouse.X, _inputState.CurrentMouse.Y);
    
    #endregion 
    
    #region Config

    /// <summary>
    /// Saves the current input config to a JSON file
    /// </summary>
    public static bool SaveToFile(string filePath)
    {
        try
        {
            var config = SerializeCurrentConfig();
            string json = JsonSerializer.Serialize(config, _jsonOptions);
            
            // Ensure directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputManager.SaveToFile Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads input config from a JSON file
    /// </summary>
    public static bool LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new InvalidOperationException($"Input config file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<SerializedInputConfig>(json, _jsonOptions);

            if (config != null)
            {
                DeserializeConfig(config);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputManager.LoadFromFile Exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Serializes the current InputManager state to a data structure
    /// </summary>
    private static SerializedInputConfig SerializeCurrentConfig()
    {
        var serializedConfig = new SerializedInputConfig();
        
        // Serialize all profiles
        foreach (var profile in InputManager.Instance.GetAllProfiles())
        {
            var serializedProfile = new SerializedProfile
            {
                Name = profile.Name,
            };
            
            // Serialize all actions in the profile
            foreach (var action in profile.GetAllActions())
            {
                var serializedAction = new SerializedAction
                {
                    Name = action.Name,
                };
                
                // Serialize all bindings
                foreach (var binding in action.Bindings)
                {
                    var serializedBinding = SerializeBinding(binding);
                    if (serializedBinding != null)
                    {
                        serializedAction.Bindings.Add(serializedBinding);
                    }
                }
                
                serializedProfile.Actions.Add(serializedAction);
            }

            serializedConfig.Profiles.Add(serializedProfile);
        }

        return serializedConfig;
    }

    /// <summary>
    /// Deserializes a config and applies it to the InputManager
    /// </summary>
    private static void DeserializeConfig(SerializedInputConfig config)
    {
        // Load all profiles
        foreach (var serializedProfile in config.Profiles)
        {
            var profile = new InputProfile(serializedProfile.Name);
            
            // Load all actions
            foreach (var serializedAction in serializedProfile.Actions)
            {
                var action = new InputAction(serializedAction.Name);
                
                // Load all bindings
                foreach (var serializedBinding in serializedAction.Bindings)
                {
                    var binding = DeserializeBinding(serializedBinding);
                    if (binding != null)
                    {
                        action.AddBinding(binding);
                    }
                }
                
                profile.RegisterAction(action);
            }

            InputManager.Instance.RegisterProfile(profile);
        }
    }

    /// <summary>
    /// Converts an InputBinding to a SerializedBinding
    /// </summary>
    private static SerializedBinding? SerializeBinding(InputBinding binding)
    {
        switch (binding)
        {
            case KeyboardBinding kb:
                return new SerializedBinding("Keyboard", kb.Key.ToString());
            case MouseButtonBinding mb:
                return new SerializedBinding("Mouse", mb.Button.ToString());
            case GamePadButtonBinding gb:
                return new SerializedBinding("GamePad", $"{gb.Button}|{gb.PlayerIndex}");
            default:
                return null;
        }
    }

    /// <summary>
    /// Converts a SerializedBinding back to an InputBinding
    /// </summary>
    private static InputBinding? DeserializeBinding(SerializedBinding serialized)
    {
        try
        {
            switch (serialized.Type)
            {
                case "Keyboard":
                    if (Enum.TryParse<Keys>(serialized.Data, out var key))
                    {
                        return new KeyboardBinding(key);
                    }
                    break;
                case "Mouse":
                    if (Enum.TryParse<MouseButtonBinding.MouseButton>(serialized.Data, out var mouseButton))
                    {
                        return new MouseButtonBinding(mouseButton);
                    }
                    break;
                case "GamePad":
                    var parts = serialized.Data.Split('|');
                    if (parts.Length == 2 &&
                        Enum.TryParse<Buttons>(parts[0], out var button) &&
                        int.TryParse(parts[1], out var playerIndex))
                    {
                        return new GamePadButtonBinding(button, playerIndex);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputManager.DeserializeBinding Exception: {ex.Message}");
        }
        
        return null;
    }

    public static string GetDefaultConfigPath(string gameName)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string gameFolder = Path.Combine(appData, gameName);
        return Path.Combine(gameFolder, "input_config.json");
    }
    
    #endregion
}