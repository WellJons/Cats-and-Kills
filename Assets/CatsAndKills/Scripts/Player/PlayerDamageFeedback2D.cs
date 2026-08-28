using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class PlayerDamageFeedback2D : MonoBehaviour
    {
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private CameraFollow2D cameraFollow;

        public void Configure(
            CharacterVitals characterVitals,
            CameraFollow2D camera)
        {
            vitals = characterVitals;
            cameraFollow = camera;
        }

        private void OnEnable()
        {
            if (vitals == null)
                vitals = GetComponent<CharacterVitals>();

            if (vitals != null)
                vitals.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            if (vitals != null)
                vitals.Damaged -= OnDamaged;
        }

        private void OnDamaged(DamageInfo info)
        {
            if (vitals == null || vitals.IsDead)
                return;

            float severity =
                Mathf.Clamp01(
                    info.Amount /
                    Mathf.Max(1f, vitals.MaxHealth));

            cameraFollow?.AddImpulse(
                info.Direction.sqrMagnitude > 0.01f
                    ? info.Direction
                    : Random.insideUnitCircle,
                Mathf.Lerp(0.06f, 0.22f, severity),
                14f);

            HapticsManager.Instance?.Pulse(
                Mathf.Lerp(0.16f, 0.55f, severity),
                Mathf.Lerp(0.10f, 0.40f, severity),
                Mathf.Lerp(0.08f, 0.18f, severity));

            PrototypeHUD.Instance?.FlashDamage(
                Mathf.Lerp(0.35f, 1f, severity));
        }
    }
}
