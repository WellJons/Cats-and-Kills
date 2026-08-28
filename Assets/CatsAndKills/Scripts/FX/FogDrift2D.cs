using UnityEngine;

namespace CatsAndKills.FX
{
    [DisallowMultipleComponent]
    public sealed class FogDrift2D : MonoBehaviour
    {
        [SerializeField] private Vector2 drift = new Vector2(0.16f, 0.03f);
        [SerializeField] private float swayAmplitude = 0.18f;
        [SerializeField] private float swaySpeed = 0.45f;
        [SerializeField] private float alphaPulse = 0.05f;
        [SerializeField] private float alphaSpeed = 0.35f;

        private Vector3 _origin;
        private SpriteRenderer _renderer;
        private float _baseAlpha;
        private float _phase;

        public void Configure(
            Vector2 velocity,
            float sway,
            float pulse)
        {
            drift = velocity;
            swayAmplitude = sway;
            alphaPulse = pulse;
        }

        private void Awake()
        {
            _origin = transform.position;
            _renderer = GetComponent<SpriteRenderer>();

            if (_renderer != null)
                _baseAlpha = _renderer.color.a;

            _phase = Random.Range(0f, 10f);
        }

        private void Update()
        {
            float t = Time.unscaledTime + _phase;

            Vector3 offset =
                (Vector3)(drift * t) +
                Vector3.up *
                Mathf.Sin(t * swaySpeed) *
                swayAmplitude;

            transform.position = _origin + offset;

            if (_renderer == null)
                return;

            Color c = _renderer.color;

            c.a =
                Mathf.Clamp01(
                    _baseAlpha +
                    Mathf.Sin(t * alphaSpeed) *
                    alphaPulse);

            _renderer.color = c;
        }
    }
}
