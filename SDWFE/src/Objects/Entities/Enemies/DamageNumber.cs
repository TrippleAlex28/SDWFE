using System;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.Objects.Entities
{
    internal class DamageNumber : GameObject
    {
        private static SpriteFont? _font;
        private readonly string _text;
        private readonly Color _baseColor;
        private readonly float _lifetime;
        private float _age = 0f;
        private Vector2 _velocity;
        private float _scale = 1f;

        public DamageNumber(int amount, Color? color = null, float lifetime = 1f) : base()
        {
            _text = amount.ToString();
            _baseColor = color ?? Color.OrangeRed;
            _lifetime = Math.Max(0.1f, lifetime);

            // start with a gentle upward velocity
            _velocity = new Vector2(0f, -28f);

            // keep damage numbers on top of objects
            this.BaseDrawLayer = 0.94f;
            this.FineDrawLayer = 0.01f;

            // load font
            if (_font == null)
            {
                try
                {
                    _font = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 12);
                }
                catch
                {
                    _font = null;
                }
            }
        }

        protected override void UpdateSelf(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _age += dt;

            // move upward and ease velocity over time for a smooth float
            LocalPosition += _velocity * dt;
            _velocity *= 0.92f;

            if (_age < 0.15f)
                _scale = 1.18f;
            else
                _scale = 1f + 0.12f * (1f - MathHelper.Clamp(_age / _lifetime, 0f, 1f));

            if (_age >= _lifetime)
                RemoveSelf();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (_font == null) return;

            float t = MathHelper.Clamp(_age / _lifetime, 0f, 1f);
            float alpha = 1f - t;

            Vector2 textSize = _font.MeasureString(_text);
            Vector2 drawPos = this.GlobalPosition + this.ScrollOffset;
            Vector2 origin = textSize / 2f;

            // draw a thin outline for readability
            Color outlineColor = Color.Black * (alpha * 0.6f);
            Vector2[] outlineOffsets = new[]
            {
                new Vector2(-1f, -1f),
                new Vector2(1f, -1f),
                new Vector2(-1f, 1f),
                new Vector2(1f, 1f),
            };

            foreach (var off in outlineOffsets)
            {
                spriteBatch.DrawString(_font, _text, drawPos + off, outlineColor, 0f, origin, _scale, SpriteEffects.None, this.DrawLayer);
            }

            spriteBatch.DrawString(_font, _text, drawPos, _baseColor * alpha, 0f, origin, _scale, SpriteEffects.None, this.DrawLayer + 0.0001f);
        }
    }
}