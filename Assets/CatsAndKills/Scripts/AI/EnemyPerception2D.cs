using CatsAndKills.Combat;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyPerception2D : MonoBehaviour
    {
        [SerializeField] private float viewDistance = 13f;
        [SerializeField, Range(1f, 360f)] private float viewAngle = 150f;
        [SerializeField] private float hearingMultiplier = 1f;
        [SerializeField] private LayerMask obstacleMask;

        private Vector2 _heardPosition;
        private float _heardAt = -999f;

        public bool HasRecentNoise => Time.time - _heardAt < 2.5f;
        public Vector2 HeardPosition => _heardPosition;

        public void Configure(
            LayerMask mask,
            float distance = 13f,
            float angle = 150f,
            float hearing = 1f)
        {
            obstacleMask = mask;
            viewDistance = distance;
            viewAngle = angle;
            hearingMultiplier = hearing;
        }

        private void OnEnable()
        {
            NoiseSystem.Noise += OnNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.Noise -= OnNoise;
        }

        private void OnNoise(NoiseEvent evt)
        {
            if (evt.Source == gameObject) return;

            float distance = Vector2.Distance(transform.position, evt.Position);
            if (distance <= evt.Radius * hearingMultiplier)
            {
                _heardPosition = evt.Position;
                _heardAt = Time.time;
            }
        }

        public bool CanSee(Transform target)
        {
            if (target == null) return false;

            Vector2 origin = transform.position;
            Vector2 delta = (Vector2)target.position - origin;
            float distance = delta.magnitude;

            if (distance > viewDistance || distance < 0.001f)
                return false;

            if (Vector2.Angle(transform.right, delta) > viewAngle * 0.5f)
                return false;

            Vector2 direction = delta / distance;
            Vector2 rayOrigin = origin + direction * 0.52f;

            RaycastHit2D block = Physics2D.Raycast(
                rayOrigin,
                direction,
                Mathf.Max(0f, distance - 0.55f),
                obstacleMask);

            return block.collider == null;
        }
    }
}
