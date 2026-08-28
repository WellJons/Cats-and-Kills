using System;
using CatsAndKills.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatsAndKills.Narrative
{
    [Serializable]
    public sealed class DialogueChoiceData
    {
        public string text;
        public string nextNodeId;
        public string requiredFlag;
        public string forbiddenFlag;
        public string setFlag;
        public string valueKey;
        public int valueDelta;
        public bool closeDialogue;
    }

    [Serializable]
    public sealed class DialogueNodeData
    {
        public string id;
        public string speaker;

        [TextArea(2, 8)]
        public string text;

        public string nextNodeId;
        public DialogueChoiceData[] choices;
    }

    public sealed class NarrativeDialogueSystem : MonoBehaviour
    {
        public static NarrativeDialogueSystem Instance { get; private set; }

        public static bool IsDialogueOpen =>
            Instance != null &&
            Instance._active != null;

        private DialogueInteractable2D _active;
        private DialogueNodeData _node;

        private GUIStyle _speakerStyle;
        private GUIStyle _textStyle;
        private GUIStyle _choiceStyle;
        private GUIStyle _boxStyle;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Begin(
            DialogueInteractable2D source)
        {
            if (source == null)
                return;

            _active = source;
            _node = source.ResolveStartNode();

            if (_node == null)
            {
                Close();
                return;
            }

            InputConsumption.ConsumeInteract();
        }

        public void Close()
        {
            if (_active != null)
                _active.NotifyDialogueClosed();

            _active = null;
            _node = null;
        }

        private void Update()
        {
            if (_active == null ||
                _node == null)
            {
                return;
            }

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            DialogueChoiceData[] visible =
                _active.GetVisibleChoices(_node);

            if (visible.Length > 0 &&
                keyboard != null)
            {
                int selected = -1;

                if (keyboard.digit1Key.wasPressedThisFrame)
                    selected = 0;
                else if (keyboard.digit2Key.wasPressedThisFrame)
                    selected = 1;
                else if (keyboard.digit3Key.wasPressedThisFrame)
                    selected = 2;
                else if (keyboard.digit4Key.wasPressedThisFrame)
                    selected = 3;

                if (selected >= 0 &&
                    selected < visible.Length)
                {
                    Choose(visible[selected]);
                }

                return;
            }

            if ((visible.Length == 0) &&
                keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame ||
                 keyboard.spaceKey.wasPressedThisFrame))
            {
                AdvanceLinear();
            }
        }

        private void AdvanceLinear()
        {
            if (_active == null ||
                _node == null)
            {
                Close();
                return;
            }

            if (string.IsNullOrEmpty(
                    _node.nextNodeId))
            {
                Close();
                return;
            }

            _node =
                _active.ResolveNode(
                    _node.nextNodeId);

            if (_node == null)
                Close();
        }

        private void Choose(
            DialogueChoiceData choice)
        {
            if (choice == null)
                return;

            NarrativeWorldState state =
                NarrativeWorldState.Instance;

            if (state != null)
            {
                if (!string.IsNullOrWhiteSpace(
                        choice.setFlag))
                {
                    state.SetFlag(
                        choice.setFlag);
                }

                if (!string.IsNullOrWhiteSpace(
                        choice.valueKey) &&
                    choice.valueDelta != 0)
                {
                    state.AddValue(
                        choice.valueKey,
                        choice.valueDelta);
                }
            }

            _active?.NotifyChoice(choice);

            if (choice.closeDialogue ||
                string.IsNullOrEmpty(
                    choice.nextNodeId))
            {
                Close();
                return;
            }

            _node =
                _active.ResolveNode(
                    choice.nextNodeId);

            if (_node == null)
                Close();
        }

        private void EnsureStyles()
        {
            if (_textStyle != null)
                return;

            _boxStyle =
                new GUIStyle(
                    GUI.skin.box);

            _speakerStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold
                };

            _speakerStyle.normal.textColor =
                new Color(
                    0.95f,
                    0.24f,
                    0.46f,
                    1f);

            _textStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    fontSize = 19,
                    wordWrap = true
                };

            _textStyle.normal.textColor =
                Color.white;

            _choiceStyle =
                new GUIStyle(
                    GUI.skin.button)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = true,
                    padding =
                        new RectOffset(
                            14,
                            14,
                            8,
                            8)
                };
        }

        private void OnGUI()
        {
            if (_active == null ||
                _node == null)
            {
                return;
            }

            EnsureStyles();

            float scale =
                Mathf.Clamp(
                    Screen.height /
                    1080f,
                    0.78f,
                    1.4f);

            Matrix4x4 oldMatrix =
                GUI.matrix;

            GUI.matrix =
                Matrix4x4.Scale(
                    new Vector3(
                        scale,
                        scale,
                        1f));

            float width =
                Screen.width /
                scale;

            float height =
                Screen.height /
                scale;

            float boxWidth =
                Mathf.Min(
                    920f,
                    width - 80f);

            DialogueChoiceData[] choices =
                _active.GetVisibleChoices(
                    _node);

            float choiceHeight =
                choices.Length *
                46f;

            float boxHeight =
                165f +
                choiceHeight;

            Rect box =
                new Rect(
                    (width - boxWidth) *
                    0.5f,
                    height -
                    boxHeight -
                    34f,
                    boxWidth,
                    boxHeight);

            Color oldColor =
                GUI.color;

            GUI.color =
                new Color(
                    0.025f,
                    0.030f,
                    0.050f,
                    0.96f);

            GUI.Box(
                box,
                GUIContent.none,
                _boxStyle);

            GUI.color = oldColor;

            GUI.Label(
                new Rect(
                    box.x + 22f,
                    box.y + 16f,
                    box.width - 44f,
                    26f),
                _node.speaker,
                _speakerStyle);

            GUI.Label(
                new Rect(
                    box.x + 22f,
                    box.y + 48f,
                    box.width - 44f,
                    92f),
                _node.text,
                _textStyle);

            float y =
                box.y + 142f;

            for (int i = 0;
                 i < choices.Length;
                 i++)
            {
                DialogueChoiceData choice =
                    choices[i];

                if (GUI.Button(
                        new Rect(
                            box.x + 20f,
                            y,
                            box.width - 40f,
                            39f),
                        (i + 1) +
                        ". " +
                        choice.text,
                        _choiceStyle))
                {
                    Choose(choice);
                    break;
                }

                y += 46f;
            }

            if (choices.Length == 0)
            {
                GUI.Label(
                    new Rect(
                        box.x + 22f,
                        box.yMax - 30f,
                        box.width - 44f,
                        22f),
                    "[Enter] продолжить   [Esc] закончить разговор",
                    _speakerStyle);
            }

            GUI.matrix = oldMatrix;
        }
    }
}
