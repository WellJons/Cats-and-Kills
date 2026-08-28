using CatsAndKills.Audio;
using CatsAndKills.Tactical;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.World
{
    [DisallowMultipleComponent]
    public sealed class CityClubAmbience2D :
        MonoBehaviour
    {
        [SerializeField] private float maxDistance = 11f;
        [SerializeField] private float baseVolume = 0.42f;
        [SerializeField] private Light2D[] pulseLights;

        private AudioSource _audio;
        private float _phase;

        public void Configure(
            Light2D[] lights,
            float distance = 11f,
            float volume = 0.42f)
        {
            pulseLights = lights;
            maxDistance = distance;
            baseVolume = volume;
        }

        private void Start()
        {
            _audio =
                gameObject.AddComponent<
                    AudioSource>();

            _audio.clip =
                ProceduralAudioFactory.ClubMusic;

            _audio.loop = true;
            _audio.playOnAwake = false;
            _audio.spatialBlend = 1f;
            _audio.rolloffMode =
                AudioRolloffMode.Linear;
            _audio.minDistance = 1.6f;
            _audio.maxDistance =
                Mathf.Max(
                    3f,
                    maxDistance);
            _audio.volume = baseVolume;

            if (_audio.clip != null)
                _audio.Play();

            _phase =
                Random.Range(
                    0f,
                    Mathf.PI *
                    2f);
        }

        private void Update()
        {
            float beat =
                Time.unscaledTime *
                116f /
                60f;

            float pulse =
                Mathf.Pow(
                    Mathf.Max(
                        0f,
                        Mathf.Sin(
                            beat *
                            Mathf.PI *
                            2f +
                            _phase)),
                    8f);

            if (pulseLights != null)
            {
                for (int i = 0;
                     i < pulseLights.Length;
                     i++)
                {
                    Light2D light =
                        pulseLights[i];

                    if (light == null)
                        continue;

                    light.intensity =
                        0.55f +
                        pulse *
                        (i % 2 == 0
                            ? 0.85f
                            : 0.55f);
                }
            }

            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (_audio != null)
            {
                float target =
                    tactical != null &&
                    tactical.IsTacticalCombat
                        ? baseVolume * 0.22f
                        : baseVolume;

                _audio.volume =
                    Mathf.MoveTowards(
                        _audio.volume,
                        target,
                        Time.unscaledDeltaTime *
                        0.65f);
            }
        }
    }
}
