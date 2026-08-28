using System.Collections;
using CatsAndKills.Audio;
using CatsAndKills.Core;
using CatsAndKills.Narrative;
using CatsAndKills.Tactical;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class CollarAbility : MonoBehaviour
    {
        [Header("Broken Collar // Protocol Fracture")]
        [SerializeField] private int bonusActionPoints = 4;
        [SerializeField] private int cooldownRounds = 2;
        [SerializeField] private float instabilityPerUse = 28f;
        [SerializeField] private float instabilityRecoveryPerRound = 7f;
        [SerializeField] private AudioClip activationClip;

        private float _instability;
        private int _readyRound = 1;
        private int _observedRound;
        private bool _active;

        public bool IsActive => _active;

        public bool IsUnlocked =>
            NarrativeWorldState.Instance != null &&
            NarrativeWorldState.Instance.HasFlag(
                "collar_protocol_unlocked");

        public float Instability01 =>
            Mathf.Clamp01(
                _instability / 100f);

        public float Instability =>
            _instability;

        public string TacticalAbilityName =>
            IsUnlocked
                ? "НЕСТАБИЛЬНЫЙ ПРОТОКОЛ"
                : "ПРОТОКОЛ НЕДОСТУПЕН";

        public float Cooldown01
        {
            get
            {
                TacticalCombatDirector tactical =
                    TacticalCombatDirector.Instance;

                if (tactical == null ||
                    !tactical.IsTacticalCombat)
                {
                    return 0f;
                }

                int remaining =
                    Mathf.Max(
                        0,
                        _readyRound -
                        tactical.RoundIndex);

                return cooldownRounds > 0
                    ? Mathf.Clamp01(
                        remaining /
                        (float)cooldownRounds)
                    : 0f;
            }
        }

        public void Configure(
            AudioClip clip)
        {
            activationClip = clip;
        }

        private void Update()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical == null ||
                !tactical.IsTacticalCombat)
            {
                return;
            }

            if (tactical.RoundIndex !=
                _observedRound)
            {
                if (_observedRound > 0)
                {
                    _instability =
                        Mathf.Max(
                            0f,
                            _instability -
                            instabilityRecoveryPerRound);
                }

                _observedRound =
                    tactical.RoundIndex;
            }

            if (!IsUnlocked ||
                !CKInput.CollarPressed ||
                _active ||
                !tactical.IsPlayerTurn ||
                tactical.RoundIndex <
                _readyRound)
            {
                return;
            }

            StartCoroutine(
                ActivateTactical(
                    tactical));
        }

        private IEnumerator ActivateTactical(
            TacticalCombatDirector tactical)
        {
            _active = true;

            _readyRound =
                tactical.RoundIndex +
                Mathf.Max(
                    1,
                    cooldownRounds);

            _instability =
                Mathf.Clamp(
                    _instability +
                    instabilityPerUse,
                    0f,
                    100f);

            tactical.GrantActionPoints(
                bonusActionPoints);

            AudioClip resolvedClip =
                activationClip != null
                    ? activationClip
                    : ProceduralAudioFactory.Collar;

            if (resolvedClip != null)
            {
                AudioSource.PlayClipAtPoint(
                    resolvedClip,
                    transform.position,
                    0.7f);
            }

            HapticsManager.Instance?.Pulse(
                0.22f,
                0.65f,
                0.18f);

            PrototypeHUD.Instance?.SetGlitch(
                1f);

            RadioDialogueSystem.Instance
                ?.ShowTransient(
                    "ОШЕЙНИК // ПРОТОКОЛ РАЗОРВАН // +4 AP",
                    1.1f);

            float elapsed = 0f;

            while (elapsed < 0.72f)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                PrototypeHUD.Instance?.SetGlitch(
                    Mathf.Lerp(
                        1f,
                        0f,
                        elapsed / 0.72f) +
                    Mathf.Sin(
                        elapsed * 42f) *
                    0.12f);

                yield return null;
            }

            PrototypeHUD.Instance?.SetGlitch(
                0f);

            _active = false;
        }

        private void OnDisable()
        {
            PrototypeHUD.Instance?.SetGlitch(
                0f);

            _active = false;
        }
    }
}
