using Microsoft.Xna.Framework;

namespace Engine.UI.Elements;

public class UIHBoxContainer : UIContainer
{
    public UIHBoxContainer() : base()
    {
    }

    protected override void SetChildLayout()
    {
        UIElement[] children = GetChildren().Cast<UIElement>().ToArray();
        int length = children.Length;

        // Get the minimum and maximum sizes of the children in ORDER
        float[] minSizes = children.Select(c => (float)c.MinSize.X).ToArray();
        float[] desiredSizes = children.Select(c => (float)c.DesiredSize.X).ToArray();
        float[] maxSizes = children.Select(c => (float)c.MaxSize.X).ToArray();
        Vector4 TotalPadding = children.Aggregate(Vector4.Zero, (sum, c) => sum + c.Padding);
        Rectangle actualSlot = CalculateActualSlot();
        float totalWidth = actualSlot.Width - Margin.X - Margin.Z;

        // Calculate the final size of each child in ORDER
        float[] finalSizes = UIExtensionMethods.DistributeSizes(minSizes, desiredSizes, maxSizes, totalWidth);
            
        float x = Margin.X;
        foreach (var (child, i) in children.Select((child, index) => (child, index)))
        {
            child.layoutSlot = new Rectangle(
                (int)x,
                (int)(Margin.Y),
                (int)finalSizes[i],
                (int)(actualSlot.Height - Margin.Y - Margin.W)
            );
            
            x += finalSizes[i];
            
            // Spacing between children
            if (i < length - 1)
                x += Spacing;
        }
    }
}