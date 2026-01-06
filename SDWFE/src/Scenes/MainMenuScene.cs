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
    
    public MainMenuScene() : base(KEY)
    {
        BackgroundColor = Color.Orange;
        SetDefaultPlayerClassNull(); // useless default player
    }

    public override void Enter()
    {
        base.Enter();

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

        // Title at top
        var titleContainer = new UIContainer();
        titleContainer.DesiredSize = new Vector2(600, 80);
        titleContainer.AlignmentPoint = Alignment.TopMiddle;
        titleContainer.Margin = new Vector4(0, 40, 0, 0);
        UIRoot.AddChild(titleContainer);

        // Title background
        var titleBg = UIVisual.FromColor(new Color(40, 40, 60));
        titleBg.DesiredSize = new Vector2(600, 80);
        titleBg.AlignmentPoint = Alignment.TopLeft;
        titleContainer.AddChild(titleBg);

        // Title text
        var titleText = UIVisual.FromText(ExtendedGame.GAME_NAME, Resources.TitleFont, Color.White);
        titleText.AlignmentPoint = Alignment.MiddleCenter;
        titleContainer.AddChild(titleText);

        // Center container for play buttons
        var playButtonsContainer = new UIContainer();
        playButtonsContainer.DesiredSize = new Vector2(600, 80);
        playButtonsContainer.AlignmentPoint = Alignment.MiddleCenter;
        UIRoot.AddChild(playButtonsContainer);

        // Play Button (left)
        var playButton = CreateButton("PLAY", new Vector2(180, 70));
        playButton.AlignmentPoint = Alignment.MiddleLeft;
        playButton.Margin = new Vector4(10, 5, 10, 5);
        playButton.Released += (control) => OnPlayClicked();
        playButtonsContainer.AddChild(playButton);

        // Host Button (center)
        var hostButton = CreateButton("HOST", new Vector2(180, 70));
        hostButton.AlignmentPoint = Alignment.MiddleCenter;
        hostButton.Margin = new Vector4(10, 5, 10, 5);
        hostButton.Released += (control) => OnHostClicked();
        playButtonsContainer.AddChild(hostButton);

        // Join Button (right)
        var joinButton = CreateButton("JOIN", new Vector2(180, 70));
        joinButton.AlignmentPoint = Alignment.MiddleRight;
        joinButton.Margin = new Vector4(10, 5, 10, 5);
        joinButton.Released += (control) => OnJoinClicked();
        playButtonsContainer.AddChild(joinButton);

        // Bottom container for settings and quit
        var bottomContainer = new UIContainer();
        bottomContainer.DesiredSize = new Vector2(500, 70);
        bottomContainer.AlignmentPoint = Alignment.BottomMiddle;
        bottomContainer.Margin = new Vector4(0, 0, 0, 40);
        UIRoot.AddChild(bottomContainer);

        // Settings Button (left)
        var settingsButton = CreateButton("SETTINGS", new Vector2(220, 60));
        settingsButton.AlignmentPoint = Alignment.MiddleLeft;
        settingsButton.Margin = new Vector4(10, 5, 10, 5);
        settingsButton.Released += (control) => OnSettingsClicked();
        bottomContainer.AddChild(settingsButton);

        // Quit Button (right)
        var quitButton = CreateButton("QUIT", new Vector2(220, 60));
        quitButton.AlignmentPoint = Alignment.MiddleRight;
        quitButton.Margin = new Vector4(10, 5, 10, 5);
        quitButton.Released += (control) => OnQuitClicked();
        bottomContainer.AddChild(quitButton);
    }
    
    private UIControl CreateButton(string text, Vector2 size)
    {
        var button = new UIControl();
        button.DesiredSize = size;

        // Button background - default state
        var buttonBg = UIVisual.FromColor(new Color(60, 60, 80));
        buttonBg.DesiredSize = size;
        buttonBg.AlignmentPoint = Alignment.TopLeft;
        button.AddChild(buttonBg);

        // Button text
        var buttonText = UIVisual.FromText(text, Resources.TextFont, Color.White);
        buttonText.AlignmentPoint = Alignment.MiddleCenter;
        button.AddChild(buttonText);

        // Hover effects
        button.HoverEntered += (control) =>
        {
            buttonBg.Tint = new Color(80, 80, 120);
        };

        button.HoverExited += (control) =>
        {
            buttonBg.Tint = new Color(60, 60, 80);
        };

        button.Pressed += (control) =>
        {
            buttonBg.Tint = new Color(40, 40, 60);
        };

        return button;
    }
    
    #endregion 
}