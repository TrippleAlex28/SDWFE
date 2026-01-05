using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine;

public class FollowCamera
{
    /// <summary>
    /// The desired camera zoom factor
    /// </summary>
    public float TargetZoom
    {
        get => _targetZoom;
        set => _targetZoom = Math.Clamp(value, 0.5f, 2.0f);
    }
    
    /// <summary>
    /// The smoothness at which the camera tracks the target position
    /// </summary>
    public float Smoothness
    {
        get => _smoothness;
        set => _smoothness = Math.Clamp(value, 0f, 1f);
    }
    
    /// Position management parameters
    private Vector2 _targetPosition = Vector2.Zero;
    private Vector2 _position = Vector2.Zero;
    
    // Zoom management parameters
    private float _targetZoom = 1f;
    private float _zoom = 1f;

    // Smoothness management parameters
    private float _smoothness = 0.35f;

    /// Camera shake management parameters
    private float _shakeDuration = 0f;
    private float _shakeTimer = 0f;
    private float _shakeMagnitude = 0f;
    private Vector2 _shakeOffset = Vector2.Zero;

    private readonly Viewport _viewport;

    public FollowCamera(Viewport viewport)
    {
        _viewport = viewport;
    }

    public void Follow(Vector2 target)
    {
        _targetPosition = target;
    }

    public void Shake(float magnitude, float duration)
    {
        _shakeMagnitude = magnitude;
        _shakeDuration = duration;
        _shakeTimer = 0f;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = gameTime.DeltaSeconds();
        
        // Smooth position follow
        _position = Vector2.Lerp(_position, _targetPosition, _smoothness);
        
        // Smooth zoom
        _zoom = MathHelper.Lerp(_zoom, _targetZoom, _smoothness);
        
        // Camera shake
        if (_shakeTimer < _shakeDuration)
        {
            _shakeTimer += deltaTime;

            // Linear shake magnitude falloff
            float progress = _shakeTimer / _shakeDuration;
            float currentMagnitude = _shakeMagnitude * (1f - progress);

            _shakeOffset.X = (ExtendedGame.Random.NextSingle() * 2 - 1) * currentMagnitude;
            _shakeOffset.Y = (ExtendedGame.Random.NextSingle() * 2 - 1) * currentMagnitude;
        }
        else
        {
            _shakeOffset = Vector2.Zero;
        }
    }

    /// <summary>
    /// Get the transform matrix that should be used to draw the level
    /// </summary>
    /// <returns></returns>
    public Matrix GetTransformMatrix()
    {
        // Get and snap camera position
        Vector2 camPos = _position + _shakeOffset;
        // camPos = new Vector2(MathF.Round(camPos.X), MathF.Round(camPos.Y)); // snapping currently makes the camera tweak, I'm suspecting because of the change from Vector2 tracking coordinates to a Point position 
        
        var origin = new Vector2(_viewport.Width / 2f, _viewport.Height / 2f);

        return Matrix.CreateTranslation(-camPos.X, -camPos.Y, 0) *
               Matrix.CreateTranslation(origin.X, origin.Y, 0) *
               Matrix.CreateScale(_zoom, _zoom, 1);
    }
    
    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        Matrix inv = Matrix.Invert(GetTransformMatrix());
        return Vector2.Transform(screenPos, inv);
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos, GetTransformMatrix());
    }
}