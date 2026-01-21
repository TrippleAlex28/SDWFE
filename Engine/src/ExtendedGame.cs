using Engine.Input;
using Engine.Network.Shared.Object;
using Engine.Network.Shared.Packet;
using Engine.Network.Shared.Packet.Packets;
using Engine.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine;

public class ExtendedGame : Game
{
    // public virtual string GAME_NAME { get; protected set; } = "SDWFE";
    public static string GAME_NAME { get; protected set; } = "SDWFE";
    public static string GAME_NAME_SUBTITLE { get; protected set; } = "Warrior of the Fallen Empire";

    public static AssetManager AssetManager { get; private set; } = null!;
    public static Random Random { get; private set; } = null!;
    public static FollowCamera FollowCamera { get; private set; } = null!;
    
    public readonly GraphicsDeviceManager GraphicsDeviceManager;
    public SpriteBatch SpriteBatch { get; private set; } = null!;
    public RenderTarget2D RenderTarget { get; private set; } = null!;
    public static Point DrawResolution { get; private set; } = new(480, 270);
    private static Point _cachedBackBufferSize;

    public static LightShader LightShaderInstance { get; private set; } = null!;
    
    public static string ContentRootDirectory
    {
        get { return "Content"; }
    }

    public static void SetDrawResolution(int width, int height)
    {
        DrawResolution = new Point(width, height);
    }
    
    public ExtendedGame()
    {
        // --- BASE SETUP ---
        this.Content.RootDirectory = ContentRootDirectory;
        
        this.GraphicsDeviceManager = new GraphicsDeviceManager(game: this);
        this.GraphicsDeviceManager.HardwareModeSwitch = false; // False makes going fullscreen less laggy, but is slightly worse for performance I believe
        
        // --- GAMEOBJECT AS NETOBJECT SETUP ---
        NetObjectRegistry.Register<GameObject>(0);
        
        // --- PACKET SETUP ---
        PacketRegistry.Register<ConnectionRequestPacket>(PacketType.ConnectionRequestPacket);
        PacketRegistry.Register<ConnectionAcceptPacket>(PacketType.ConnectionAcceptPacket);
        PacketRegistry.Register<EmptyUdpPacket>(PacketType.EmptyUdpPacket);
        PacketRegistry.Register<DisconnectPacket>(PacketType.DisconnectPacket);
        
        PacketRegistry.Register<ChatPacket>(PacketType.ChatPacket);
        PacketRegistry.Register<CommandPacket>(PacketType.CommandPacket);
        PacketRegistry.Register<SnapshotPacket>(PacketType.SnapshotPacket);
        PacketRegistry.Register<SceneChangePacket>(PacketType.SceneChangePacket);
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        Random = new Random();
        FollowCamera = new FollowCamera(this.GraphicsDevice.Viewport);
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        AssetManager = new AssetManager(this.Content);
        LightShaderInstance = new LightShader();
        SpriteBatch = new SpriteBatch(this.GraphicsDevice);
        RenderTarget = new RenderTarget2D(
            GraphicsDevice,
            DrawResolution.X, DrawResolution.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents
        );

        SettingsManager.Instance.Initialize(GraphicsDeviceManager);
        
        EngineResources.LoadContent(this.GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        InputManager.Instance.Update();
        
        // Dump scene to console bind
        if (InputManager.Instance.GetInputState().CurrentKeyboard.IsKeyDown(Keys.F8) &&
            InputManager.Instance.GetInputState().PreviousKeyboard.IsKeyUp(Keys.F8))
            GameState.Instance.CurrentScene?.WriteScene();
        
        // Fullscreen toggle
        if (InputManager.Instance.GetInputState().CurrentKeyboard.IsKeyDown(Keys.F11) && InputManager.Instance.GetInputState().PreviousKeyboard.IsKeyUp(Keys.F11))
            SettingsManager.Instance.ToggleFullScreen();
        
        // Game exit set bind
        if (InputManager.Instance.GetInputState().CurrentKeyboard.IsKeyDown(Keys.Delete) && InputManager.Instance.GetInputState().PreviousKeyboard.IsKeyUp(Keys.Delete))
            Exit();
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        _cachedBackBufferSize = new Point(
            GraphicsDevice.PresentationParameters.BackBufferWidth,    
            GraphicsDevice.PresentationParameters.BackBufferHeight    
        );
        
        #region Draw Onto RenderTarget 
        
        // Setup RenderTarget
        this.GraphicsDevice.SetRenderTarget(RenderTarget);
        
        // Draw Background
        Color backgroundColor = GameState.Instance.CurrentScene != null
            ? GameState.Instance.CurrentScene.BackgroundColor
            : Color.Black;
        this.GraphicsDevice.Clear(backgroundColor);
        
        // Draw Scene
        SpriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: FollowCamera.GetTransformMatrix()
        );
        GameState.Instance.DrawScene(SpriteBatch);
        SpriteBatch.End();

        #endregion 
        
        #region Draw RenderTarget Onto Screen
        
        // Reset RenderTarget
        this.GraphicsDevice.SetRenderTarget(null);
        this.GraphicsDevice.Clear(Color.Black); // bars color

        var presentationRect = GetPresentationRect();
        
        // Draw onto screen
        SpriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: BlendState.Opaque,
            samplerState: SamplerState.PointClamp
        );
        SpriteBatch.Draw(RenderTarget, presentationRect, Color.White);
        SpriteBatch.End();
        #endregion 
        
