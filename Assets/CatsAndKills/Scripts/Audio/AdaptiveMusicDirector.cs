using CatsAndKills.Core;
using UnityEngine;

namespace CatsAndKills.Audio
{
    public sealed class AdaptiveMusicDirector : MonoBehaviour
    {
        [SerializeField] private AudioClip ambient;
        [SerializeField] private AudioClip alert;
        [SerializeField] private AudioClip combat;
        [SerializeField] private float fadeSpeed = 1.3f;

        private AudioSource _ambientSource;
        private AudioSource _alertSource;
        private AudioSource _combatSource;

        public void Configure(AudioClip a, AudioClip al, AudioClip c)
        {
            ambient = a;
            alert = al;
            combat = c;
        }

        private void Start()
        {
            _ambientSource = CreateSource("Music Ambient", ambient);
            _alertSource = CreateSource("Music Alert", alert);
            _combatSource = CreateSource("Music Combat", combat);

            if (_ambientSource != null) _ambientSource.volume = 0.65f;
            if (_alertSource != null) _alertSource.volume = 0f;
            if (_combatSource != null) _combatSource.volume = 0f;
        }

        private AudioSource CreateSource(string sourceName, AudioClip clip)
        {
            if (clip == null) return null;
            var src = gameObject.AddComponent<AudioSource>();
            src.name = sourceName;
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.volume = 0f;
            src.Play();
            return src;
        }

        private void Update()
        {
            CombatIntensity intensity =
                CombatDirector.Instance != null
                    ? CombatDirector.Instance.Intensity
                    : CombatIntensity.Calm;

            float ambientTarget = intensity == CombatIntensity.Calm ? 0.62f : 0.20f;
            float alertTarget = intensity == CombatIntensity.Alert ? 0.62f : 0f;
            float combatTarget = intensity == CombatIntensity.Combat ? 0.78f : 0f;

            if (_ambientSource != null)
                _ambientSource.volume = Mathf.MoveTowards(
                    _ambientSource.volume,
                    ambientTarget,
                    fadeSpeed * Time.unscaledDeltaTime);

            if (_alertSource != null)
                _alertSource.volume = Mathf.MoveTowards(
                    _alertSource.volume,
                    alertTarget,
                    fadeSpeed * Time.unscaledDeltaTime);

            if (_combatSource != null)
                _combatSource.volume = Mathf.MoveTowards(
                    _combatSource.volume,
                    combatTarget,
                    fadeSpeed * Time.unscaledDeltaTime);
        }
    }
}
