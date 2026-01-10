using System;
using Engine;
using Engine.Input;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.UI.Dialogue;

/// <summary>
/// Full-screen dialogue display with animated text progression.
/// Shows a semi-transparent background with a text box containing typewriter-style text.
/// </summary>
public class UIDialogue : UIControl
{
    private UIVisual _backgroundOverlay;
    private UIContainer _dialogueBox;
    private UIVisual _dialogueBoxBg;
    private UITextProgression _textProgression;
    private UIRoot _uiRoot;
    
    private bool _isOpen;
    private SpriteFont _font;
    
    /// <summary>
    /// Whether the dialogue is currently open.
    /// </summary>
    public bool IsOpen => _isOpen;
    
    /// <summary>
    /// Event fired when the dialogue is closed (after last page is completed).
    /// </summary>
    public event Action? OnDialogueClosed;
    
    /// <summary>
    /// Event fired when a new page is shown.
    /// </summary>
    public event Action? OnPageChanged;

    public UIDialogue()
    {
        // Full screen size
        AlignmentPoint = Alignment.TopLeft;
        _uiRoot = new UIRoot();
        // Load font
        _font = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 16);
        
        SetupUI();
        
        // Subscribe to click events from UIControl
        Released += OnClicked;
        
        // Start hidden - must be AFTER SetupUI so children inherit visibility
        _isOpen = false;
        _uiRoot.IsVisible = false;
    }

    private void SetupUI()
    {
        _uiRoot.SetRootRect(new Rectangle(0, 0, ExtendedGame.DrawResolution.X, ExtendedGame.DrawResolution.Y));
        
        // Semi-transparent dark background overlay
        _backgroundOverlay = UIVisual.FromColor(new Color(0, 0, 0, 180));
        _backgroundOverlay.DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);
        _backgroundOverlay.AlignmentPoint = Alignment.TopLeft;
        _uiRoot.AddChild(_backgroundOverlay);
        
        // Dialogue box container - positioned at bottom of screen
        _dialogueBox = new UIContainer();
        _dialogueBox.DesiredSize = new Vector2(
            UIExtensionMethods.GetScreenPercentageWidth(80),
            UIExtensionMethods.GetScreenPercentageHeight(80)
        );
        _dialogueBox.AlignmentPoint = Alignment.BottomMiddle;
        _dialogueBox.Margin = new Vector4(0, 0, 0, 20); // Bottom margin
        _uiRoot.AddChild(_dialogueBox);
        
        // Dialogue box background (using engine's blank square with color)
        _dialogueBoxBg = UIVisual.FromColor(new Color(30, 30, 40, 240));
        _dialogueBoxBg.DesiredSize = _dialogueBox.DesiredSize;
        _dialogueBoxBg.AlignmentPoint = Alignment.TopLeft;
        _dialogueBox.AddChild(_dialogueBoxBg);
        
        // Text progression element - disable its own click handling
        _textProgression = new UITextProgression("", _font);
        _textProgression.DesiredSize = new Vector2(
            _dialogueBox.DesiredSize.X - 40, // Padding from box edges
            _dialogueBox.DesiredSize.Y - 30
        );
        _textProgression.Margin = new Vector4(20, 15, 20, 15);
        _textProgression.AlignmentPoint = Alignment.TopLeft;
        _textProgression.TextColor = Color.White;
        _textProgression.SecondsPerCharacter = 0.03f;
        _textProgression.AutoAdvanceOnClick = false; // We handle clicks ourselves
        
        // Subscribe to text progression events
        _textProgression.OnTextCompleted += HandleTextCompleted;
        _textProgression.OnPageChanged += () => OnPageChanged?.Invoke();
        
        _dialogueBox.AddChild(_textProgression);

        AddChild(_uiRoot);
    }
    
    private void OnClicked(UIControl control)
    {
        if (!_isOpen)
            return;
            
        // If text is still animating, reveal it fully first
        if (!_textProgression.IsPageFullyRevealed)
        {
            _textProgression.RevealCurrentPage();
            return;
        }

        // If on last page and fully revealed, close dialogue
        if (_textProgression.IsOnLastPage)
        {
            Hide();
            OnDialogueClosed?.Invoke();
            return;
        }

        // Otherwise advance to next page
        _textProgression.NextPage();
    }

    /// <summary>
    /// Shows the dialogue with the specified text.
    /// Use "|p" in the text to manually split pages.
    /// </summary>
    /// <param name="text">The text to display. Use "|p" for manual page breaks.</param>
    public void Show(string text)
    {
        _textProgression.SetText(text);
        _isOpen = true;
        IsVisible = true;
        
        // Switch to UI input profile for proper click handling
        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);
    }

    /// <summary>
    /// Hides the dialogue.
    /// </summary>
    public void Hide()
    {
        _isOpen = false;
        IsVisible = false;
        
        // Switch back to gameplay input profile
        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_GAMEPLAY);
    }

    /// <summary>
    /// Toggles the dialogue visibility. If showing, will hide. If hidden, does nothing
    /// (use Show() to display with text).
    /// </summary>
    public void Toggle()
    {
        if (_isOpen)
        {
            Hide();
        }
    }

    private void HandleTextCompleted()
    {
        Hide();
        OnDialogueClosed?.Invoke();
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        if (!_isOpen)
            return;
        
        // Allow closing with cancel/escape
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_CANCEL))
        {
            Hide();
            OnDialogueClosed?.Invoke();
        }
    }
}
