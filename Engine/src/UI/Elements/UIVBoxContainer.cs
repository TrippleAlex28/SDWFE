using Microsoft.Xna.Framework;

namespace Engine.UI.Elements;

public class UIVBoxContainer : UIContainer
{
    public UIVBoxContainer() : base()
    {
    }

    protected override void SetChildLayout()
    {
        UIElement[] children = GetChildren().Cast<UIElement>().ToArray();
        int length = children.Length;

        // Get the minimum, desired, and maximum HEIGHTS of the children (in order)
        float[] minSizes = children.Select(c => (float)c.MinSize.Y).ToArray();
        float[] desiredSizes = children.Select(c => (float)c.DesiredSize.Y).ToArray();
        float[] maxSizes = children.Select(c => (float)c.MaxSize.Y).ToArray();

        Rectangle actualSlot = CalculateActualSlot();

        float totalHeight = actualSlot.Height - Margin.Y - Margin.W;

        // Calculate the final HEIGHT of each child
        float[] finalSizes = UIExtensionMethods.DistributeSizes(
            minSizes,
            desiredSizes,
            maxSizes,
            totalHeight
        );

        float y = Margin.Y;

        foreach (var (child, i) in children.Select((child, index) => (child, index)))
        {
            child.layoutSlot = new Rectangle(
                (int)Margin.X,
                (int)y,
                (int)(actualSlot.Width - Margin.X - Margin.Z),
                (int)finalSizes[i]
            );

            y += finalSizes[i];
        }
    }
}