using Microsoft.Xna.Framework;

namespace Engine.Settings;

public class WindowSettings
{
    public bool FullScreen { get; set; } = false;

    // Windowed-mode size 
    public int WindowWidth { get; set; } = ExtendedGame.DrawResolution.X;
    public int WindowHeight { get; set; } = ExtendedGame.DrawResolution.Y;
    
    // Fullscreen resolution (usually display resolution)
    public int ScreenWidth { get; set; } = 0;
    public int ScreenHeight { get; set; } = 0;
}