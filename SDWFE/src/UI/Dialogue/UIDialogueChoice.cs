
using System;
using System.Collections.Generic;
using Engine;
using Engine.Dialogue;
using Engine.Input;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.UI.Dialogue;
#nullable enable
public class UIDialogueChoice : UIControl
{
    // Root and resources
    private UIRoot _root = null!;
    private readonly Texture2D _spriteSheetTexture;
    private readonly SpriteFont _font;
    private readonly Texture2D? _profileSprite;
    private readonly Rectangle _smallWhiteBgRect;

    // Main layout containers
    private UIHBoxContainer _mainHbox = null!;
    private UIVBoxContainer _textAndOptionsVbox = null!;
    private UIHBoxContainer _choicesHbox = null!;

    // Speaker name elements
    private UIVisual _speakerNameBackground = null!;
    private UIVisual _speakerNameText = null!;

    // Text progression elements
    private UIVisual _textProgressionBg = null!;
    private UITextProgression _dialogueText = null!;

    // Choice buttons
    private List<UIControl> _choiceButtons = new List<UIControl>();

    // Current dialogue state
    private DialogueNode _currentNode = null!;
    private bool _isOpen;

    /// <summary>
    /// Whether the dialogue is currently open.
    /// </summary>
    public bool IsOpen => _isOpen;

    public event Action? OnDialogueClosed;

    public UIDialogueChoice(Texture2D? profileSprite = null)
    {
        DialogueRegistry.Load(ExtendedGame.AssetManager.GetContentDirectory() + "/Levels/Dialogue.json");

        _profileSprite = profileSprite;
        _spriteSheetTexture = ExtendedGame.AssetManager.LoadTexture("UI_Dialogue", "UI/");
        _smallWhiteBgRect = new Rectangle(new Point(0, 0), new Point(32, 32));
        _font = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 8);

