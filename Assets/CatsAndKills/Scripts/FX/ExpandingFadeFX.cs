using UnityEngine;

namespace CatsAndKills.FX
{
    public sealed class ExpandingFadeFX : MonoBehaviour
    {
        private float _duration = 0.5f;
        private float _startScale = 1f;
        private float _endScale = 2f;
        private float _elapsed;
        private SpriteRenderer _renderer;

        public void Configure(float duration, float startScale, float endScale)
        {
            _duration = duration;
            _startScale = startScale;
            _endScale = endScale;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, _duration));

            float scale = Mathf.Lerp(_startScale, _endScale, t);
            transform.localScale = Vector3.one * scale;

            if (_renderer != null)
            {
                Color c = _renderer.color;
                c.a = Mathf.Lerp(c.a, 0f, t);
                _renderer.color = c;
            }

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
