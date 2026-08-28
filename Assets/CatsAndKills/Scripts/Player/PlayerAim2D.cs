using CatsAndKills.Combat;
using CatsAndKills.Core;
using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class PlayerAim2D : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform aimPivot;
        [SerializeField] private Transform bodyVisual;
        [SerializeField] private float bodyTurnSharpness = 16f;
        [SerializeField] private bool rotateBodyVisual = true;

        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public Vector2 AimWorldPoint { get; private set; }

        public void Configure(Camera cameraRef, Transform pivot, Transform body = null)
        {
            worldCamera = cameraRef;
            aimPivot = pivot;
            bodyVisual = body;
        }

        public void SetBodyRotationEnabled(bool enabled)
        {
            rotateBodyVisual = enabled;
        }

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (aimPivot == null) aimPivot = transform;
        }

        private void LateUpdate()
        {
            if (worldCamera == null) return;

            Vector2 stick = CKInput.AimStick;

            if (stick.sqrMagnitude > 0.08f)
            {
                AimDirection = stick.normalized;
                AimWorldPoint = (Vector2)transform.position + AimDirection * 8f;
            }
            else
            {
                Vector3 screen = CKInput.MouseScreenPosition;
                Vector3 world = worldCamera.ScreenToWorldPoint(
                    new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));

                AimWorldPoint = world;

                Vector2 visualAimOrigin =
                    CharacterCombatGeometry2D.AimPoint(
                        transform);

                Vector2 delta =
                    AimWorldPoint -
                    visualAimOrigin;

                if (delta.sqrMagnitude > 0.001f)
                    AimDirection = delta.normalized;
            }

            float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);

            if (bodyVisual != null && rotateBodyVisual)
            {
                float t = 1f - Mathf.Exp(-bodyTurnSharpness * Time.unscaledDeltaTime);
                bodyVisual.rotation = Quaternion.Lerp(
                    bodyVisual.rotation,
                    Quaternion.Euler(0f, 0f, angle),
                    t);
            }
        }
    }
}
