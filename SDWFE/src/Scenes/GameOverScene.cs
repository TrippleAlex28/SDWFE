using Engine;
using Engine.Input;
using Engine.Network.Shared.Session;
using Engine.Scene;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE;
using SDWFE.Scenes;
using SDWFE.Scenes.Levels;

public class GameOverScene : Scene
{
    public const string KEY = "GameOverScene";
    
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

    private void CreateGameOverUI()
    {
        UIRoot.Children.Clear();

        // HBOX layout
        UIVBoxContainer hbox = new UIVBoxContainer();
        hbox.DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);
        UIRoot.AddChild(hbox);


        // Title text
        var titleText = UIVisual.FromText("Game Over", Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 24), Color.White);
        titleText.AlignmentPoint = Alignment.MiddleCenter;
        titleText.DesiredSize = UIExtensionMethods.ScreenPercent(100, 20);
        hbox.AddChild(titleText);

        // Restart button
        var restartButton = CreateButton("Restart", new Rectangle(256, 72, 64, 16), new Vector4(7, 7, 7, 7), fontSize: 12);
        restartButton.AlignmentPoint = Alignment.MiddleCenter;
        restartButton.MaxSize = UIExtensionMethods.ScreenPercent(60, 100);
        restartButton.Released += (control) => OnRestartClicked();
        hbox.AddChild(restartButton);

        // Restart button
        var MainMenuButton = CreateButton("Main Menu", new Rectangle(256, 72, 64, 16), new Vector4(7, 7, 7, 7), fontSize: 12);
        MainMenuButton.AlignmentPoint = Alignment.MiddleCenter;
        MainMenuButton.MaxSize = UIExtensionMethods.ScreenPercent(60, 100);
        MainMenuButton.Released += (control) => OnMainMenuClicked();
        hbox.AddChild(MainMenuButton);
    
    }
    private void OnMainMenuClicked()
    {
        // Go back to main menu
        GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
    }
    private void OnRestartClicked()
    {
        // Restart the game by switching to the main menu
        SceneData.levelIndex = -1;
        GameState.Instance.SwitchScene(HubLevel.KEY);
        // GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, HubLevel.KEY);
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
}