        #region Draw Shader
        
        GraphicsDevice.SetRenderTarget(null);

        // var list = new List<PointLight>() { 
        //     new PointLight
        //     {
        //         WorldPosition = new Vector2(240, 135),
        //         WorldRadius = 200,
        //         LightColor = Color.White
        //     }
        // };
        // LightShaderInstance.SetLights(list);
        float screenWidth = GraphicsDevice.Viewport.Width;
        float screenHeight = GraphicsDevice.Viewport.Height;

        if (LightShaderInstance.Enabled){
            SpriteBatch.Begin(effect: LightShaderInstance.LightEffect, blendState: BlendState.AlphaBlend); 

            Rectangle fullScreenRectangle = new Rectangle(0, 0, (int)screenWidth, (int)screenHeight);
            Texture2D whitePixel = EngineResources.BlankSquare;
            SpriteBatch.Draw(
                whitePixel, 
                fullScreenRectangle, 
                Color.Black 
            ); 

            SpriteBatch.End();
        }
        
        
        #endregion
        
        #region Draw UI
        
        // Draw UI
        SpriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix:
                Matrix.CreateScale(
                    presentationRect.Width / (float)DrawResolution.X,
                    presentationRect.Height / (float)DrawResolution.Y,
                    1f
                ) *
                Matrix.CreateTranslation(presentationRect.X, presentationRect.Y, 0)
        );
        GameState.Instance.DrawUI(SpriteBatch);
        SpriteBatch.End();
        
        #endregion
    }
    
    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        InputManager.SaveToFile(InputManager.GetDefaultConfigPath(GAME_NAME));
        SettingsManager.Instance.SaveSettings(SettingsManager.GetDefaultConfigPath(GAME_NAME));
        
        GameState.Instance.SessionManager.CurrentSession?.Reset();
        
        base.OnExiting(sender, args);
    }

    #region Utils
    public static float GetYSort(Vector2 position, Vector2 offset){
        return (float)(position.Y + offset.Y) / 10000f;
    }
    public static Vector2 ScreenToWorld(Vector2 screenPos)
    {
        Vector2 rtPos = ScreenToRenderTarget(screenPos);
        if (rtPos.X < 0)
            return new Vector2(-1, -1);

        return FollowCamera.ScreenToWorld(rtPos);
    }
    
    public static Vector2 WorldToScreen(Vector2 worldPos)
    {
        Vector2 rtPos = FollowCamera.WorldToScreen(worldPos);
        return RenderTargetToScreen(rtPos);
    }

    public static Vector2 GetMouseUIPosition()
    {
        return ScreenToRenderTarget(InputManager.Instance.MousePosition.ToVector2());
    }
    
    #endregion
    
    #region Helpers
    
    /// <summary>
    /// Used the get the desired RenderTarget destination Rectangle
    /// </summary>
    private static Rectangle GetPresentationRect()
    {
        int backW = _cachedBackBufferSize.X;
        int backH = _cachedBackBufferSize.Y;
        
        int scale = Math.Max(1, Math.Min(backW / DrawResolution.X, backH / DrawResolution.Y));

        // Calculate width & height
        int w = DrawResolution.X * scale;
        int h = DrawResolution.Y * scale;

        // Calculate central screen position
        int x = (backW - w) / 2;
        int y = (backH - h) / 2;

        return new Rectangle(x, y, w, h);
    }

    /// <summary>
    /// Converts a screen position to a position on the RenderTarget
    /// </summary>
    private static Vector2 ScreenToRenderTarget(Vector2 screenPoint)
    {
        Rectangle presentation = GetPresentationRect();

        // Outside Game area
        if (!presentation.Contains(screenPoint))
            return new Vector2(-1, -1);

        Vector2 normalized = new Vector2(
            (screenPoint.X - presentation.X) / (float)presentation.Width,
            (screenPoint.Y - presentation.Y) / (float)presentation.Height
        );

        return normalized * DrawResolution.ToVector2();
    }

    private static Vector2 RenderTargetToScreen(Vector2 rtPos)
    {
        Rectangle presentation = GetPresentationRect();

        Vector2 normalized = rtPos / DrawResolution.ToVector2();

        return new Vector2(
            presentation.X + normalized.X * presentation.Width,    
            presentation.Y + normalized.Y * presentation.Height    
        );
    }
    
    #endregion
}