using Engine;
using Engine.Sprite;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE;

public static class Resources
{
    public static SpriteFont TitleFont { get; private set; }
    public static SpriteFont TextFont { get; private set; }
    public static string UPHEAVEL_FONTNAME { get; private set; } = "Upheavel";
    public static string VISITOR_FONTNAME { get; private set; } = "Visitor";
    public static string INTRIGORA_FONTNAME { get; private set; } = "Intrigora";

    
    public static void LoadContent()
    {
        TitleFont = ExtendedGame.AssetManager.LoadFont(VISITOR_FONTNAME, "Fonts/");
        TextFont = ExtendedGame.AssetManager.LoadFont(UPHEAVEL_FONTNAME, "Fonts/");
    }

    /// <summary>
    /// Loads a font by name and size. Attempts to load "{fontName}{size}" (e.g., "Visitor24").
    /// </summary>
    /// <param name="fontName">The base font name (e.g., "Visitor")</param>
    /// <param name="size">The font size in pixels (e.g., 24)</param>
    /// <param name="path">The content path (default: "Fonts/")</param>
    /// <returns>The loaded SpriteFont, or the default font if sized version doesn't exist</returns>
    public static SpriteFont GetFont(string fontName, int size, string path = "Fonts/")
    {
        string sizedFontName = $"{fontName}{size}";
        
        try
        {
            return ExtendedGame.AssetManager.LoadFont(sizedFontName, path);
        }
        catch
        {
            // Fall back to base font if sized version doesn't exist
            return ExtendedGame.AssetManager.LoadFont(fontName, path);
        }
    }
}
