using Microsoft.Xna.Framework.Input;

namespace Engine.Input;

/// <summary>
/// Holds the current and previous frame input states
/// </summary>
public sealed class InputState
{
    public KeyboardState CurrentKeyboard { get; private set; }
    public KeyboardState PreviousKeyboard { get; private set; }
    
    public MouseState CurrentMouse { get; private set; }
    public MouseState PreviousMouse { get; private set; }
    
    public GamePadState[] CurrentGamePads { get; private set; }
    public GamePadState[] PreviousGamePads { get; private set; }

    public InputState(int maxGamePads = 4)
    {
        CurrentGamePads = new GamePadState[maxGamePads];
        PreviousGamePads = new GamePadState[maxGamePads];
    }

    public void Update()
    {
        // Store previous states
        PreviousKeyboard = CurrentKeyboard;
        PreviousMouse = CurrentMouse;
        for (int i = 0; i < CurrentGamePads.Length; i++)
        {
            PreviousGamePads[i] = CurrentGamePads[i];
        }

        // Get new states
        CurrentKeyboard = Keyboard.GetState();
        CurrentMouse = Mouse.GetState();
        for (int i = 0; i < CurrentGamePads.Length; i++)
        {
            CurrentGamePads[i] = GamePad.GetState(i);
        }
    }
}