using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.FX
{
    [DisallowMultipleComponent]
    public sealed class NeonLightFlicker2D : MonoBehaviour
    {
        [SerializeField] private Light2D light2D;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private float baseIntensity = 1.0f;
        [SerializeField] private float flickerAmount = 0.12f;
        [SerializeField] private float flickerSpeed = 11f;
        [SerializeField] private float pulseSpeed = 1.8f;
        [SerializeField] private float pulseAmount = 0.08f;
        [SerializeField] private bool occasionalDropout;

        private float _phase;
        private float _nextDropout;
        private float _dropoutUntil;
        private float _baseAlpha = 1f;

        public void Configure(
            Light2D light,
            SpriteRenderer glow,
            float intensity,
            float flicker,
            float speed,
            bool dropout = false)
        {
            light2D = light;
            glowRenderer = glow;
            baseIntensity = intensity;
            flickerAmount = flicker;
            flickerSpeed = speed;
            occasionalDropout = dropout;

            if (glowRenderer != null)
                _baseAlpha = glowRenderer.color.a;
        }

        private void Awake()
        {
            if (light2D == null)
                light2D = GetComponent<Light2D>();

            if (glowRenderer == null)
                glowRenderer = GetComponentInChildren<SpriteRenderer>();

            if (glowRenderer != null)
                _baseAlpha = glowRenderer.color.a;

            _phase = Random.Range(0f, Mathf.PI * 2f);
            _nextDropout = Time.unscaledTime + Random.Range(3f, 10f);
        }

        private void Update()
        {
            float now = Time.unscaledTime;

            if (occasionalDropout &&
                now >= _nextDropout)
            {
                _dropoutUntil =
                    now +
                    Random.Range(0.035f, 0.12f);

                _nextDropout =
                    now +
                    Random.Range(4f, 13f);
            }

            float noise =
                Mathf.PerlinNoise(
                    _phase,
                    now * flickerSpeed) *
                2f -
                1f;

            float pulse =
                Mathf.Sin(
                    now * pulseSpeed +
                    _phase) *
                pulseAmount;

            float multiplier =
                Mathf.Max(
                    0f,
                    1f +
                    noise * flickerAmount +
                    pulse);

            if (now < _dropoutUntil)
                multiplier *= 0.10f;

            if (light2D != null)
                light2D.intensity =
                    baseIntensity *
                    multiplier;

            if (glowRenderer != null)
            {
                Color c =
                    glowRenderer.color;

                c.a =
                    Mathf.Clamp01(
                        _baseAlpha *
                        Mathf.Lerp(
                            0.72f,
                            1.18f,
                            Mathf.Clamp01(multiplier)));

                glowRenderer.color = c;
            }
        }
    }
}
