using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.UI;

public static class UIExtensionMethods
{
    /// <summary>
    /// Creates a nine slice drawing function, that allows to make different sizes of buttons and sprites, while keeping it clean.
    /// </summary>
    /// <param name="spriteBatch">The spritebatch to draw on.</param>
    /// <param name="texture">The texture to draw from.</param>
    /// <param name="destination">The destination rectangle to draw to.</param>
    /// <param name="spriteSource">The source rectangle to draw from.</param>
    /// <param name="color">The color to draw with.</param>
    /// <param name="Left">The size of the (int)slices.X slice.</param>
    /// <param name="Top">The size of the (int)slices.Y slice.</param>
    /// <param name="Right">The size of the right slice.</param>
    /// <param name="Bottom">The size of the (int)slices.W slice.</param>
    public static void DrawNineSlice(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Rectangle? spriteSourc, Color color, Vector4? ninePatchSlices, float layerDepth = 1f)
    {

        Rectangle spriteSource = spriteSourc ?? new Rectangle(0, 0, texture.Width, texture.Height);
        Vector4 slices = ninePatchSlices ?? new Vector4(12, 12, 12, 12);

        int textureWidth = spriteSource.Width;
        int textureHeight = spriteSource.Height;
        //Source Rectangles
        Rectangle TopLeft = new Rectangle(spriteSource.X, spriteSource.Y, (int)slices.X, (int)slices.Y);
        Rectangle TopMiddle = new Rectangle(spriteSource.X + (int)slices.X, spriteSource.Y, textureWidth - (int)slices.X - (int)slices.Z, (int)slices.Y);
        Rectangle TopRight = new Rectangle(spriteSource.X + textureWidth - (int)slices.Z, spriteSource.Y, (int)slices.Z, (int)slices.Y);

        Rectangle MiddleLeft = new Rectangle(spriteSource.X, spriteSource.Y + (int)slices.Y, (int)slices.X, textureHeight - (int)slices.Y - (int)slices.W);
        Rectangle MiddleCenter = new Rectangle(spriteSource.X + (int)slices.X, spriteSource.Y + (int)slices.Y, textureWidth - (int)slices.Z - (int)slices.X, textureHeight - (int)slices.Y - (int)slices.W);
        Rectangle MiddleRight = new Rectangle(spriteSource.X + textureWidth - (int)slices.Z, spriteSource.Y + (int)slices.Y, (int)slices.Z, textureHeight - (int)slices.Y - (int)slices.W);

        Rectangle BottomLeft = new Rectangle(spriteSource.X, spriteSource.Y + textureHeight - (int)slices.W, (int)slices.X, (int)slices.W);
        Rectangle BottomCenter = new Rectangle(spriteSource.X + (int)slices.X, spriteSource.Y + textureHeight - (int)slices.W, textureWidth - (int)slices.X - (int)slices.Z, (int)slices.W);
        Rectangle BottomRight = new Rectangle(spriteSource.X + textureWidth - (int)slices.Z, spriteSource.Y + textureHeight - (int)slices.W, (int)slices.Z, (int)slices.W);
        
        //Destination Rectangles
        Rectangle DestTopLeft = new Rectangle(destination.X, destination.Y, (int)slices.X, (int)slices.Y);
        Rectangle DestTopMiddle = new Rectangle(destination.X + (int)slices.X, destination.Y, destination.Width - (int)slices.X - (int)slices.Z, (int)slices.Y);
        Rectangle DestTopRight = new Rectangle(destination.X + destination.Width - (int)slices.Z, destination.Y, (int)slices.Z, (int)slices.Y);

        Rectangle DestMiddleLeft = new Rectangle(destination.X, destination.Y + (int)slices.Y, (int)slices.X, destination.Height - (int)slices.Y - (int)slices.W);
        Rectangle DestMiddleCenter = new Rectangle(destination.X + (int)slices.X, destination.Y + (int)slices.Y, destination.Width - (int)slices.X - (int)slices.Z, destination.Height - (int)slices.Y - (int)slices.W);
        Rectangle DestMiddleRight = new Rectangle(destination.X + destination.Width - (int)slices.Z, destination.Y + (int)slices.Y, (int)slices.Z, destination.Height - (int)slices.Y - (int)slices.W);   

        Rectangle DestBottomLeft = new Rectangle(destination.X, destination.Y + destination.Height - (int)slices.W, (int)slices.X, (int)slices.W);
        Rectangle DestBottomCenter = new Rectangle(destination.X + (int)slices.X, destination.Y + destination.Height - (int)slices.W, destination.Width - (int)slices.X - (int)slices.Z, (int)slices.W);
        Rectangle DestBottomRight = new Rectangle(destination.X + destination.Width - (int)slices.Z, destination.Y + destination.Height - (int)slices.W, (int)slices.Z, (int)slices.W);

        //Draw Calls
        spriteBatch.Draw(texture, DestTopLeft, TopLeft, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        spriteBatch.Draw(texture, DestTopMiddle, TopMiddle, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        spriteBatch.Draw(texture, DestTopRight, TopRight, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);

        spriteBatch.Draw(texture, DestMiddleLeft, MiddleLeft, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        spriteBatch.Draw(texture, DestMiddleCenter, MiddleCenter, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        spriteBatch.Draw(texture, DestMiddleRight, MiddleRight, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);

        spriteBatch.Draw(texture, DestBottomLeft, BottomLeft, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        spriteBatch.Draw(texture, DestBottomCenter, BottomCenter, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
        spriteBatch.Draw(texture, DestBottomRight, BottomRight, color, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
    }
    /// <summary>
    /// Distributes horizontal space between children in an HBox-style layout.
    ///
    /// Rules (in order of importance):
    /// 1. Minimum size is ALWAYS honored.
    /// 2. We try to reach the desired size next.
    /// 3. If space still remains, we grow up to the maximum size.
    /// 4. No child is ever allowed to exceed its maximum.
    /// 5. The total width is never exceeded.
    ///
    /// This function is deterministic, stable, and safe against edge cases.
    /// </summary>
    public static float[] DistributeSizes(
        float[] minSizes,
        float[] desiredSizes,
        float[] maxSizes,
        float totalWidth)
    {
        int count = minSizes.Length;

        // Final output array
        float[] sizes = new float[count];

        // Normalize max sizes.
        // Convention:
        // max <= 0 means "no limit".
        float[] max = new float[count];
        for (int i = 0; i < count; i++)
        {
            max[i] = maxSizes[i] <= 0f ? float.MaxValue : maxSizes[i];
        }

        // ------------------------------------------------------------
        // STEP 1: Assign minimum sizes
        // ------------------------------------------------------------
        // Minimum size is non-negotiable.
        // If we can't even fit the minimums, we stop here.
        float usedWidth = 0f;

        for (int i = 0; i < count; i++)
        {
            sizes[i] = minSizes[i];
            usedWidth += sizes[i];
        }

        float remainingWidth = totalWidth - usedWidth;

        if (remainingWidth <= 0f)
        {
            // Layout is over-constrained.
            // Returning minimums is the only correct thing to do.
            return sizes;
        }

        // ------------------------------------------------------------
        // STEP 2: Grow children up to their desired sizes
        // ------------------------------------------------------------
        // This is the "normal" layout phase.
        // Most layouts end here.
        remainingWidth = DistributeGrowth(
            sizes,
            desiredSizes,
            max,
            remainingWidth
        );

        // ------------------------------------------------------------
        // STEP 3: If space still remains, grow up to max
        // ------------------------------------------------------------
        // At this point, desired sizes are satisfied.
        // Extra space is distributed evenly to anything that can still grow.
        DistributeGrowth(
            sizes,
            max,
            max,
            remainingWidth
        );

        return sizes;
    }

    /// <summary>
    /// Grows each element toward a target size without exceeding max.
    /// Space is distributed evenly among eligible children.
    ///
    /// Returns the amount of unused space (if any).
    /// </summary>
    private static float DistributeGrowth(
        float[] sizes,
        float[] targets,
        float[] max,
        float availableWidth)
    {
        int count = sizes.Length;

        // We loop because some children may hit their limit earlier than others.
        while (availableWidth > 0f)
        {
            // --------------------------------------------------------
            // Determine which children are allowed to grow
            // --------------------------------------------------------
            int growableCount = 0;

            for (int i = 0; i < count; i++)
            {
                if (sizes[i] < targets[i] && sizes[i] < max[i])
                {
                    growableCount++;
                }
            }

            // No one can grow any further — we're done.
            if (growableCount == 0)
                break;

            // --------------------------------------------------------
            // Evenly divide remaining space
            // --------------------------------------------------------
            float sharePerChild = availableWidth / growableCount;

            bool anyGrowthThisPass = false;

            for (int i = 0; i < count; i++)
            {
                // Skip children that are already at their limit
                if (sizes[i] >= targets[i] || sizes[i] >= max[i])
                    continue;

                // Determine how far this child is allowed to grow
                float upperBound = Math.Min(targets[i], max[i]);
                float allowedGrowth = upperBound - sizes[i];

                if (allowedGrowth <= 0f)
                    continue;

                // Apply growth, but never exceed the allowed limit
                float growth = Math.Min(sharePerChild, allowedGrowth);

                sizes[i] += growth;
                availableWidth -= growth;

                if (growth > 0f)
                    anyGrowthThisPass = true;
            }

            // Safety net:
            // If nothing changed in this pass, we must stop
            // to avoid infinite loops due to floating-point precision.
            if (!anyGrowthThisPass)
                break;
        }

        return availableWidth;
    }

    public static Vector2 GetScreenPercentage(float percentage)
    {
        Vector2 screenSize = ExtendedGame.DrawResolution.ToVector2();
        float factor = percentage / 100f;
        return screenSize * factor;
    }
    public static float GetScreenPercentageWidth(float percentage)
    {
        return ExtendedGame.DrawResolution.X * (percentage / 100f);
    }

    public static float GetScreenPercentageHeight(float percentage)
    {
        return ExtendedGame.DrawResolution.Y * (percentage / 100f);
    }

    /// <summary>
    /// Creates a Vector2 from screen width and height percentages.
    /// Usage: ScreenPercent(50, 20) for 50% width, 20% height
    /// </summary>
    /// <param name="widthPercent">The percentage of screen width (0-100)</param>
    /// <param name="heightPercent">The percentage of screen height (0-100)</param>
    /// <returns>A Vector2 with the specified screen percentages</returns>
    public static Vector2 ScreenPercent(float widthPercent, float heightPercent)
    {
        return new Vector2(GetScreenPercentageWidth(widthPercent), GetScreenPercentageHeight(heightPercent));
    }
}