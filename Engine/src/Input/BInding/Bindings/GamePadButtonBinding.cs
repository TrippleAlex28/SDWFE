using Microsoft.Xna.Framework.Input;

namespace Engine.Input.Binding.Bindings;

public class GamePadButtonBinding : InputBinding
{
    public int PlayerIndex { get; set; }
    public Buttons Button { get; set; }
    
    public GamePadButtonBinding(Buttons button, int playerIndex = 0)
    {
        Button = button;
        PlayerIndex = playerIndex;
    }
    
    public override string GetDisplayName() => $"GamePad {Button}";
    
    public override bool IsPressed(InputState state)
    {
        if (PlayerIndex >= state.CurrentGamePads.Length) return false;
        return state.CurrentGamePads[PlayerIndex].IsButtonDown(Button) &&
               state.PreviousGamePads[PlayerIndex].IsButtonUp(Button);
    }

    public override bool IsDown(InputState state)
    {
        if (PlayerIndex >= state.CurrentGamePads.Length) return false;
        return state.CurrentGamePads[PlayerIndex].IsButtonDown(Button);
    }

    public override bool IsReleased(InputState state)
    {
        if (PlayerIndex >= state.CurrentGamePads.Length) return false;
        return state.CurrentGamePads[PlayerIndex].IsButtonUp(Button) &&
               state.PreviousGamePads[PlayerIndex].IsButtonDown(Button);
    }
}