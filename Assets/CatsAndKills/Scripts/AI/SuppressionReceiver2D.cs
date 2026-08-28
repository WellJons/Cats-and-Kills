using CatsAndKills.Combat;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class SuppressionReceiver2D : MonoBehaviour
    {
        [SerializeField] private float nearMissRadius = 1.35f;
        [SerializeField] private float decayPerSecond = 0.34f;
        [SerializeField] private float maxSuppression = 1f;

        public float Suppression { get; private set; }
        public bool IsPinned => Suppression >= 0.72f;
        public bool IsUnderFire => Suppression >= 0.22f;

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
            if (evt.Source == gameObject) return;

            float distance = SuppressionSystem.DistanceToSegment(
                transform.position,
                evt.Start,
                evt.End);

            if (distance > nearMissRadius) return;

            float proximity = 1f - Mathf.Clamp01(distance / nearMissRadius);
            Suppression = Mathf.Clamp(
                Suppression + evt.Strength * Mathf.Lerp(0.35f, 1f, proximity),
                0f,
                maxSuppression);
        }

        public void AddSuppression(float value)
        {
            Suppression = Mathf.Clamp(Suppression + value, 0f, maxSuppression);
        }
    }
}
