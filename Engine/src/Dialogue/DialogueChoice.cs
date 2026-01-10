namespace Engine.Dialogue;

#nullable enable
public class DialogueChoice
{
    public string ChoiceText { get; set; } = "";
    public DialogueNode? NextNode { get; set; }

    public DialogueChoice() {}
    public DialogueChoice(string choiceText, DialogueNode nextNode)
    {
        ChoiceText = choiceText;
        NextNode = nextNode;
    }
}
public class DialogueChoiceJson
{
    public string ChoiceText { get; set; } = "";
    public string NextId { get; set; } = "";
}