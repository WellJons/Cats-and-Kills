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

        private bool _conversationProgressed;

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

                if (node == null ||
                    node.id != id ||
                    !IsNodeAvailable(node))
                {
                    continue;
                }

                return node;
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

                if (!string.IsNullOrWhiteSpace(
                        choice.requiredValueKey) &&
                    (state == null ||
                     state.GetValue(
                         choice.requiredValueKey) <
                     choice.minimumValue))
                {
                    continue;
                }

                _visibleChoices.Add(choice);
            }

            return _visibleChoices.ToArray();
        }

        public void NotifyDialogueOpened()
        {
            _conversationProgressed = false;
        }

        public void NotifyNodeAdvanced()
        {
            _conversationProgressed = true;
        }

        public void NotifyChoice(
            DialogueChoiceData choice)
        {
            _conversationProgressed = true;
        }

        public void NotifyDialogueClosed()
        {
            if (_conversationProgressed &&
                !string.IsNullOrWhiteSpace(
                    completedFlag))
            {
                NarrativeWorldState.Instance
                    ?.SetFlag(
                        completedFlag);
            }
        }

        private static bool IsNodeAvailable(
            DialogueNodeData node)
        {
            NarrativeWorldState state =
                NarrativeWorldState.Instance;

            if (!string.IsNullOrWhiteSpace(
                    node.requiredFlag) &&
                (state == null ||
                 !state.HasFlag(
                     node.requiredFlag)))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(
                    node.forbiddenFlag) &&
                state != null &&
                state.HasFlag(
                    node.forbiddenFlag))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(
                    node.requiredValueKey) &&
                (state == null ||
                 state.GetValue(
                     node.requiredValueKey) <
                 node.minimumValue))
            {
                return false;
            }

            return true;
        }
    }
}
