
using Engine.Input;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Engine.UI;
#nullable enable
public class UIProgressbar : UIControl
{
    public float Min { get; set; } = 0f;
    public float Max { get; set; } = 500f;
    
    private float _value;
    public float Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, Min, Max);
            MarkLayoutDirty();
        }
    }
    
    private float _targetPercentage = 1f;
    private float _displayedPercentage = 1f;
    private UIVisual? _background;
    private UIVisual _fill;
    
    private int _scale = 1;
    /// <summary>
    /// Animation speed - higher = faster. Set to 0 for instant.
    /// </summary>
    public float AnimationSpeed { get; set; } = 5f;

    public UIProgressbar(UIVisual fill, UIVisual? background, int scale = 1)
    {
        _scale = scale;
        _fill = fill;
        _background = background;

        Value = 0f;
        if (_background != null)
        {
            _background.SetDesiredSize();
            this.AddChild(_background);
        }
        _fill.SetDesiredSize();
        this.AddChild(_fill);

        MarkLayoutDirty();
    }

    public void SetProgress(float value)
    {
        _targetPercentage = MathHelper.Clamp(value, 0f, 1f);
    }
    
    /// <summary>
    /// Sets progress instantly without animation
    /// </summary>
    public void SetProgressInstant(float value)
    {
        _targetPercentage = MathHelper.Clamp(value, 0f, 1f);
        _displayedPercentage = _targetPercentage;
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Smoothly interpolate towards target
        if (AnimationSpeed <= 0f)
        {
            _displayedPercentage = _targetPercentage;
        }
        else
        {
            float diff = _targetPercentage - _displayedPercentage;
            if (Math.Abs(diff) < 0.001f)
            {
                _displayedPercentage = _targetPercentage;
            }
            else
            {
                _displayedPercentage += diff * AnimationSpeed * deltaTime;
            }
        }

        Rectangle? originalRect = _fill.OriginalSourceRect;
        if (originalRect == null) return;

        float width = originalRect.Value.Width;
        int newWidth = (int)(width * _displayedPercentage);
        
        if (newWidth <= 0)
        {
            // Hide by making fully transparent instead of using IsVisible
            _fill.Tint = Color.Transparent;
        }
        else
        {
            _fill.Tint = Color.White;
            Rectangle sourceRect = new Rectangle(originalRect.Value.X, originalRect.Value.Y, newWidth, originalRect.Value.Height);
            _fill.SourceRect = sourceRect;
            _fill.DesiredSize = new Vector2(newWidth * _scale, sourceRect.Height * _scale);
        }
    }

}