using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprite;

public class AnimatedSprite : Sprite
{
    private readonly int _spriteWidth;
    private readonly int _spriteHeight;
    private readonly bool _isLooping;
    private double _elapsedTimeSinceFrame = 0;
    private int _currentFrame = 0;
    private int _totalFrames => Texture.Width / _spriteWidth;

    public AnimatedSprite(Texture2D spriteSheet, int spriteWidth, int spriteHeight, bool isLooping = false) : base(spriteSheet)
    {
        this._spriteWidth = spriteWidth;
        this._spriteHeight = spriteHeight;
        this._isLooping = isLooping;
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        _elapsedTimeSinceFrame += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_elapsedTimeSinceFrame >= 200) // Change frame every 200 ms
        {
            _elapsedTimeSinceFrame = 0;
            _currentFrame++;
            if (_currentFrame >= _totalFrames)
            {
                if (_isLooping)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = _totalFrames - 1; // Stay on last frame
                }
            }

            SourceRectangle = GetSpriteFromSheet(0, _currentFrame);
        }

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