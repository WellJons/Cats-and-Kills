using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.FX
{
    [DisallowMultipleComponent]
    public sealed class TransientLight2D : MonoBehaviour
    {
        [SerializeField] private Light2D light2D;
        [SerializeField] private float duration = 0.18f;
        [SerializeField] private float peakIntensity = 2.0f;

        private float _started;

        public void Configure(
            Color color,
            float intensity,
            float radius,
            float lifetime)
        {
            light2D =
                GetComponent<Light2D>();

            if (light2D == null)
                light2D =
                    gameObject.AddComponent<Light2D>();

            light2D.lightType =
                Light2D.LightType.Point;

            light2D.color = color;
            light2D.intensity = intensity;
            light2D.pointLightInnerRadius =
                radius * 0.08f;

            light2D.pointLightOuterRadius =
                radius;

            light2D.falloffIntensity = 0.76f;
            light2D.overlapOperation =
                Light2D.OverlapOperation.Additive;

            light2D.targetSortingLayers =
                new[]
                {
                    SortingLayer.NameToID(
                        "Default")
                };

            peakIntensity = intensity;
            duration =
                Mathf.Max(
                    0.02f,
                    lifetime);

            _started =
                Time.unscaledTime;
        }

        private void Update()
        {
            if (light2D == null)
            {
                Destroy(this);
                return;
            }

            float t =
                Mathf.Clamp01(
                    (Time.unscaledTime -
                     _started) /
                    duration);

            light2D.intensity =
                peakIntensity *
                Mathf.Pow(
                    1f - t,
                    2.2f);

            if (t >= 1f)
            {
                light2D.enabled = false;
                Destroy(this);
            }
        }
    }
}
