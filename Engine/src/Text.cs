using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Engine;

public class Text : GameObject
{
    private readonly SpriteFont _font;
    private string label;
    private Color color;
    private Vector2 offset;
    private int spriteWidth;

    public Text(string label, SpriteFont font, Color? color = null, Vector2? offset = null, int spriteWidth = 16) : base()
    {
        this.label = label;
        this._font = font;
        this.color = color ?? Color.White;
        this.offset = offset ?? new Vector2(0, -20);
        this.spriteWidth = spriteWidth;

        this.BaseDrawLayer = 0.9f;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (string.IsNullOrEmpty(this.label))
            return;

        // Center the text
        Vector2 textSize = _font.MeasureString(this.label);
        Vector2 centeredOffset = new Vector2(-textSize.X / 2 + this.spriteWidth, 0);

        Vector2 drawPosition = this.GlobalPosition + this.ScrollOffset + this.offset + centeredOffset;

        spriteBatch.DrawString(
            this._font,
            this.label,
            drawPosition,
            this.color,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            this.DrawLayer
        );
    }
}