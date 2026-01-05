using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprite;

public class AnimatedSprite : Sprite
{
    private readonly int _spriteWidth;
    private readonly int _spriteHeight;

    public AnimatedSprite(Texture2D spriteSheet, int spriteWidth, int spriteHeight) : base(spriteSheet)
    {
        this._spriteWidth = spriteWidth;
        this._spriteHeight = spriteHeight;
    }

    public Rectangle GetSpriteFromSheet(int row, int column)
    {
        return new Rectangle(
            this._spriteWidth * column,
            this._spriteHeight * row,
            this._spriteWidth,
            this._spriteHeight);
    }
}