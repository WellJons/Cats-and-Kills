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
        [SerializeField] private bool clampToWorldBounds;
        [SerializeField] private Vector2 worldMin = new Vector2(-23f, -14f);
        [SerializeField] private Vector2 worldMax = new Vector2(23f, 14f);
        [SerializeField] private Camera followCamera;

        private Vector3 _impulse;
        private float _impulseDecay = 18f;

        public void Configure(Transform newTarget, PlayerAim2D newAim)
        {
            target = newTarget;
            aim = newAim;

            if (followCamera == null)
                followCamera = GetComponent<Camera>();
        }

        public void ConfigureBounds(
            Vector2 min,
            Vector2 max,
            Camera cameraRef = null)
        {
            worldMin = min;
            worldMax = max;
            followCamera =
                cameraRef != null
                    ? cameraRef
                    : GetComponent<Camera>();
            clampToWorldBounds = true;
        }

        public void AddImpulse(Vector2 direction, float strength, float decay = 18f)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Random.insideUnitCircle;
            float shake = CatsAndKills.Core.GamePreferences.ScreenShake;
            _impulse += (Vector3)(direction.normalized * strength * shake);
            _impulse += (Vector3)(Random.insideUnitCircle * strength * 0.45f * shake);
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

            if (clampToWorldBounds)
            {
                Camera cam =
                    followCamera != null
                        ? followCamera
                        : GetComponent<Camera>();

                float halfHeight =
                    cam != null && cam.orthographic
                        ? cam.orthographicSize
                        : 0f;

                float halfWidth =
                    cam != null && cam.orthographic
                        ? halfHeight * cam.aspect
                        : 0f;

                float minX = worldMin.x + halfWidth;
                float maxX = worldMax.x - halfWidth;
                float minY = worldMin.y + halfHeight;
                float maxY = worldMax.y - halfHeight;

                desired.x =
                    minX <= maxX
                        ? Mathf.Clamp(desired.x, minX, maxX)
                        : (worldMin.x + worldMax.x) * 0.5f;

                desired.y =
                    minY <= maxY
                        ? Mathf.Clamp(desired.y, minY, maxY)
                        : (worldMin.y + worldMax.y) * 0.5f;
            }

            float t = 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
    }
}
