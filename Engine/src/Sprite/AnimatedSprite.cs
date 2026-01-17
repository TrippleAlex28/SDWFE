using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprite;

/// <summary>
/// Stores animation data for a single animation
/// </summary>
public class AnimationData
{
    public Texture2D Texture { get; set; }
    public int Column { get; set; }
    public float TimePerFrame { get; set; }
    public bool IsLooping { get; set; }
    
    public AnimationData(Texture2D texture, int column, float timePerFrame = 200f, bool isLooping = true)
    {
        Texture = texture;
        Column = column;
        TimePerFrame = timePerFrame;
        IsLooping = isLooping;
    }
}

public class AnimatedSprite : Sprite
{
    public int Column { get; set; } = 0;

    public readonly int _spriteWidth;
    public readonly int _spriteHeight;
    private bool _isLooping;
    private bool _isPlaying = false;
    private double _elapsedTimeSinceFrame = 0;
    private int _currentFrame = 0;
    private int _totalFrames => Texture.Width / _spriteWidth;

    private float _timePerFrame = 200f; // milliseconds per frame

    // Queue for playing animations once and returning to previous
    private AnimationData? _queuedAnimation = null;
    private AnimationData? _previousAnimation = null;
    private bool _isPlayingOneShot = false;

    public event Action? AnimationCompleted;
    public event Action<string>? OnAnimationEvent; // For animation events like "attack_hit"
    
    /// <summary>
    /// Whether an animation is currently playing
    /// </summary>
    public bool IsPlaying => _isPlaying;
    
    /// <summary>
    /// Whether a one-shot animation is currently playing
    /// </summary>
    public bool IsPlayingOneShot => _isPlayingOneShot;
    
    /// <summary>
    /// Current frame index
    /// </summary>
    public int CurrentFrame => _currentFrame;

    public AnimatedSprite(Texture2D spriteSheet, int spriteWidth, int spriteHeight, float _timePerFrame = 200f, bool isLooping = false, bool isPlaying = true) : base(spriteSheet)
    {
        this._spriteWidth = spriteWidth;
        this._spriteHeight = spriteHeight;
        this._isLooping = isLooping;
        this._isPlaying = isPlaying;
        this._timePerFrame = _timePerFrame;
        SourceRectangle = GetSpriteFromSheet(0, 0);
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        UpdateAnimation(gameTime);
    }
    
    /// <summary>
    /// Play the current animation from the beginning
    /// </summary>
    public void Play()
    {
        _isPlaying = true;
        _currentFrame = 0;
        SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
    }
    
    /// <summary>
    /// Resume playing the current animation from where it stopped
    /// </summary>
    public void Resume()
    {
        _isPlaying = true;
    }
    
    /// <summary>
    /// Play an animation once and then return to the previous animation
    /// </summary>
    public void PlayOneShot(Texture2D texture, int column, float timePerFrame = 200f)
    {
        // Save current state
        _previousAnimation = new AnimationData(Texture, Column, _timePerFrame, _isLooping);
        
        // Set up one-shot animation
        Texture = texture;
        Column = column;
        _timePerFrame = timePerFrame;
        _isLooping = false;
        _isPlayingOneShot = true;
        
        Play();
    }
    
    /// <summary>
    /// Play an animation once using AnimationData and then return to the previous animation
    /// </summary>
    public void PlayOneShot(AnimationData animation)
    {
        PlayOneShot(animation.Texture, animation.Column, animation.TimePerFrame);
    }
    
    /// <summary>
    /// Queue an animation to play after the current one-shot completes
    /// </summary>
    public void QueueAnimation(AnimationData animation)
    {
        _queuedAnimation = animation;
    }
    
    /// <summary>
    /// Set the animation to play (will interrupt one-shots)
    /// </summary>
    public void SetAnimation(Texture2D texture, int column, float timePerFrame = 200f, bool isLooping = true)
    {
        _isPlayingOneShot = false;
        _previousAnimation = null;
        
        Texture = texture;
        Column = column;
        _timePerFrame = timePerFrame;
        _isLooping = isLooping;
        
        Play();
    }
    
    /// <summary>
    /// Set the animation using AnimationData
    /// </summary>
    public void SetAnimation(AnimationData animation)
    {
        SetAnimation(animation.Texture, animation.Column, animation.TimePerFrame, animation.IsLooping);
    }
    
    /// <summary>
    /// Set time per frame (animation speed)
    /// </summary>
    public void SetSpeed(float timePerFrame)
    {
        _timePerFrame = timePerFrame;
    }
    
    public void Reset()
    {
        _isPlaying = false;
        _currentFrame = 0;
        _isPlayingOneShot = false;
        _previousAnimation = null;
        SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
    }
    
    public void Stop()
    {
        _isPlaying = false;
    }
    
    public void setColumn(int column)
    {
        Column = column;
        _currentFrame = 0;
        SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
    }
    
    /// <summary>
    /// Jump to a specific frame
    /// </summary>
    public void SetFrame(int frame)
    {
        _currentFrame = Math.Clamp(frame, 0, _totalFrames - 1);
        SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
    }
    
    private void UpdateAnimation(GameTime gameTime)
    {
        if (!_isPlaying) return;

        _elapsedTimeSinceFrame += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_elapsedTimeSinceFrame >= _timePerFrame)
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
                    
                    // If this was a one-shot, return to previous animation
                    if (_isPlayingOneShot && _previousAnimation != null)
                    {
                        ReturnToPreviousAnimation();
                        return;
                    }
                    
                    // Check for queued animation
                    if (_queuedAnimation != null)
                    {
                        var queued = _queuedAnimation;
                        _queuedAnimation = null;
                        SetAnimation(queued);
                        return;
                    }
                    
                    _currentFrame = _totalFrames - 1; // Stay on last frame
                    Stop();
                }
            }
            SourceRectangle = GetSpriteFromSheet(Column, _currentFrame);
        }
    }
    
    private void ReturnToPreviousAnimation()
    {
        if (_previousAnimation == null) return;
        
        Texture = _previousAnimation.Texture;
        Column = _previousAnimation.Column;
        _timePerFrame = _previousAnimation.TimePerFrame;
        _isLooping = _previousAnimation.IsLooping;
        _isPlayingOneShot = false;
        _previousAnimation = null;
        
        Play();
    }
    
    public Rectangle GetSpriteFromSheet(int row, int column)
    {
        column = Math.Clamp(column, 0, Texture.Width / _spriteWidth - 1);
        return new Rectangle(
            this._spriteWidth * column,
            this._spriteHeight * row,
            this._spriteWidth,
            this._spriteHeight);
    }
}