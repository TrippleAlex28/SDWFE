using Engine.UI;
namespace Engine.Dialogue;

#nullable enable
public class DialogueNode
{
    public string Id { get; set; } = "";
    // optional multi person dialogue
    public string SpeakerName { get; set; } = "";
    public string Text { get; set; } = "";

    // optional for when there are more than 1 speakers, you can change the picture with each node
    public TextureInfo? SpeakerPicture { get; set; }
    public List<DialogueChoice> Choices { get; set; } = new List<DialogueChoice>();

    public DialogueNode() {}
    public DialogueNode(string speakerName, string text, string id, List<DialogueChoice> choices, TextureInfo? speakerPicture = null)
    {
        this.SpeakerName = speakerName;
        this.Text = text;
        this.Id = id;
        this.Choices = choices;
        this.SpeakerPicture = speakerPicture;
    }
}

public class DialogueJsonNode
{
    public string Id { get; set; } = "";
    public string SpeakerName { get; set; } = "";
    public string Text { get; set; } = "";
    public List<DialogueChoiceJson> Choices { get; set; } = new();
}

