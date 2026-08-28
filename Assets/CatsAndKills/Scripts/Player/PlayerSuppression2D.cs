using CatsAndKills.Combat;
using CatsAndKills.Core;
using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class PlayerSuppression2D : MonoBehaviour
    {
        [SerializeField] private float nearMissRadius = 1.25f;
        [SerializeField] private float decayPerSecond = 0.42f;
        [SerializeField] private CameraFollow2D cameraFollow;

        public float Suppression { get; private set; }
        public bool IsSuppressed => Suppression >= 0.28f;

        private float _nextFeedback;

        public void Configure(CameraFollow2D camera)
        {
            cameraFollow = camera;
        }

        private void OnEnable()
        {
            SuppressionSystem.ShotPassed += OnShotPassed;
        }

        private void OnDisable()
        {
            SuppressionSystem.ShotPassed -= OnShotPassed;
        }

        private void Update()
        {
            Suppression = Mathf.MoveTowards(
                Suppression,
                0f,
                decayPerSecond * Time.deltaTime);
        }

        private void OnShotPassed(SuppressionEvent evt)
        {
            if (evt.Source != null &&
                evt.Source.transform.root == transform.root)
                return;

            float distance = SuppressionSystem.DistanceToSegment(
                transform.position,
                evt.Start,
                evt.End);

            if (distance > nearMissRadius)
                return;

            float proximity = 1f - Mathf.Clamp01(distance / nearMissRadius);
            Suppression = Mathf.Clamp01(
                Suppression + evt.Strength * Mathf.Lerp(0.25f, 0.95f, proximity));

            if (Time.unscaledTime >= _nextFeedback)
            {
                _nextFeedback = Time.unscaledTime + 0.08f;

                cameraFollow?.AddImpulse(
                    Random.insideUnitCircle,
                    Mathf.Lerp(0.015f, 0.055f, Suppression),
                    20f);

                HapticsManager.Instance?.Pulse(
                    0.04f,
                    Mathf.Lerp(0.03f, 0.11f, Suppression),
                    0.035f);
            }
        }
    }
}