        this.DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);
        Released += OnClicked;

        BuildDialogueUI();
        // Start hidden
        _isOpen = false;
        IsVisible = false;

    }
    /// <summary>
    /// Shows the dialogue with the specified dialogue node.
    /// </summary>
    public void Show(DialogueNode startNode)
    {
        UpdateDialogueNode(startNode);
        _isOpen = true;
        IsVisible = true;
        
        // Switch to UI input profile for proper click handling
        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);
    }

    /// <summary>
    /// Shows the dialogue starting from the specified node ID.
    /// </summary>
    public void Show(string nodeId)
    {
        var node = DialogueRegistry.GetNode(nodeId);
        if (node != null)
        {
            Show(node);
        }
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
    /// Builds the dialogue UI structure once. Called only in constructor.
    /// </summary>
    private void BuildDialogueUI()
    {
        _root = new UIRoot();
        _root.SetRootRect(new Rectangle(Vector2.Zero.ToPoint(), UIExtensionMethods.ScreenPercent(100, 100).ToPoint()));
        _root.Margin = new Vector4(20, 20, 20, 20);
        this.AddChild(_root);

        // Main horizontal container
        _mainHbox = new UIHBoxContainer();
        _mainHbox.DesiredSize = new Vector2(UIExtensionMethods.GetScreenPercentageWidth(80), 96);
        _mainHbox.AlignmentPoint = Alignment.BottomMiddle;
        _root.AddChild(_mainHbox);

        // Vbox for text progression and options
        _textAndOptionsVbox = new UIVBoxContainer();
        _mainHbox.AddChild(_textAndOptionsVbox);

        // Create speaker name background
        Rectangle sourceRect = new Rectangle(new Point(32, 0), new Point(32, 32));
        _speakerNameBackground = UIVisual.FromStretchableTexture(_spriteSheetTexture, new Vector4(6, 6, 6, 6), sourceRect);
        _speakerNameBackground.DesiredSize = new Vector2(UIExtensionMethods.GetScreenPercentageWidth(40), 32);

        _speakerNameText = UIVisual.FromText(" ", _font, Color.White);
        _speakerNameText.Margin = new Vector4(10, 10, 10, 10);
        _speakerNameText.AlignmentPoint = Alignment.MiddleCenter;
        _speakerNameBackground.AddChild(_speakerNameText);
        _textAndOptionsVbox.AddChild(_speakerNameBackground);

        // Create text progression background
        _textProgressionBg = UIVisual.FromStretchableTexture(_spriteSheetTexture, new Vector4(6, 6, 6, 6), _smallWhiteBgRect);
        _textProgressionBg.MinSize = new Vector2(UIExtensionMethods.GetScreenPercentageWidth(80), 64);
        _textAndOptionsVbox.AddChild(_textProgressionBg);

        // Create text progression element (will be updated later)
        _dialogueText = new UITextProgression(" ", _font);
        _dialogueText.Margin = new Vector4(15, 15, 15, 20);
        _textProgressionBg.AddChild(_dialogueText);

        // Choices horizontal container
        _choicesHbox = new UIHBoxContainer();
        _choicesHbox.DesiredSize = new Vector2(
            UIExtensionMethods.GetScreenPercentageWidth(100),
            32
        );
        _textAndOptionsVbox.AddChild(_choicesHbox);
    }

    /// <summary>
    /// Updates the dialogue to show a new DialogueNode without rebuilding the entire UI.
    /// </summary>
    public void UpdateDialogueNode(DialogueNode newNode)
    {
        _currentNode = newNode;

        // Update speaker name
        _speakerNameText.SetText(newNode.SpeakerName, _font, Color.White);

        // Update dialogue text
        _dialogueText.SetText(newNode.Text);

        // Clear old choice buttons
        ClearChoiceButtons();
        _textAndOptionsVbox.RemoveChild(_choicesHbox);
        _choicesHbox = new UIHBoxContainer();
        _choicesHbox.DesiredSize = new Vector2(
            UIExtensionMethods.GetScreenPercentageWidth(100),
            32
        );
        // Create new choice buttons
        for (int i = 0; i < newNode.Choices.Count; i++)
        {
            var choice = newNode.Choices[i];
            UIControl buttonChoice = CreateChoiceButton(choice.ChoiceText, choice.NextNode!);
            buttonChoice.DesiredSize = new Vector2(
                UIExtensionMethods.GetScreenPercentageWidth(40),
                32
            );
            //_choicesHbox.AddChild(buttonChoice);
        }
        foreach (var button in _choiceButtons)
        {
            _choicesHbox.AddChild(button);
        }
        _textAndOptionsVbox.AddChild(_choicesHbox);
    }

    /// <summary>
    /// Clears all choice buttons from the UI.
    /// </summary>
    private void ClearChoiceButtons()
    {
        foreach (var button in _choiceButtons)
        {
            _choicesHbox.RemoveChild(button);
        }
        _choiceButtons.Clear();
    }

    private void OnClicked(UIControl control)
    {
        if (!_isOpen)
            return;

        // If text is still animating, reveal it fully first
        if (!_dialogueText.IsPageFullyRevealed)
        {
            _dialogueText.RevealCurrentPage();
            return;
        }

        if (_dialogueText.IsOnLastPage && _currentNode.Choices.Count == 0)
        {
            Hide();
            OnDialogueClosed?.Invoke();
            return;
        }

        if (_dialogueText.IsOnLastPage)
        {
            RevealChoices();
        }

        // Otherwise advance to next page
        _dialogueText.NextPage();
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

    public void RevealChoices()
    {
        foreach (var button in _choiceButtons)
        {
            button.IsVisible = true;
        }
    }

    private UIControl CreateChoiceButton(string buttonText, DialogueNode nextNode)
    {
        UIControl button = new UIControl();

        // add button background
        UIVisual buttonBg = UIVisual.FromStretchableTexture(_spriteSheetTexture, new Vector4(6, 6, 6, 6), _smallWhiteBgRect);
        buttonBg.DesiredSize = new Vector2(
            UIExtensionMethods.GetScreenPercentageWidth(100),
            32
        );
        button.AddChild(buttonBg);

        // add button text
        UIVisual buttonTextVisual = UIVisual.FromText(buttonText, _font, Color.White);
        buttonTextVisual.AlignmentPoint = Alignment.MiddleCenter;
        buttonBg.AddChild(buttonTextVisual);

        // add click event
        button.Released += (UIControl ctrl) =>
        {
            UpdateDialogueNode(nextNode);
        };

        button.HoverEntered += (UIControl ctrl) =>
        {
            // Change button appearance on hover
            buttonBg.Tint = new Color(200, 200, 255);
        };

        button.HoverExited += (UIControl ctrl) =>
        {
            // Revert button appearance when not hovered
            buttonBg.Tint = Color.White;
        };

        button.IsVisible = false;
        _choiceButtons.Add(button);
        return button;
    }
}