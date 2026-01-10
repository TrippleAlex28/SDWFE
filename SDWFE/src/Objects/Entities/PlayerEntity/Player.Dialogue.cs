using Engine;
using Engine.Input;
using Microsoft.Xna.Framework;
using SDWFE.UI.Dialogue;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    private UIDialogue? _dialogue;
    private UIDialogueChoice? _dialogueChoice;
    
    /// <summary>
    /// Gets the player's dialogue UI.
    /// </summary>
    public UIDialogue? Dialogue => _dialogue;
    public UIDialogueChoice? DialogueChoice => _dialogueChoice;
    
    /// <summary>
    /// Whether the player is currently viewing a dialogue.
    /// </summary>
    public bool IsInDialogue => _dialogue?.IsOpen ?? false;

    private void ConstructDialogue()
    {
        _dialogue = new UIDialogue();
        _dialogueChoice = new UIDialogueChoice();
        _dialogue.OnDialogueClosed += OnDialogueClosed;
    }

    private void OnDialogueClosed()
    {
        HotbarUI.IsVisible = true;
        WeaponsUI.IsVisible = true; 
        StatsUI.IsVisible = true; 
    }

    /// <summary>
    /// Shows a dialogue with the specified text.
    /// Use "|p" in text to manually create page breaks.
    /// </summary>
    /// <param name="text">The dialogue text to display.</param>
    public void ShowDialogue(string text)
    {
        _dialogue?.Show(text);
        OnDialogueOpen();
    }

    private void ShowChoiceDialogue(string nodeId)
    {
        _dialogueChoice?.Show(nodeId);
        OnDialogueOpen();
        
    }

    private void OnDialogueOpen()
    {
        HotbarUI.IsVisible = false; 
        WeaponsUI.IsVisible = false; 
        StatsUI.IsVisible = false;
    }
    /// <summary>
    /// Hides the current dialogue if visible.
    /// </summary>
    public void HideDialogue()
    {
        _dialogue?.Hide();
    }

    private void UpdateDialogue(GameTime gameTime)
    {
        // Demo: Press T to show a test dialogue
        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_DIALOGUE) && !IsInDialogue)
        {
            ShowChoiceDialogue("fireman_root");
            // ShowDialogue(
            //     "Welcome, adventurer! This is a test of the dialogue system.|p" +
            //     "As you can see, the text appears letter by letter, creating a typewriter effect.|p" +
            //     "You can click anywhere in the dialogue box to speed up the text, or advance to the next page.|p" +
            //     "When the text overflows the box, it automatically creates new pages. This is useful for long conversations with NPCs or for telling stories in your game.|p" +
            //     "Press Escape or click after the last page to close this dialogue. Good luck on your adventure!"
            // );
        }
    }
}
