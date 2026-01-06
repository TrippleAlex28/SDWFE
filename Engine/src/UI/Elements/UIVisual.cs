using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI.Elements;

public enum VisualType
    {
        Texture,
        Text,
        Color,
        StretchTexture,
    }

    #nullable enable
    public class UIVisual : UIElement
    {
        // Optional source rectangle for the texture. 
        // Null if this visual is text or a solid color panel.
        private Rectangle? _sourceRect;
        public Rectangle? SourceRect
        {
            get => _sourceRect;
            set
            {
                _sourceRect = value;
                SetDesiredSize();
            }   
        }

        public Rectangle? OriginalSourceRect;
        // The main texture to draw. Null if this visual is text or a solid color.
        public Texture2D? source;

        // Text to render. Null if this visual is a texture or solid color.
        private string? _text;
        public string? Text
        {
            get => _text;
            set
            {
                _text = value;
                SetDesiredSize();
            }
        }

        // Font used for rendering text. Only relevant if 'text' is set.
        protected SpriteFont? font;

        // Color or tint applied to the visual. 
        // For textures, this tints the image. 
        // For text or solid color visuals, this defines the color.
        protected Color color;

        // Public tint accessor so callers can tint visuals (useful for rarity borders)
        public Color Tint
        {
            get => color;
            set => color = value;
        }

        protected Vector4? slicePatch;

        // 
        private VisualType _visualtype;
        public VisualType VisualType => _visualtype;

        private UIVisual()
        {
            _layoutDirty = true;
        }

        public static UIVisual FromTexture(Texture2D texture, Rectangle? sourceRect = null)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            
            return new UIVisual()
            {
                _visualtype = VisualType.Texture,
                source = texture,
                SourceRect = sourceRect,
                OriginalSourceRect = sourceRect,
                Text = null,
                font = null,
                color = Color.White
            };
        }

        public static UIVisual FromText(string text, SpriteFont font, Color color)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            if (font == null)
                throw new ArgumentNullException(nameof(font));
            
            return new UIVisual()
            {
                _visualtype = VisualType.Text,
                source = null,
                _sourceRect = null,
                _text = text,
                font = font,
                color = color
            };
        }

        public static UIVisual FromColor(Color color)
        {
            return new UIVisual()
            {
                _visualtype = VisualType.Color,
                source = null,
                SourceRect = null,
                Text = null,
                font = null,
                color = color
            };
        }

        public static UIVisual FromStretchableTexture(Texture2D texture, Vector4 ninePatchSlice, Rectangle? sourceRect = null)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));
            
            return new UIVisual()
            {
                _visualtype = VisualType.StretchTexture,
                source = texture,
                SourceRect = sourceRect,
                OriginalSourceRect = sourceRect,
                Text = null,
                font = null,
                color = Color.White,
                slicePatch = ninePatchSlice
            };
        }

        public void SetDesiredSize()
        {
            if (DesiredSize != Vector2.Zero)
                    return;
            switch (VisualType)
            {
                
                case VisualType.Texture:
                    DesiredSize = SourceRect == null ? new Vector2(source!.Width, source.Height) : new Vector2(SourceRect.Value.Width, SourceRect.Value.Height);
                    break;
                case VisualType.Text:
                    if (font == null || Text == null)
                        throw new InvalidOperationException("Font and Text must be set for Text visuals.");
                    DesiredSize = new Vector2(font!.MeasureString(Text).X, font.MeasureString(Text).Y);
                    break;
                case VisualType.Color:
                    DesiredSize = new Vector2(layoutSlot.Width, layoutSlot.Height);
                    break;
                case VisualType.StretchTexture:
                    DesiredSize = SourceRect == null ? new Vector2(source!.Width, source.Height) : new Vector2(SourceRect.Value.Width, SourceRect.Value.Height);
                    break;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            switch (VisualType)
            {
                case VisualType.Texture:
                    spriteBatch.Draw(source, GetDestRect(), SourceRect, color, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
                    break;
                case VisualType.Text:
                    Vector2 textPos = GetTextDrawPosition();
                    
                    spriteBatch.DrawString(font, Text, textPos, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, DrawLayer);
                    break;
                case VisualType.Color:
                    spriteBatch.Draw(EngineResources.BlankSquare, GetDestRect(), SourceRect, color, 0f, Vector2.Zero, SpriteEffects.None, DrawLayer);
                    break;
                case VisualType.StretchTexture:
                    if (source != null)
                        spriteBatch.DrawNineSlice(source, GetDestRect(), SourceRect, color, slicePatch, DrawLayer);
                    break;
            }
        }
        private Vector2 GetTextDrawPosition(){
            Rectangle actualSlot = CalculateActualSlot();
            Vector2 textSize = font!.MeasureString(Text);
            Vector2 alignmentAnchor = ResolveAlignment();
            
            // Calculate the offset based on alignment and text size
            Vector2 textOffset = -textSize * alignmentAnchor;
            
            // Position text at the center of the actual slot with alignment offset
            Vector2 textPosition = new Vector2(
                GlobalPosition.X + actualSlot.Width * alignmentAnchor.X,
                GlobalPosition.Y + actualSlot.Height * alignmentAnchor.Y
            );
            
            return textPosition + textOffset;
        }
        private Rectangle GetDestRect()
        {
            Rectangle actualSlot = CalculateActualSlot();

            return new Rectangle(
                (int)GlobalPosition.X,
                (int)GlobalPosition.Y,
                actualSlot.Width,
                actualSlot.Height
            );
        }
    }