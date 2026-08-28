using System.Collections;
using CatsAndKills.Core;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class CollarAbility : MonoBehaviour
    {
        [SerializeField] private float duration = 1.55f;
        [SerializeField] private float timeScale = 0.42f;
        [SerializeField] private float cooldown = 7f;
        [SerializeField] private AudioClip activationClip;

        private float _readyAt;
        private bool _active;

        public float Cooldown01 =>
            Mathf.Clamp01((_readyAt - Time.unscaledTime) / Mathf.Max(0.01f, cooldown));

        public bool IsActive => _active;

        public void Configure(AudioClip clip)
        {
            activationClip = clip;
        }

        private void Update()
        {
            if (CKInput.CollarPressed && !_active && Time.unscaledTime >= _readyAt)
                StartCoroutine(Activate());
        }

        private IEnumerator Activate()
        {
            _active = true;
            _readyAt = Time.unscaledTime + cooldown;

            if (activationClip != null)
                AudioSource.PlayClipAtPoint(activationClip, transform.position, 0.6f);

            HapticsManager.Instance?.Pulse(0.15f, 0.45f, 0.13f);
            PrototypeHUD.Instance?.SetGlitch(1f);

            float originalFixed = Time.fixedDeltaTime;
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = originalFixed * timeScale;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                PrototypeHUD.Instance?.SetGlitch(0.55f + Mathf.Sin(elapsed * 35f) * 0.22f);
                yield return null;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixed;
            PrototypeHUD.Instance?.SetGlitch(0f);
            _active = false;
        }

        private void OnDisable()
        {
            if (_active)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;
            }
        }
    }
}
