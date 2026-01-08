using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Engine.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.Input;

public class RoomDoor : GameObject
{
    public Rectangle _hitBox;

    private int _tileSize = 16;
    private Texture2D _spriteSheet;
    private Rectangle _sourceRect;
    
    private List<Rectangle> _animationSourceRects = new();
    private int _currentFrame = 1;
    private int _animationTotalFrames = 1;
    private float _animationTotalTime = 0.5f;
    private float _animationProgressTime = 0.0f;
    private bool _animationPlaying = false;
    
    public RoomDoor(Texture2D spriteSheet, Vector2 globalPosition, int tileSize, int animationTotalFrames, Rectangle? sourceRect = null, Rectangle? hitbox = null)
    {
        this._tileSize = tileSize;
        this._spriteSheet = spriteSheet;
        this.GlobalPosition = globalPosition;
        this._animationTotalFrames = animationTotalFrames;
        this._sourceRect = sourceRect ?? new Rectangle(0, 0, _tileSize, _tileSize);
        this._hitBox = hitbox ?? new Rectangle(0, 0, 0, 0);

        this.BaseDrawLayer =  (float)(0.8f / ExtendedGame.DrawResolution.Y) * (_hitBox.Y + _hitBox.Size.Y / 2); 

        InitializeFrames();
    }

    public void StartAnimation()
    {
        _animationPlaying = true;
    }
    public void Reset()
    {
        _animationPlaying = false;
        _currentFrame = 1;
        _sourceRect = _animationSourceRects[0];
        _animationProgressTime = 0.0f;
    }
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

        if (_animationPlaying)
        {
            _animationProgressTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateAnimation();
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        Rectangle destRect = new Rectangle((int)GlobalPosition.X, (int)GlobalPosition.Y, _tileSize, _tileSize);
        spriteBatch.Draw(_spriteSheet, destRect, _sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
    }
    private void UpdateAnimation()
    {
        _currentFrame = Math.Clamp((int)(_animationProgressTime / _animationTotalTime * _animationTotalFrames), 0, _animationTotalFrames - 1);
        _sourceRect = _animationSourceRects[_currentFrame];

        if (_animationProgressTime >= _animationTotalTime)
        {
            FinishedAnimation();
        }
    }

    private void FinishedAnimation()
    {
        _animationPlaying = false;
    }
    private void InitializeFrames()
    {
        for (int i = 0; i < _animationTotalFrames; i++)
        {
            int currentX = _sourceRect.X / _tileSize;

            int x = (currentX + i) % (_spriteSheet.Width / _tileSize);
            int y = (currentX + i) / (_spriteSheet.Width / _tileSize);

            _animationSourceRects.Add(new Rectangle(x * _tileSize, y * _tileSize, _tileSize, _tileSize));
        }
    }
    
}