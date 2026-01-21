using Engine;
using Engine.Input;
using Engine.Network.Shared.Session;
using Engine.Scene;
using Engine.Settings;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE;
using SDWFE.Scenes;
using SDWFE.Scenes.Levels;

public class WinScene : Scene
{
    public const string KEY = "WinScene";
    
    private readonly Texture2D _ChineseUITexture;

    public WinScene() : base(KEY)
    {
        BackgroundColor = new Color(0xEA, 0xD2, 0xAD);
        _ChineseUITexture = ExtendedGame.AssetManager.LoadTexture("UI_Chinese", "UI/");
        SetDefaultPlayerClassNull(); // useless default player
        BackgroundColor = new Color(0, 0, 0);

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
        UIRoot.SetRootRect(new Rectangle(0, 0, ExtendedGame.DrawResolution.X, ExtendedGame.DrawResolution.Y));

        // Title text
        

        UIScrollContainer scrollContainer = new UIScrollContainer(false, true, false, 25);
        scrollContainer.AlignmentPoint = Alignment.MiddleCenter;
        scrollContainer.DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);
        UIRoot.AddChild(scrollContainer);

        string[] textLines = new string[]
        {
            "Congratulations, brave warrior!",
            "",
            "You have successfully navigated the perils",
            "of the Song Dynasty and emerged",
            "victorious against all odds.",
            "",
            "Your courage, skill,",
            "and determination have restored peace",
            "to the fallen empire.",
            "",
            "May your legend be remembered ",
            "for generations to come!",
            "",
            "Thank you for playing",
            "SONG DYNASTY WARRIOR OF THE FALLEN EMPIRE",
            "WINNERS OF THE UU GAMECOMPETITION 2026",
            "",
            "",
            "",
            "",
            "|hCREDITS",
            "",
            "",
            "|bGRAND STRATEGIST (Project Lead)",
            "Tijn Reijke",
            "",
            "",
            "|bMASTER OF THE DIGITAL FORGE (Lead Dev)",
            "Alexander Ansinger",
            "",
            "",
            "|bARCHITECT OF THE DYNASTY (Level Design)",
            "David Teeuw",
            "",
            "",
            "|bSCRIBE OF ANCIENT LOGIC (Scripting)",
            "Frank van der Stappen",
            "",
            "",
            "|bGrand Planner (Planning)",
            "Jurjen Hekhuis",
            "",
            "",
            "|bPROFESSIONAL BUG ARCHITECT",
            "Jurjen Hekhuis",
            "",
            "",
            "|bEMOTIONAL SUPPORT HUMAN",
            "Alexander Ansinger",
            "",
            "",
            "|bMASTER OF JUST IN TIME",
            "Frank van der Stappen",
            "",
            "",
            "|bMASTER OF REDOS",
            "Jurjen Hekhuis",
            "",
            "",
            "|bNEVER SKIPPED A MEETING",
            "Tijn Reijke",
            "Frank van der Stappen",
            "",
            "",
            "|bLATE NIGHT GRINDER",
            "Tijn Reijke",
            "Alexander Ansinger",
            "",
            "",
            "|bNETWORKING WIZARD",
            "Alexander Ansinger",
            "",
            "",
            "|bPLACEHOLDER ILLUSTRATOR",
            "Jurjen Hekhuis",
            "",
            "",
            "|bSOUND EFFECTS INTERN",
            "David Teeuw",
            "",
            "",
            "|bTHE PRESENTATION SPECIALIST",
            "David Teeuw",
            "",
            "",
            "|bNEVER TOUCHED MULTIPLAYER",
            "Frank van der Stappen",
            "David Teeuw",
            "Jurjen Hekhuis",
            "",
            "",
            "|hSPECIAL THANKS",
            "",
            "",
            "HodmakGames for the portal animations",
            "",
            "And most importantly...",
            "YOU, THE BRAVE WARRIOR!",
            "",
            "Developed by Group 7a",
            "2026",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
        };

        foreach (var line in textLines)
        {
            string fontName = Resources.UPHEAVEL_FONTNAME;
            SpriteFont textFont = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 12);
            if (line.StartsWith("|b"))
            {
                textFont = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 16);
            }
            else if (line.StartsWith("|h"))
            {
                textFont = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 20);
            }
            var cleanLine = line.Replace("|h", "").Replace("|b", "");
            var lineText = UIVisual.FromText(cleanLine, textFont, Color.White);
            lineText.AlignmentPoint = Alignment.MiddleCenter;
            lineText.DesiredSize = UIExtensionMethods.ScreenPercent(80, 10);
            scrollContainer.AddChild(lineText);
        }
        // Restart button
        var restartButton = CreateButton("Restart", new Rectangle(256, 72, 64, 16), new Vector4(7, 7, 7, 7), fontSize: 12);
        restartButton.AlignmentPoint = Alignment.MiddleCenter;
        restartButton.DesiredSize = UIExtensionMethods.ScreenPercent(60, 20);
        restartButton.Released += (control) => OnRestartClicked();
        scrollContainer.AddChild(restartButton);

        // Restart button
        var MainMenuButton = CreateButton("Main Menu", new Rectangle(256, 72, 64, 16), new Vector4(7, 7, 7, 7), fontSize: 12);
        MainMenuButton.AlignmentPoint = Alignment.MiddleCenter;
        MainMenuButton.DesiredSize = UIExtensionMethods.ScreenPercent(60, 20);
        MainMenuButton.Released += (control) => OnMainMenuClicked();
        scrollContainer.AddChild(MainMenuButton);

        var spacer = new UIElement();
        spacer.DesiredSize = UIExtensionMethods.ScreenPercent(100, 30);
        scrollContainer.AddChild(spacer);
    
    }
    private void OnMainMenuClicked()
    {
        // Go back to main menu
        GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
    }
    private void OnRestartClicked()
    {
        // Restart the game by switching to the main menu
        GameState.Instance.SwitchScene(HubLevel.KEY);
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