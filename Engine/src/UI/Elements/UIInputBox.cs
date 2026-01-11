using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.UI.Elements;

public class UIInputBox : UIControl
{
    private string _text = "";
    private SpriteFont _font;

    private Color _textColor = Color.Black;
    private Color _backgroundColor = Color.White;
    private Color _borderColor = Color.Gray;
    private Color _focusedBorderColor = Color.Blue;
    
    private bool _isFocused = false;
    private float _cursorBlinkTimer = 0f;
    private bool _showCursor = true;
    private const float CURSOR_BLINK_INTERVAL = 0.5f;
    
    // Optional background visual
    private UIVisual? _background;
    
    // Text constraints
    public int MaxLength { get; set; } = 100;
    public string PlaceholderText { get; set; } = "";
    public Color PlaceholderColor { get; set; } = Color.Gray;
    
    // Events
    public event Action<string>? TextChanged;
    public event Action<string>? TextSubmitted;
    public event Action? Focused;
    public event Action? Unfocused;
    
    // Properties
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value?.Substring(0, Math.Min(value.Length, MaxLength)) ?? "";
                TextChanged?.Invoke(_text);
                MarkLayoutDirty();
            }
        }
    }
    
    public Color TextColor
    {
        get => _textColor;
        set => _textColor = value;
    }
    
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            if (_background != null)
                _background.Tint = value;
        }
    }
    
    public UIInputBox(SpriteFont font, UIVisual? background = null)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _background = background;
        _previousKeyboardState = Keyboard.GetState();
        
        if (_background != null)
        {
            _background.Tint = _backgroundColor;
            AddChild(_background);
        }
        
        // Subscribe to control events
        Pressed += OnPressed;
        
        // Set default size based on font
        DesiredSize = new Vector2(200, _font.LineSpacing + 8);
        MinSize = new Vector2(50, _font.LineSpacing + 8);
    }
    
    private void OnPressed(UIControl control)
    {
        SetFocused(true);
    }
    
    public void SetFocused(bool focused)
    {
        if (_isFocused == focused)
            return;
            
        _isFocused = focused;
        
        if (_isFocused)
        {
            _previousKeyboardState = Keyboard.GetState();
            _heldKeys.Clear();
            Focused?.Invoke();
        }
        else
        {
            _heldKeys.Clear();
            Unfocused?.Invoke();
        }
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        // Check if clicked outside to unfocus
        if (_isFocused && InputManager.Instance.IsActionPressed("UISelect"))
        {
            Rectangle slot = CalculateActualSlot();
            Vector2 size = new Vector2(slot.Width, slot.Height);
            Vector2 mousePos = ExtendedGame.GetMouseUIPosition();
            
            if (!(mousePos.X > GlobalPosition.X && mousePos.X < GlobalPosition.X + size.X 
                                                && mousePos.Y > GlobalPosition.Y && mousePos.Y < GlobalPosition.Y + size.Y))
            {
                SetFocused(false);
            }
        }
        
        if (!_isFocused)
            return;
        
        // Handle keyboard input
        HandleTextInput();
        
        // Handle cursor blinking
        _cursorBlinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_cursorBlinkTimer >= CURSOR_BLINK_INTERVAL)
        {
            _cursorBlinkTimer = 0f;
            _showCursor = !_showCursor;
        }
    }
    
    // Track previous keyboard state to detect new key presses
    private KeyboardState _previousKeyboardState;
    private Dictionary<Keys, double> _heldKeys = new Dictionary<Keys, double>();
    private const double KeyRepeatDelay = 0.5;
    private const double KeyRepeatRate = 0.03;
    
    private void HandleTextInput()
    {
        KeyboardState keyboardState = Keyboard.GetState();
        Keys[] pressedKeys = keyboardState.GetPressedKeys();
        Keys[] previousPressedKeys = _previousKeyboardState.GetPressedKeys();
        
        // Find keys that were just pressed (not held from previous frame)
        HashSet<Keys> previousKeysSet = new HashSet<Keys>(previousPressedKeys);
        HashSet<Keys> currentKeysSet = new HashSet<Keys>(pressedKeys);
        
        // Update held key timers and remove released keys
        List<Keys> keysToRemove = new List<Keys>();
        foreach (var kvp in _heldKeys)
        {
            if (!currentKeysSet.Contains(kvp.Key))
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            _heldKeys.Remove(key);
        }

        foreach (Keys key in pressedKeys)
        {
            bool shouldProcessKey = false;
            
            // Key was just pressed this frame
            if (!previousKeysSet.Contains(key))
            {
                _heldKeys[key] = 0;
                shouldProcessKey = true;
            }
            // Key is being held - check for repeat
            else if (_heldKeys.ContainsKey(key))
            {
                _heldKeys[key] += 0.016; // Approximate frame time
                
                if (_heldKeys[key] >= KeyRepeatDelay)
                {
                    double timeSinceDelay = _heldKeys[key] - KeyRepeatDelay;
                    double previousTimeSinceDelay = _heldKeys[key] - 0.016 - KeyRepeatDelay;
                    
                    // Check if we've crossed a repeat interval
                    if ((int)(timeSinceDelay / KeyRepeatRate) > (int)(previousTimeSinceDelay / KeyRepeatRate))
                    {
                        shouldProcessKey = true;
                    }
                }
            }
            
            if (shouldProcessKey)
            {
                ProcessKey(key, keyboardState);
            }
        }
        
        _previousKeyboardState = keyboardState;
    }
    
    private void ProcessKey(Keys key, KeyboardState keyboardState)
    {
        if (key == Keys.Back && _text.Length > 0)
        {
            Text = _text.Substring(0, _text.Length - 1);
            ResetCursorBlink();
        }
        else if (key == Keys.Enter)
        {
            TextSubmitted?.Invoke(_text);
            SetFocused(false);
        }
        else if (key == Keys.Space && _text.Length < MaxLength)
        {
            Text = _text + " ";
            ResetCursorBlink();
        }
        else if (_text.Length < MaxLength)
        {
            char? character = KeyToChar(key, keyboardState);
            if (character.HasValue)
            {
                Text = _text + character.Value;
                ResetCursorBlink();
            }
        }
    }

    private char? KeyToChar(Keys key, KeyboardState keyboardState)
    {
        bool shift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
        
        // Letters
        if (key >= Keys.A && key <= Keys.Z)
        {
            char c = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpper(c) : c;
        }
        
        // Numbers
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            if (shift)
            {
                return ")!@#$%^&*("[key - Keys.D0];
            }
            return (char)('0' + (key - Keys.D0));
        }
        
        // Special characters
        return key switch
        {
            Keys.OemPeriod => shift ? '>' : '.',
            Keys.OemComma => shift ? '<' : ',',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            Keys.OemOpenBrackets => shift ? '{' : '[',
            Keys.OemCloseBrackets => shift ? '}' : ']',
            Keys.OemPipe => shift ? '|' : '\\',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPlus => shift ? '+' : '=',
            _ => null
        };
    }

    private void ResetCursorBlink()
    {
        _cursorBlinkTimer = 0f;
        _showCursor = true;
    }
    
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        Rectangle actualSlot = CalculateActualSlot();
        Rectangle drawRect = new Rectangle(
            (int)GlobalPosition.X,
            (int)GlobalPosition.Y,
            actualSlot.Width,
            actualSlot.Height
        );
        
        // Draw background if no visual provided
        if (_background == null)
        {
            spriteBatch.Draw(
                EngineResources.BlankSquare,
                drawRect,
                null,
                _backgroundColor,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                DrawLayer - 0.001f
            );
        }
        
        // Draw border
        Color borderColor = _isFocused ? _focusedBorderColor : _borderColor;
        DrawBorder(spriteBatch, drawRect, borderColor, 2);
        
        // Draw text or placeholder
        Vector2 textPosition = new Vector2(
            GlobalPosition.X + Padding.X + 4,
            GlobalPosition.Y + Padding.Y + 4
        );
        
        if (string.IsNullOrEmpty(_text) && !_isFocused)
        {
            // Draw placeholder
            spriteBatch.DrawString(
                _font,
                PlaceholderText,
                textPosition,
                PlaceholderColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                DrawLayer
            );
        }
        else
        {
            // Draw actual text
            spriteBatch.DrawString(
                _font,
                _text,
                textPosition,
                _textColor,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                DrawLayer
            );
            
            // Draw cursor
            if (_isFocused && _showCursor)
            {
                Vector2 textSize = _font.MeasureString(_text);
                Vector2 cursorPosition = new Vector2(
                    textPosition.X + textSize.X,
                    textPosition.Y
                );
                
                Rectangle cursorRect = new Rectangle(
                    (int)cursorPosition.X,
                    (int)cursorPosition.Y,
                    2,
                    _font.LineSpacing
                );
                
                spriteBatch.Draw(
                    EngineResources.BlankSquare,
                    cursorRect,
                    null,
                    _textColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    DrawLayer
                );
            }
        }
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(EngineResources.BlankSquare, 
            new Rectangle(rect.X, rect.Y, rect.Width, thickness), 
            null, color, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        
        // Bottom
        spriteBatch.Draw(EngineResources.BlankSquare, 
            new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), 
            null, color, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        
        // Left
        spriteBatch.Draw(EngineResources.BlankSquare, 
            new Rectangle(rect.X, rect.Y, thickness, rect.Height), 
            null, color, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
        
        // Right
        spriteBatch.Draw(EngineResources.BlankSquare, 
            new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), 
            null, color, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
    }
}