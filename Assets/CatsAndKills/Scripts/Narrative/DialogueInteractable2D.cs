using System.Collections.Generic;
using CatsAndKills.World;
using UnityEngine;

namespace CatsAndKills.Narrative
{
    public sealed class DialogueInteractable2D :
        MonoBehaviour,
        IInteractable
    {
        [SerializeField] private string displayName =
            "СОБЕСЕДНИК";

        [SerializeField] private string prompt =
            "ПОГОВОРИТЬ [E]";

        [SerializeField] private string startNodeId =
            "start";

        [SerializeField] private string completedFlag;

        [SerializeField] private DialogueNodeData[] nodes;

        private readonly List<DialogueChoiceData>
            _visibleChoices =
                new List<DialogueChoiceData>();

        public string InteractionPrompt =>
            prompt;

        public void Configure(
            string name,
            string interactionPrompt,
            string firstNode,
            DialogueNodeData[] dialogueNodes,
            string completionFlag = null)
        {
            displayName = name;
            prompt = interactionPrompt;
            startNodeId = firstNode;
            nodes = dialogueNodes;
            completedFlag = completionFlag;
        }

        public void Interact()
        {
            NarrativeDialogueSystem system =
                NarrativeDialogueSystem.Instance;

            if (system == null)
                return;

            system.Begin(this);
        }

        public DialogueNodeData ResolveStartNode()
        {
            return ResolveNode(
                startNodeId);
        }

        public DialogueNodeData ResolveNode(
            string id)
        {
            if (nodes == null ||
                string.IsNullOrEmpty(id))
            {
                return null;
            }

            for (int i = 0;
                 i < nodes.Length;
                 i++)
            {
                DialogueNodeData node =
                    nodes[i];

                if (node != null &&
                    node.id == id)
                {
                    return node;
                }
            }

            return null;
        }

        public DialogueChoiceData[] GetVisibleChoices(
            DialogueNodeData node)
        {
            _visibleChoices.Clear();

            if (node == null ||
                node.choices == null)
            {
                return _visibleChoices.ToArray();
            }

            NarrativeWorldState state =
                NarrativeWorldState.Instance;

            for (int i = 0;
                 i < node.choices.Length;
                 i++)
            {
                DialogueChoiceData choice =
                    node.choices[i];

                if (choice == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(
                        choice.requiredFlag) &&
                    (state == null ||
                     !state.HasFlag(
                         choice.requiredFlag)))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(
                        choice.forbiddenFlag) &&
                    state != null &&
                    state.HasFlag(
                        choice.forbiddenFlag))
                {
                    continue;
                }

                _visibleChoices.Add(choice);
            }

            return _visibleChoices.ToArray();
        }

        public void NotifyChoice(
            DialogueChoiceData choice)
        {
        }

        public void NotifyDialogueClosed()
        {
            if (!string.IsNullOrWhiteSpace(
                    completedFlag))
            {
                NarrativeWorldState.Instance
                    ?.SetFlag(
                        completedFlag);
            }
        }
    }
}
