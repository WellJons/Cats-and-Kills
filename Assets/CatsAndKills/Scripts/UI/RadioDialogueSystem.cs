using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class RadioDialogueSystem : MonoBehaviour
    {
        public static RadioDialogueSystem Instance { get; private set; }

        private readonly Queue<(string speaker, string text, float duration)> _queue =
            new Queue<(string, string, float)>();

        private string _speaker = "";
        private string _text = "";
        private bool _showing;

        private GUIStyle _speakerStyle;
        private GUIStyle _textStyle;
        private GUIStyle _boxStyle;

        private void Awake()
        {
            Instance = this;
        }

        public void Say(string speaker, string text, float duration = 3.2f)
        {
            _queue.Enqueue((speaker, text, duration));
            if (!_showing) StartCoroutine(Process());
        }

        public void ShowTransient(string text, float duration = 1f)
        {
            Say("", text, duration);
        }

        private IEnumerator Process()
        {
            _showing = true;

            while (_queue.Count > 0)
            {
                var item = _queue.Dequeue();
                _speaker = item.speaker;
                _text = item.text;
                yield return new WaitForSecondsRealtime(item.duration);
            }

            _speaker = "";
            _text = "";
            _showing = false;
        }

        private void EnsureStyles()
        {
            if (_textStyle != null) return;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = Texture2D.whiteTexture;

            _speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            _speakerStyle.normal.textColor = new Color(0.95f, 0.22f, 0.48f);

            _textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true
            };
            _textStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_text)) return;
            EnsureStyles();

            float width = Mathf.Min(760f, Screen.width - 60f);
            Rect box = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height - 150f,
                width,
                95f);

            Color old = GUI.color;
            GUI.color = new Color(0.035f, 0.04f, 0.065f, 0.92f);
            GUI.Box(box, GUIContent.none, _boxStyle);
            GUI.color = old;

            if (!string.IsNullOrEmpty(_speaker))
                GUI.Label(new Rect(box.x + 18, box.y + 10, width - 36, 24), _speaker, _speakerStyle);

            GUI.Label(
                new Rect(box.x + 18, box.y + 33, width - 36, 56),
                _text,
                _textStyle);
        }
    }
}
