using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprite;

public class AnimatedSprite : Sprite
{
    public int Column { get; set; } = 0;

    public readonly int _spriteWidth;
    public readonly int _spriteHeight;
    private readonly bool _isLooping;
    private bool _isPlaying = false;
    private double _elapsedTimeSinceFrame = 0;
    private int _currentFrame = 0;
    private int _totalFrames => Texture.Width / _spriteWidth;

    public event Action? AnimationCompleted;

    public AnimatedSprite(Texture2D spriteSheet, int spriteWidth, int spriteHeight, bool isLooping = false, bool isPlaying = true) : base(spriteSheet)
    {
        this._spriteWidth = spriteWidth;
        this._spriteHeight = spriteHeight;
        this._isLooping = isLooping;
        this._isPlaying = isPlaying;
        SourceRectangle = GetSpriteFromSheet(0, 0);
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
       UpdateAnimation(gameTime);
    }
    public void Play()
    {
        _isPlaying = true;
        _currentFrame = 0;
        SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
    }
    public void Reset()
    {
        _isPlaying = false;
        _currentFrame = 0;
        SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
    }
    public void Stop()
    {
        _isPlaying = false;
    }
    private void UpdateAnimation(GameTime gameTime)
    {
        if (!_isPlaying) return;

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
                    AnimationCompleted?.Invoke();
                    _currentFrame = _totalFrames - 1; // Stay on last frame
                    Stop();
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