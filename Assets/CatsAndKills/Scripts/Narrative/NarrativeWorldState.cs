using System.Collections.Generic;
using UnityEngine;

namespace CatsAndKills.Narrative
{
    public sealed class NarrativeWorldState : MonoBehaviour
    {
        public static NarrativeWorldState Instance { get; private set; }

        private readonly HashSet<string> _flags =
            new HashSet<string>();

        private readonly Dictionary<string, int> _values =
            new Dictionary<string, int>();

        public event System.Action<string, bool> FlagChanged;
        public event System.Action<string, int> ValueChanged;

        private void Awake()
        {
            if (Instance != null &&
                Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool HasFlag(string key)
        {
            return !string.IsNullOrEmpty(key) &&
                   _flags.Contains(key);
        }

        public void SetFlag(
            string key,
            bool value = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            bool changed;

            if (value)
                changed = _flags.Add(key);
            else
                changed = _flags.Remove(key);

            if (changed)
                FlagChanged?.Invoke(key, value);
        }

        public int GetValue(
            string key,
            int fallback = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
                return fallback;

            return _values.TryGetValue(
                key,
                out int value)
                ? value
                : fallback;
        }

        public void SetValue(
            string key,
            int value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _values[key] = value;
            ValueChanged?.Invoke(key, value);
        }

        public int AddValue(
            string key,
            int delta)
        {
            int value =
                GetValue(key) +
                delta;

            SetValue(key, value);

            return value;
        }
    }
}
