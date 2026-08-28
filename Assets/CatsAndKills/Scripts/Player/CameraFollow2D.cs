using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private float followSharpness = 10f;
        [SerializeField] private float aimLead = 1.4f;
        [SerializeField] private float maxLead = 2.5f;

        private Vector3 _impulse;
        private float _impulseDecay = 18f;

        public void Configure(Transform newTarget, PlayerAim2D newAim)
        {
            target = newTarget;
            aim = newAim;
        }

        public void AddImpulse(Vector2 direction, float strength, float decay = 18f)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Random.insideUnitCircle;
            _impulse += (Vector3)(direction.normalized * strength);
            _impulse += (Vector3)(Random.insideUnitCircle * strength * 0.45f);
            _impulseDecay = Mathf.Max(1f, decay);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 lead = Vector3.zero;
            if (aim != null)
            {
                Vector2 delta = aim.AimWorldPoint - (Vector2)target.position;
                if (delta.magnitude > maxLead) delta = delta.normalized * maxLead;
                lead = delta * aimLead / Mathf.Max(maxLead, 0.001f);
            }

            _impulse = Vector3.Lerp(
                _impulse,
                Vector3.zero,
                1f - Mathf.Exp(-_impulseDecay * Time.unscaledDeltaTime));

            Vector3 desired = target.position + lead + _impulse;
            desired.z = transform.position.z;

            float t = 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
    }
}
