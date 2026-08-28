using System.Collections.Generic;
using UnityEngine;

namespace CatsAndKills.World
{
    [DisallowMultipleComponent]
    public sealed class BuildingRoofFader2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] roofRenderers;
        [SerializeField] private Collider2D interiorTrigger;
        [SerializeField, Range(0f, 1f)] private float insideAlpha = 0.08f;
        [SerializeField] private float fadeSpeed = 5.5f;

        private readonly HashSet<Transform> _playersInside =
            new HashSet<Transform>();

        private float[] _baseAlpha;

        public bool PlayerInside =>
            _playersInside.Count > 0;

        public void Configure(
            SpriteRenderer[] renderers,
            Collider2D trigger,
            float fadedAlpha = 0.08f,
            float speed = 5.5f)
        {
            roofRenderers = renderers;
            interiorTrigger = trigger;
            insideAlpha =
                Mathf.Clamp01(
                    fadedAlpha);
            fadeSpeed =
                Mathf.Max(
                    0.1f,
                    speed);

            if (interiorTrigger != null)
                interiorTrigger.isTrigger = true;

            CacheBaseAlpha();
            ApplyImmediate(false);
        }

        private void Awake()
        {
            if (roofRenderers == null ||
                roofRenderers.Length == 0)
            {
                roofRenderers =
                    GetComponentsInChildren<SpriteRenderer>(
                        true);
            }

            if (interiorTrigger == null)
            {
                foreach (Collider2D col in
                         GetComponentsInChildren<Collider2D>(
                             true))
                {
                    if (col != null &&
                        col.isTrigger)
                    {
                        interiorTrigger = col;
                        break;
                    }
                }
            }

            CacheBaseAlpha();
        }

        private void OnDisable()
        {
            _playersInside.Clear();
        }

        private void CacheBaseAlpha()
        {
            if (roofRenderers == null)
            {
                _baseAlpha = null;
                return;
            }

            _baseAlpha =
                new float[roofRenderers.Length];

            for (int i = 0;
                 i < roofRenderers.Length;
                 i++)
            {
                SpriteRenderer sr =
                    roofRenderers[i];

                _baseAlpha[i] =
                    sr != null
                        ? sr.color.a
                        : 1f;
            }
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            RegisterPlayer(
                other,
                true);
        }

        private void OnTriggerStay2D(
            Collider2D other)
        {
            RegisterPlayer(
                other,
                true);
        }

        private void OnTriggerExit2D(
            Collider2D other)
        {
            RegisterPlayer(
                other,
                false);
        }

        private void RegisterPlayer(
            Collider2D other,
            bool inside)
        {
            if (other == null)
                return;

            Transform root =
                other.transform.root;

            if (root == null ||
                !root.CompareTag("Player"))
            {
                return;
            }

            if (inside)
                _playersInside.Add(root);
            else
                _playersInside.Remove(root);
        }

        private void Update()
        {
            if (roofRenderers == null ||
                _baseAlpha == null)
            {
                return;
            }

            float targetMultiplier =
                PlayerInside
                    ? insideAlpha
                    : 1f;

            float step =
                fadeSpeed *
                Time.deltaTime;

            for (int i = 0;
                 i < roofRenderers.Length;
                 i++)
            {
                SpriteRenderer sr =
                    roofRenderers[i];

                if (sr == null)
                    continue;

                Color color =
                    sr.color;

                float baseAlpha =
                    i < _baseAlpha.Length
                        ? _baseAlpha[i]
                        : 1f;

                color.a =
                    Mathf.MoveTowards(
                        color.a,
                        baseAlpha *
                        targetMultiplier,
                        step);

                sr.color =
                    color;
            }
        }

        private void ApplyImmediate(
            bool inside)
        {
            if (roofRenderers == null ||
                _baseAlpha == null)
            {
                return;
            }

            float multiplier =
                inside
                    ? insideAlpha
                    : 1f;

            for (int i = 0;
                 i < roofRenderers.Length;
                 i++)
            {
                SpriteRenderer sr =
                    roofRenderers[i];

                if (sr == null)
                    continue;

                Color color =
                    sr.color;

                color.a =
                    _baseAlpha[i] *
                    multiplier;

                sr.color =
                    color;
            }
        }
    }
}
