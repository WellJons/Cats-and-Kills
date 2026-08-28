using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class LightFlicker2D : MonoBehaviour
    {
        [SerializeField] private Light2D targetLight;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private float baseIntensity = 1f;
        [SerializeField, Range(0f, 1f)] private float flickerAmount = 0.28f;
        [SerializeField] private float noiseSpeed = 7f;
        [SerializeField] private float hardFlickerChance = 0.045f;
        [SerializeField] private Vector2 hardFlickerDuration = new Vector2(0.035f, 0.16f);

        private float _seed;
        private float _hardFlickerUntil;
        private float _baseGlowAlpha = 1f;

        public void Configure(
            Light2D lightSource,
            SpriteRenderer glow,
            float intensity,
            float amount)
        {
            targetLight = lightSource;
            glowRenderer = glow;
            baseIntensity =
                Mathf.Max(
                    0f,
                    intensity);
            flickerAmount =
                Mathf.Clamp01(
                    amount);

            if (glowRenderer != null)
                _baseGlowAlpha = glowRenderer.color.a;
        }

        private void Awake()
        {
            _seed =
                Random.Range(
                    0f,
                    1000f);

            if (targetLight != null &&
                baseIntensity <= 0.001f)
            {
                baseIntensity =
                    targetLight.intensity;
            }

            if (glowRenderer != null)
                _baseGlowAlpha = glowRenderer.color.a;
        }

        private void Update()
        {
            if (Random.value <
                hardFlickerChance *
                Time.deltaTime)
            {
                _hardFlickerUntil =
                    Time.time +
                    Random.Range(
                        hardFlickerDuration.x,
                        hardFlickerDuration.y);
            }

            float value;

            if (Time.time <
                _hardFlickerUntil)
            {
                value =
                    Random.value >
                    0.45f
                        ? 0.08f
                        : 0.45f;
            }
            else
            {
                float noise =
                    Mathf.PerlinNoise(
                        _seed,
                        Time.time *
                        noiseSpeed);

                value =
                    Mathf.Lerp(
                        1f -
                        flickerAmount,
                        1f,
                        noise);
            }

            if (targetLight != null)
            {
                targetLight.intensity =
                    baseIntensity *
                    value;
            }

            if (glowRenderer != null)
            {
                Color color =
                    glowRenderer.color;

                color.a =
                    _baseGlowAlpha *
                    Mathf.Lerp(
                        0.45f,
                        1f,
                        value);

                glowRenderer.color =
                    color;
            }
        }
    }
}
