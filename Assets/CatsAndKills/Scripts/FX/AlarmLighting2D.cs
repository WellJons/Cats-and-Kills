using System.Collections.Generic;
using CatsAndKills.AI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.FX
{
    [DisallowMultipleComponent]
    public sealed class AlarmLighting2D : MonoBehaviour
    {
        [SerializeField] private float calmMultiplier = 0.55f;
        [SerializeField] private float alarmMultiplier = 1.45f;
        [SerializeField] private float pulseSpeed = 5.5f;
        [SerializeField] private float pulseAmount = 0.26f;

        private readonly List<Light2D> _redLights =
            new List<Light2D>();

        private readonly List<float> _base =
            new List<float>();

        private bool _lastAlarm;

        private void Awake()
        {
            Cache();
        }

        private void Cache()
        {
            _redLights.Clear();
            _base.Clear();

            foreach (Light2D light in
                     GetComponentsInChildren<Light2D>(true))
            {
                if (light == null ||
                    light.lightType ==
                    Light2D.LightType.Global)
                {
                    continue;
                }

                bool red =
                    light.color.r >
                    light.color.b * 1.4f &&
                    light.color.r >
                    light.color.g * 1.4f;

                if (!red)
                    continue;

                _redLights.Add(light);
                _base.Add(
                    Mathf.Max(
                        0.01f,
                        light.intensity));
            }
        }

        private void Update()
        {
            if (_redLights.Count == 0)
                Cache();

            bool alarm =
                FacilityAlarmDirector.Instance != null &&
                FacilityAlarmDirector.Instance.AlarmRaised;

            float pulse =
                alarm
                    ? 1f +
                      Mathf.Sin(
                          Time.unscaledTime *
                          pulseSpeed) *
                      pulseAmount
                    : 1f;

            float multiplier =
                (alarm
                    ? alarmMultiplier
                    : calmMultiplier) *
                pulse;

            for (int i = 0;
                 i < _redLights.Count;
                 i++)
            {
                Light2D light =
                    _redLights[i];

                if (light == null)
                    continue;

                light.intensity =
                    _base[i] *
                    multiplier;
            }

            _lastAlarm = alarm;
        }
    }
}
