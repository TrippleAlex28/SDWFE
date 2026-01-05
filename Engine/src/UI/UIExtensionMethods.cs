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

    // this function makes child size distribution work with max sizes and min sizes
    public static float[] DistributeSizes(this float[] minSize, float totalWidth, float[] maxSizes)
    {
        int n = minSize.Length;
        float[] size = new float[n];
        float[] maxsize = new float[n];

        for (int i = 0; i < n; i++)
        {
            maxsize[i] = maxSizes[i] <= 0 ? float.MaxValue : maxSizes[i];
        }
        // Assign all min sizes first
        float used = 0f;
        for (int i = 0; i < n; i++)
        {
            size[i] = minSize[i];
            used += size[i];
        }

        float extra = totalWidth - used;
        if (extra <= 0)
            return size; // nothing to distribute

        // Create index list for sorting and grouping
        List<int> order = Enumerable.Range(0, n)
            .OrderBy(i => size[i])
            .ToList();

        for (int i = 0; i < n; i++)
        {
            // Find the group of lowest sized elements
            float current = size[order[i]];
            int groupCount = i + 1;

            // Expand group: all items with same size
            while (groupCount < order.Count && size[order[groupCount]] == current)
                groupCount++;

            // Determine next target size for the group
            float nextSize = float.MaxValue;

            // Next distinct size
            if (groupCount < order.Count)
                nextSize = Math.Min(nextSize, size[order[groupCount]]);

            // Group max constraint
            for (int k = i; k < groupCount; k++)
                nextSize = Math.Min(nextSize, maxsize[order[k]]);

            if (nextSize <= current)
            {
                // Everything is stuck; no child in this group can grow
                break;
            }

            // Cost to raise group to nextSize
            float delta = nextSize - current;
            float cost = delta * (groupCount - i);

            if (extra >= cost)
            {
                // Fully raise group
                for (int k = i; k < groupCount; k++)
                    size[order[k]] = nextSize;

                extra -= cost;
            }
            else
            {
                // Only partially raise them
                float add = extra / (groupCount - i);
                for (int k = 0; k < groupCount; k++)
                {
                    float limit = maxsize[order[k]] - size[order[k]];
                    float apply = Math.Min(add, limit);
                    size[order[k]] += apply;
                    extra -= apply;
                }
                break;
            }
            // Resort order for next iteration
            order = order.OrderBy(i => size[i]).ToList();
        }
        return size;
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
}