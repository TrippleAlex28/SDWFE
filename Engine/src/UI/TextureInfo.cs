using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI;

// Struct to help drawing logic, as we always need a sourceRect and Texture, if we are using a spriteSheet
public struct TextureInfo
{
    public Texture2D Texture;
    public Rectangle sourceRect = new Rectangle(0, 0, 0, 0);
    public Vector4 NinePatchSlices = new Vector4(0, 0, 0, 0);

    public TextureInfo(Texture2D texture, Rectangle sourceRect = default, Vector4 ninePatchSlices = default)
    {
        Texture = texture;
        this.sourceRect = sourceRect;
        this.NinePatchSlices = ninePatchSlices;
    }
}