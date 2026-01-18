using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine;

public static class EngineResources
{
    /// <summary>
    /// 1x1 Placeholder white block texture that can be used for testing purposes
    /// </summary>
    public static Texture2D BlankSquare { get; private set; } = null!;

    /// <summary>
    /// 32x32 Placeholder white circle texture that can be used for testing purposes
    /// </summary>
    public static Texture2D BlankCircle { get; private set; } = null!;
    
    public static void LoadContent(GraphicsDevice graphics)
    {
        // Setup BlankSquare
        BlankSquare = new Texture2D(graphics, 1, 1);
        BlankSquare.SetData([Color.White]);

        // Setup BlankCircle
        BlankCircle = new Texture2D(graphics, 32, 32);
        Color[] data = new Color[32 * 32];
        for (int i = 0; i < data.Length; i++)
        {
            int x = i % 32 - 16;
            int y = i / 32 - 16;
            data[i] = (x * x + y * y <= 16 * 16) ? Color.White : Color.Transparent;
        }
        BlankCircle.SetData(data);
    }
}