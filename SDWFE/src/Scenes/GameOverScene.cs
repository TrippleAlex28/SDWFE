using System;
using System.Numerics;
using Engine;
using Engine.Input;
using Engine.Network.Shared.Session;
using Engine.Network.Shared.Session.Sessions;
using Engine.Particle;
using Engine.Scene;
using Engine.Settings;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace SDWFE.Scenes;

public class GameOverScene : Scene
{
    public const string KEY = "GameOverScene";

    private string _desiredIp;
    
    private bool _isLoading = false;

    private readonly Texture2D _ChineseUITexture;
    
    public GameOverScene() : base(KEY)
    {
        BackgroundColor = new Color(0xEA, 0xD2, 0xAD);
        _ChineseUITexture = ExtendedGame.AssetManager.LoadTexture("UI_Chinese", "UI/");
        SetDefaultPlayerClassNull(); // useless default player
    }

    public override void Enter()
    {
        base.Enter();
        ExtendedGame.LightShaderInstance.Enabled = false;
        // Switch input profile back to UI for menu navigation
        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);

        CreateGameOverUI();
    }

    #region Handlers
    
    private void OnPlayClicked()
    {
        if (_isLoading) return;
        
        // Create singleplayer session
        _isLoading = true;
        GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, GameplayScene.KEY);
        _isLoading = false;
    }

    private void OnHostClicked()
    {
        if (_isLoading) return;
        
        // Create multiplayer host session
        _isLoading = true;
        GameState.Instance.SwitchSessionAndScene(SessionType.MultiplayerHost, GameplayScene.KEY);
        _isLoading = false;
    }

    private void OnJoinInputTextChanged(string text)
    {
        _desiredIp = text;
    }
    
    private async void OnJoinClicked()
    {
        if (_isLoading) return;
        
        // Create multiplayer client session
        _isLoading = true;
        GameState.Instance.SwitchSession(SessionType.MultiplayerClient);
        if (!await ((MultiplayerClientSession)GameState.Instance.SessionManager.CurrentSession!).ConnectAsync(
                _desiredIp))
        {
            // Re-enter this scene as SinglePlayer
            GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, KEY);
        }
        _isLoading = false;
    }

    private void OnSettingsClicked()
    {
        Console.WriteLine("Settings button clicked!");
    }

    private void OnQuitClicked()
    {
        // SDWFE.Instance.Exit();
    }
    
    #endregion 
    
    #region Helpers

    private void CreateGameOverUI()
    {
        // Clear any existing UI
        UIRoot.Children.Clear();

        // HBOX Layout
        var mainHBox = new UIVBoxContainer();
        mainHBox.DesiredSize = ExtendedGame.DrawResolution.ToVector2();
        mainHBox.AlignmentPoint = Alignment.TopLeft;
        UIRoot.AddChild(mainHBox);

        // Title at top
        var titleContainer = new UIElement();
        titleContainer.MaxSize = UIExtensionMethods.ScreenPercent(100, 10);
        titleContainer.AlignmentPoint = Alignment.TopLeft;
        mainHBox.AddChild(titleContainer);


        // Title text
        var titleText = UIVisual.FromText("Game Over", Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 24), Color.White);
        titleText.AlignmentPoint = Alignment.MiddleCenter;
        titleText.DesiredSize = UIExtensionMethods.ScreenPercent(100, 30);
        mainHBox.AddChild(titleText);


        // Center container for play buttons
        var playButtonsContainer = new UIHBoxContainer();
        playButtonsContainer.DesiredSize = UIExtensionMethods.ScreenPercent(100, 50);
        playButtonsContainer.AlignmentPoint = Alignment.MiddleCenter;
        mainHBox.AddChild(playButtonsContainer);

        // Play Button (left)
        var playButton = CreateButton("Restart", new Rectangle(128, 0, 128, 128));
        playButton.AlignmentPoint = Alignment.MiddleCenter;
        playButton.Released += (control) => OnPlayClicked();
        playButtonsContainer.AddChild(playButton);

        // Host Button (center)
        var hostButton = CreateButton("Main Menu", new Rectangle(128, 0, 128, 128));
        hostButton.AlignmentPoint = Alignment.MiddleLeft;
        hostButton.Released += (control) => OnHostClicked();
        playButtonsContainer.AddChild(hostButton);

    }
    
    private UIControl CreateButton(string text, Rectangle? sourceRect = null, Vector4? sliceRect = null, int fontSize = 24)
    {
        var button = new UIControl();
        button.Padding = new Vector4(8, 8, 8, 8);

        Vector4 slice = sliceRect ?? new Vector4(32, 32, 32, 32);
        // Button background - default state
        var buttonBg = UIVisual.FromStretchableTexture(_ChineseUITexture, slice, sourceRect);
        buttonBg.AlignmentPoint = Alignment.MiddleCenter;
        buttonBg.DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);
        button.AddChild(buttonBg);

        // Button text
        var buttonText = UIVisual.FromText(text, Resources.GetFont(Resources.UPHEAVEL_FONTNAME, fontSize), Color.White);
        buttonText.DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);

        buttonText.AlignmentPoint = Alignment.MiddleCenter;
        buttonBg.AddChild(buttonText);

        // Hover effects
        button.HoverEntered += (control) =>
        {
            buttonBg.Tint = new Color(80, 80, 120);
            buttonText.Tint = new Color(80, 80, 120);
        };

        button.HoverExited += (control) =>
        {
            buttonBg.Tint = Color.White;
            buttonText.Tint = Color.White;
        };

        button.Pressed += (control) =>
        {
            buttonBg.Tint = new Color(40, 40, 60);
        };

        return button;
    }
    
    #endregion 
}