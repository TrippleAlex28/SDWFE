using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Settings;

public class SettingsManager
{
    private class SettingsData
    {
        public WindowSettings WindowSettings { get; set; }
        public AudioSettings AudioSettings { get; set; }
    }
    
    private static SettingsManager? _instance;
    public static SettingsManager Instance => _instance ??= new SettingsManager();

    public WindowSettings WindowSettings { get; private set; }
    public AudioSettings AudioSettings { get; private set; }

    private GraphicsDeviceManager _graphics;
    
    private SettingsManager()
    {
        WindowSettings = new WindowSettings();
        AudioSettings = new AudioSettings();
    }

    public void Initialize(GraphicsDeviceManager graphics)
    {
        _graphics = graphics;
        LoadSettings(GetDefaultConfigPath(ExtendedGame.GAME_NAME));
        ApplyResolutionSettings();
    }
    
    #region Window Settings

    public void ToggleFullScreen()
    {
        SetFullScreen(!WindowSettings.FullScreen);
    }
    
    public void SetFullScreen(bool fullScreen)
    {
        WindowSettings.FullScreen = fullScreen;

        if (fullScreen && (WindowSettings.ScreenWidth <= 0 || WindowSettings.ScreenHeight <= 0))
        {
            var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            WindowSettings.ScreenWidth = mode.Width;
            WindowSettings.ScreenHeight = mode.Height;
        }

        ApplyResolutionSettings();
    }

    public void SetWindowedResolution(int width, int height)
    {
        WindowSettings.WindowWidth = Math.Max(1, width);
        WindowSettings.WindowHeight = Math.Max(1, height);
        
        if (!WindowSettings.FullScreen)
            ApplyResolutionSettings();
    }

    private void ApplyResolutionSettings()
    {
        Point targetSize = WindowSettings.FullScreen
            ? new Point(WindowSettings.ScreenWidth, WindowSettings.ScreenHeight)
            : new Point(WindowSettings.WindowWidth, WindowSettings.WindowHeight);
        
        _graphics.IsFullScreen = WindowSettings.FullScreen;
        _graphics.PreferredBackBufferWidth = targetSize.X;
        _graphics.PreferredBackBufferHeight = targetSize.Y;
        _graphics.ApplyChanges();
    }
    
    #endregion 
    
    #region Saving & Loading 
    
    public void LoadSettings(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return;
            
            string json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            
            if (data != null)
            {
                WindowSettings = data.WindowSettings;
                AudioSettings = data.AudioSettings;
                ApplyResolutionSettings();
            }
        }
        catch (Exception ex)
        {
            // Log error or handle appropriately
            Console.WriteLine($"Failed to load settings: {ex.Message}");
            
        }
    }

    public void SaveSettings(string filePath)
    {
        try
        {
            // Ensure directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = new SettingsData
            {
                WindowSettings = WindowSettings,
                AudioSettings = AudioSettings
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            // Log error or handle appropriately
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
    
    public static string GetDefaultConfigPath(string gameName)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string gameFolder = Path.Combine(appData, gameName);
        return Path.Combine(gameFolder, "settings_config.json");
    }
    
    #endregion
}