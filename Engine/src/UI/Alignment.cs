namespace Engine.UI;

public enum Alignment
{
    TopLeft,
    TopMiddle,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomMiddle,
    BottomRight
}

// Direction, mainly used so that functions can return either X or Y
public enum Direction
{
    Horizontal,
    Vertical,
}

// Decides whether an element should be scaled by parent or not
public enum SizeMode
{
    Fixed,
    Expand,
}