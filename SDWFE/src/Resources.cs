using Engine;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE;

public static class Resources
{
    public static SpriteFont TitleFont { get; private set; }
    public static SpriteFont TextFont { get; private set; }
    
    public static void LoadContent()
    {
        TitleFont = ExtendedGame.AssetManager.LoadFont("Visitor", "Fonts/");
        TextFont = ExtendedGame.AssetManager.LoadFont("Upheavel", "Fonts/");
    }
}