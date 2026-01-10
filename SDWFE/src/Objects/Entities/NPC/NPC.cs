using Engine.Dialogue;
using Engine;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Engine.Sprite;

#nullable enable
public class NPC : GameObject
{
    private string _npcname;
    private bool _isRepeatable;
    private bool _completed;

    private DialogueNode? _currentDialogue;
    private DialogueNode? _rootDialogue;

    public bool IsInRange = false;
    private bool _wasInRange = false;

    private Sprite sprite;
    private Sprite speechBubble;

    private NPCJsonNode _npcdata = new NPCJsonNode();
    private static Random _random = new Random();


    public NPC(string name, DialogueNode? dialogue = null, bool isRepeatable = false)
    {
        _npcname = name;
        _rootDialogue = dialogue;
        _isRepeatable = isRepeatable;


        load();
        sprite = new Sprite(ExtendedGame.AssetManager.LoadTexture("TempNPC", "UI/"))
        {
            SourceRectangle = new Rectangle(new Point(0, 0), new Point(16, 32)),
            LocalPosition = new Vector2(0, 16)
        };
        speechBubble = new Sprite(ExtendedGame.AssetManager.LoadTexture("TempNPC", "UI/"))
        {
            SourceRectangle = new Rectangle(new Point(16, 0), new Point(16, 16)),
            LocalPosition = new Vector2(0, 0)
        };
        speechBubble.IsVisible = false;
        this.AddChild(speechBubble);
        this.AddChild(sprite);
    }
    public void load()
    {
        string path = "../../../Objects/Entities/NPC/States.json";
        Dictionary<string, NPCJsonNode> existingData = LoadData<NPCJsonNode>(path);
        if (existingData.TryGetValue(_npcname, out NPCJsonNode? character))
        {
            _npcdata = character;
            
            if (_isRepeatable)
            {
                _npcdata.Choices = new List<DialogueChoiceJson>();
            }
            _completed = character.Completed;
            while  (_npcdata.FillerWords.Count < 3)
            {
                string? newFiller = GetRandomNewFiller();
                if (newFiller == null) { break; }
                _npcdata.FillerWords.Add(newFiller);
            } 
        }
        SafelySave(path, new Dictionary<string, NPCJsonNode>() { {_npcname, _npcdata} });

    }
    public void ToggleSpeechBubble()
    {   
        if (_completed && !_isRepeatable)
        {
            speechBubble.IsVisible = false;
        } else
        {
            speechBubble.IsVisible = !speechBubble.IsVisible;
        }
    }
    public void StartDialogue(DialogueNode? dialogue = default)
    {
        if (dialogue == default && _rootDialogue == null || _completed && !_isRepeatable) { return; }

        if (dialogue != default)
        {
            _currentDialogue = dialogue;
        } else
        {
            _currentDialogue = _rootDialogue;
        }
        // DialogueOverlay.Instance.ChoiceSelected += OnChoiceSelected;
        // DialogueOverlay.Instance.OnTextFinished += EndDialogue;
        // ShowCurrentDialogue();
    }


    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

        if ((IsInRange && !_wasInRange) || (!IsInRange && _wasInRange))
        {
            _wasInRange = !_wasInRange;
            ToggleSpeechBubble();
        } 
    }
    private void ShowCurrentDialogue()
    {
        //SceneManager.Instance.SetUIGameTree(DialogueOverlay.Instance);
        if (_currentDialogue == null) { return; }
        int choicesCount = _currentDialogue.Choices.Count;

        if (_npcdata.FillerWords.Count > 0)
        {
            string randomFiller = _npcdata.FillerWords[_random.Next(_npcdata.FillerWords.Count)];
            _currentDialogue.Text = randomFiller + "|p" + _currentDialogue.Text;
        }
        
        if (choicesCount == 0)
        {
            //DialogueOverlay.Instance.DialogueBackground(_currentDialogue);
        }
        else if (choicesCount <= 3)
        {
            //DialogueOverlay.Instance.DialogueBackgroundChoices(_currentDialogue);
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (_currentDialogue == null) { return; }

        if (choiceIndex >= 0 && choiceIndex < _currentDialogue.Choices.Count)
        {
            var currentChoice = _currentDialogue.Choices[choiceIndex];
            DialogueChoiceJson choiceJson = new DialogueChoiceJson() { ChoiceText = currentChoice.ChoiceText, NextId = currentChoice.NextNode.Id }; 
            _npcdata.Choices.Add(choiceJson);
            _currentDialogue = _currentDialogue.Choices[choiceIndex].NextNode;
            ShowCurrentDialogue();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        if (_currentDialogue?.Choices.Count > 0) { return; }
        _currentDialogue = null;
        if (!_isRepeatable)
        {
            _npcdata.Completed = true;
        }else
        {
            _npcdata.Completed = false;
        }

        SafelySave("../../../Objects/Entities/NPC/States.json", new Dictionary<string, NPCJsonNode>() { {_npcname, _npcdata} });
        
        //DialogueOverlay.Instance.RemoveDialogue();
        load();
        //DialogueOverlay.Instance.OnTextFinished -= EndDialogue;
        //DialogueOverlay.Instance.ChoiceSelected -= OnChoiceSelected;
    }
    public static string? GetRandomNewFiller()
    {
        string path = "../../../Objects/Entities/NPC/FillerWords.json";
        Dictionary<string, List<string>> fillerWords = LoadData<List<string>>(path);

        List<string> allWords = fillerWords["FillerWords"];
        List<string> wordsAlreadyUsed = fillerWords["FillerWordsUsed"];
        List<string> remainingWords = allWords.Except(wordsAlreadyUsed).ToList();

        if (remainingWords.Count > 0)
        {
            string newWord = remainingWords[_random.Next(remainingWords.Count)];

            wordsAlreadyUsed.Add(newWord);
            SafelySave(path, new Dictionary<string, List<string>> () { {"FillerWordsUsed", wordsAlreadyUsed} });
            return newWord;
        }
        return null;
    }
    public static void SafelySave<T>(string path, Dictionary<string, T> data)
    {
        Dictionary<string, T> existingData = LoadData<T>(path);

        //add new npc data
        foreach (var element in data)
        {
            existingData[element.Key] = element.Value;
        }

        JsonManager.Save(existingData, path);
    }
    public static Dictionary<string, T> LoadData<T>(string path)
    {
        if (File.Exists(path))
        {
            return JsonManager.Load<Dictionary<string, T>>(path) ?? new Dictionary<string, T>();
        }
        return new Dictionary<string, T>();
    }
}


public class NPCJsonNode
{
    public bool Completed { get; set; }
    public List<DialogueChoiceJson> Choices { get; set; } = new();
    public List<string> FillerWords { get; set; } = new();
}