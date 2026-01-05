using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Sprite;

public class Sprite : GameObject
{
    public Sprite(Texture2D texture)
    {
        Texture = texture;
    }

    public Texture2D Texture { get; set; }
    /// <summary>
    /// Use for flipping the sprite horizontally or vertically.
    /// This is especially useful if you have a sidescrolling game.
    /// No effect is applied by default.
    /// </summary>
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    /// <summary>
    /// The color used to modulate the sprite.
    /// </summary>
    public Color Color { get; set; } = Color.White;
    public Vector2 GlobalDrawPosition => GlobalPosition - ScrollOffset;
    public Vector2 Scale { get; set; } = Vector2.One;
    /// <summary>
    /// How much the sprite is offset compared to its position.
    /// </summary>
    public Vector2 Origin => SourceRectangle.GetOrigin(OriginType);
    /// <summary>
    /// How the sprite is offset compared to its position.
    /// It is set to top left by default (as you would expect from MonoGame).
    /// </summary>
    public OriginType OriginType { get; set; } = OriginType.TopLeft;
    /// <summary>
    /// How much the sprite is rotated. Monogame uses radians for rotation.
    /// Modulo (2 PI) is applied on a provided value to ensure that values
    /// are always within the range [0, 2 * PI) and correctly converted.
    /// </summary>
    public float AngleRadians
    {
        get => _rotationRad;
        set => _rotationRad = value.Modulo(2 * MathF.PI);
    }
    private float _rotationRad = 0;
    /// <summary>
    /// How much the sprite is rotated.
    /// This is an alternative to RotationRadians.
    /// It is a bit more slow, because of conversions between DEG and RAD.
    /// </summary>
    public float AngleDegrees
    {
        get => MathHelper.ToDegrees(AngleRadians);
        set => AngleRadians = MathHelper.ToRadians(value.Modulo(360));
    }
    /// <summary>
    /// The portion of the texture that is drawn.
    /// Set this if the texture is a sprite sheet.
    /// The whole texture is drawn by default.
    /// </summary>
    public Rectangle SourceRectangle
    {
        get => _sourceRectangle ?? Texture.Bounds;
        set => _sourceRectangle = value;
    }
    private Rectangle? _sourceRectangle;
    /// <summary>
    /// A rectangle around the portion of the texture that is drawn without rotation.
    /// Useful for simple collision detection (e.g. mouse point).
    /// Possibly less useful for 'combat' collision, because the source
    /// rectangle can be too large to be practical for such a use case.
    /// In case of detecting a mouse point, make sure that mouse coordinates
    /// are translated to world coordinates.
    /// </summary>
    public Rectangle BoundingBox
    {
        get
        {
            return new Rectangle(
                location: (GlobalDrawPosition - Origin * Scale).ToPoint(),
                size: new Point(
                    (int)MathF.Round(Scale.X * SourceRectangle.Width),
                    (int)MathF.Round(Scale.Y * SourceRectangle.Height)
                    )
                );
        }
    }
    /// <summary>
    /// Determines how opaque the sprite is. Opacity is an antonym for transparency.
    /// A value of 1 means absolutely no transparency. A value of 0 means full transparency.
    /// Values that exceed the range are clamped automatically.
    /// </summary>
    public float Opacity
    {
        get => _opacity;
        set => _opacity = value.Clamp(0, 1);
    }
    private float _opacity = 1;
    /// <summary>
    /// The inverse of Opacity. Determines how transparent the sprite is.
    /// A value of 1 means absolutely full transparency. A value of 0 means no transparency.
    /// Values that exceed the range are clamped automatically.
    /// </summary>
    public float Transparency
    {
        get => 1 - Opacity;
        set => _opacity = 1 - value.Clamp(0, 1);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture: Texture,
            position: GlobalDrawPosition,
            sourceRectangle: SourceRectangle,
            color: Color * Opacity,
            rotation: AngleRadians,
            origin: Origin,
            scale: Scale,
            effects: Effects,
            layerDepth: DrawLayer
            );
    }
}