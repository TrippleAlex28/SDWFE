using System;
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

namespace SDWFE.Scenes;

public class MainMenuScene : Scene
{
    public const string KEY = "MainMenuScene";

    private bool _isLoading = false;

    private readonly Texture2D _ChineseUITexture;
    
    public MainMenuScene() : base(KEY)
    {
        BackgroundColor = new Color(0xEA, 0xD2, 0xAD);
        _ChineseUITexture = ExtendedGame.AssetManager.LoadTexture("UI_Chinese", "UI/");
        SetDefaultPlayerClassNull(); // useless default player
    }

    public override void Enter()
    {
        base.Enter();

        // Switch input profile back to UI for menu navigation
        InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);

        CreateMainMenuUI();
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

    private async void OnJoinClicked()
    {
        if (_isLoading) return;
        
        // Create multiplayer client session
        _isLoading = true;
        GameState.Instance.SwitchSession(SessionType.MultiplayerClient);
        if (!await ((MultiplayerClientSession)GameState.Instance.SessionManager.CurrentSession!).ConnectAsync(
                "192.168.68.125"))
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

    private void CreateMainMenuUI()
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
        var titleText = UIVisual.FromText("Song Dynasty", Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 24), Color.White);
        titleText.AlignmentPoint = Alignment.MiddleCenter;
        titleText.DesiredSize = UIExtensionMethods.ScreenPercent(100, 20);
        mainHBox.AddChild(titleText);

        // Title subtitle text
        var subtitleText = UIVisual.FromText(ExtendedGame.GAME_NAME_SUBTITLE, Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 8), Color.White);
        subtitleText.AlignmentPoint = Alignment.TopMiddle;
        subtitleText.DesiredSize = UIExtensionMethods.ScreenPercent(100, 10);
        mainHBox.AddChild(subtitleText);

        // Center container for play buttons
        var playButtonsContainer = new UIHBoxContainer();
        playButtonsContainer.DesiredSize = UIExtensionMethods.ScreenPercent(100, 50);
        playButtonsContainer.AlignmentPoint = Alignment.MiddleCenter;
        mainHBox.AddChild(playButtonsContainer);

        // Play Button (left)
        var playButton = CreateButton("PLAY", new Rectangle(128, 0, 128, 128));
        playButton.AlignmentPoint = Alignment.MiddleCenter;
        playButton.Released += (control) => OnPlayClicked();
        playButtonsContainer.AddChild(playButton);

        // Host Button (center)
        var hostButton = CreateButton("HOST", new Rectangle(128, 0, 128, 128));
        hostButton.AlignmentPoint = Alignment.MiddleLeft;
        hostButton.Released += (control) => OnHostClicked();
        playButtonsContainer.AddChild(hostButton);

        // Join Button (right)
        var joinButton = CreateButton("JOIN", new Rectangle(128, 0, 128, 128));
        joinButton.AlignmentPoint = Alignment.MiddleCenter;
        joinButton.Released += (control) => OnJoinClicked();
        playButtonsContainer.AddChild(joinButton);

        // Bottom container for settings and quit
        var bottomContainer = new UIHBoxContainer();
        bottomContainer.DesiredSize = UIExtensionMethods.ScreenPercent(100, 20);
        bottomContainer.AlignmentPoint = Alignment.MiddleCenter;
        mainHBox.AddChild(bottomContainer);

        // Settings Button (left)
        var settingsButton = CreateButton("SETTINGS", new Rectangle(256, 72, 64, 16), new Vector4(7, 7, 7, 7), fontSize: 12);
        settingsButton.AlignmentPoint = Alignment.MiddleLeft;
        settingsButton.MaxSize = UIExtensionMethods.ScreenPercent(25, 100);
        settingsButton.Released += (control) => OnSettingsClicked();
        bottomContainer.AddChild(settingsButton);

        // filling
        var filler = new UIElement();
        bottomContainer.AddChild(filler);
        // Quit Button (right)
        var quitButton = CreateButton("QUIT", new Rectangle(256, 104, 64, 16), new Vector4(7, 7, 7, 7), fontSize: 12);
        quitButton.AlignmentPoint = Alignment.MiddleRight;
        quitButton.MaxSize = UIExtensionMethods.ScreenPercent(25, 100);
        quitButton.Released += (control) => OnQuitClicked();
        bottomContainer.AddChild(quitButton);
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