using System.Collections.Generic;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class WorldCalloutSystem : MonoBehaviour
    {
        public static WorldCalloutSystem Instance { get; private set; }

        private class Callout
        {
            public Transform target;
            public string text;
            public float until;
        }

        private readonly List<Callout> _items = new List<Callout>();
        private GUIStyle _style;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Show(Transform target, string text, float duration = 1.15f)
        {
            if (target == null) return;
            _items.Add(new Callout
            {
                target = target,
                text = text,
                until = Time.unscaledTime + duration
            });
        }

        private void Update()
        {
            _items.RemoveAll(x => x.target == null || Time.unscaledTime > x.until);
        }

        private void OnGUI()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                _style.normal.textColor = Color.white;
            }

            foreach (var item in _items)
            {
                Vector3 sp = cam.WorldToScreenPoint(item.target.position + Vector3.up * 0.9f);
                if (sp.z < 0f) continue;

                Vector2 size = _style.CalcSize(new GUIContent(item.text));
                Rect r = new Rect(
                    sp.x - size.x * 0.5f - 8,
                    Screen.height - sp.y - 18,
                    size.x + 16,
                    26);

                GUI.Box(r, item.text, _style);
            }
        }
    }
}
