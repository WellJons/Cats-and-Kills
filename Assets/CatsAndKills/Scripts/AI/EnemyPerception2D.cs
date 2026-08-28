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
        private Vector2 _facing = Vector2.right;

        public bool HasRecentNoise => Time.time - _heardAt < 2.5f;
        public Vector2 HeardPosition => _heardPosition;

        public void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
                _facing = direction.normalized;
        }

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

            Vector2 origin =
                CharacterCombatGeometry2D.AimPoint(transform);

            Vector2 targetPoint =
                CharacterCombatGeometry2D.AimPoint(target);

            Vector2 delta = targetPoint - origin;
            float distance = delta.magnitude;

            if (distance > viewDistance || distance < 0.001f)
                return false;

            if (Vector2.Angle(_facing, delta) > viewAngle * 0.5f)
                return false;

            Vector2 direction = delta / distance;
            Vector2 rayOrigin = origin + direction * 0.12f;

            RaycastHit2D block = Physics2D.Raycast(
                rayOrigin,
                direction,
                Mathf.Max(0f, distance - 0.20f),
                obstacleMask);

            return block.collider == null;
        }
    }
}
