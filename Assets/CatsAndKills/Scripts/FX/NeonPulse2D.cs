using UnityEngine;

namespace CatsAndKills.FX
{
    [DisallowMultipleComponent]
    public sealed class NeonPulse2D : MonoBehaviour
    {
        [SerializeField] private float speed = 1.4f;
        [SerializeField] private float amount = 0.16f;

        private SpriteRenderer _renderer;
        private Color _baseColor;
        private float _phase;

        public void Configure(float pulseSpeed, float pulseAmount)
        {
            speed = pulseSpeed;
            amount = pulseAmount;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();

            if (_renderer != null)
                _baseColor = _renderer.color;

            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (_renderer == null)
                return;

            float pulse =
                1f +
                Mathf.Sin(
                    Time.unscaledTime * speed +
                    _phase) *
                amount;

            Color c = _baseColor;
            c.r = Mathf.Clamp01(c.r * pulse);
            c.g = Mathf.Clamp01(c.g * pulse);
            c.b = Mathf.Clamp01(c.b * pulse);
            c.a = _baseColor.a;

            _renderer.color = c;
        }
    }
}
