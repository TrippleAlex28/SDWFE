using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.Pathfinding
{
    public static class PathVisualizer
    {
        // Call this in your Draw method, passing the path and a 1x1 white pixel texture
        public static void DrawPath(SpriteBatch spriteBatch, List<Vector2> path, Texture2D pixel, Color? color = null, float thickness = 2f)
        {
            if (path == null || path.Count < 2 || spriteBatch == null || pixel == null)
                return;

            Color drawColor = color ?? Color.Red;

            for (int i = 0; i < path.Count - 1; i++)
            {
                DrawLine(spriteBatch, pixel, path[i], path[i + 1], drawColor, thickness);
            }

            // Optionally, draw circles at each node
            foreach (var point in path)
            {
                DrawCircle(spriteBatch, pixel, point, 6, drawColor * 0.7f);
            }
        }

        // Draws a line between two points
        private static void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)System.Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();
            spriteBatch.Draw(pixel, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 1f);
        }

        // Draws a filled circle using a pixel texture
        private static void DrawCircle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, int radius, Color color)
        {
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi * i / segments;
                float angle2 = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 p1 = center + radius * new Vector2((float)System.Math.Cos(angle1), (float)System.Math.Sin(angle1));
                Vector2 p2 = center + radius * new Vector2((float)System.Math.Cos(angle2), (float)System.Math.Sin(angle2));
                DrawLine(spriteBatch, pixel, p1, p2, color, 2f);
            }
        }
    }
}
