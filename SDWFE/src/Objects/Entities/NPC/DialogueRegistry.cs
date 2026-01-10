using System;
using System.Collections.Generic;
using Engine;
using Engine.Dialogue;
using System.Linq;
using Engine.UI;

namespace SDWFE;

#nullable enable
public static class DialogueRegistry
{
    private static Dictionary<string, DialogueJsonNode> _jsonNodes = new();
    private static Dictionary<string, DialogueNode> _nodes = new();

    public static void Load(string path)
    {
        var list = JsonManager.Load<List<DialogueJsonNode>>(path);
        _jsonNodes = list.ToDictionary(d => d.Id, d => d);

        foreach (var jsonNode in _jsonNodes.Values)
        {
            if (!_nodes.ContainsKey(jsonNode.Id))
                _nodes[jsonNode.Id] = BuildNode(jsonNode);
        }
    }

    private static DialogueNode BuildNode(DialogueJsonNode jsonNode)
    {
        if (_nodes.TryGetValue(jsonNode.Id, out var existingNode))
            return existingNode;

        var node = new DialogueNode
        {
            SpeakerName = jsonNode.SpeakerName,
            Text = jsonNode.Text,
            Id = jsonNode.Id,
        };
        _nodes[jsonNode.Id] = node;

        foreach (var choiceJson in jsonNode.Choices)
        {
            if (_jsonNodes.TryGetValue(choiceJson.NextId, out var nextJsonNode))
            {
                var nextNode = BuildNode(nextJsonNode);
                node.Choices.Add(new DialogueChoice(choiceJson.ChoiceText, nextNode));
            }
        }

        return node;
    }

    public static DialogueNode? GetNode(string id)
    {
        _nodes.TryGetValue(id, out var node);
        return node;
    }
}




