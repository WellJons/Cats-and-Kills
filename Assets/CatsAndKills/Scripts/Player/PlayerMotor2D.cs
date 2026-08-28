using System.Collections;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5.3f;
        [SerializeField] private float sprintSpeed = 7.1f;
        [SerializeField] private float acceleration = 46f;
        [SerializeField] private float deceleration = 58f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 12.5f;
        [SerializeField] private float dashDuration = 0.11f;
        [SerializeField] private float dashCooldown = 0.8f;

        [SerializeField] private CharacterVitals vitals;

        private Rigidbody2D _rb;
        private Vector2 _desiredVelocity;
        private Vector2 _dashDirection;
        private bool _dashing;
        private float _nextDash;

        public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (vitals == null) vitals = GetComponent<CharacterVitals>();
        }

        private void Update()
        {
            if (vitals != null && vitals.IsDead) return;

            if (CKInput.DashPressed && Time.time >= _nextDash && CKInput.Move.sqrMagnitude > 0.05f)
                StartCoroutine(DashRoutine());

            if (_dashing) return;

            float limbFactor = vitals != null ? vitals.MovementMultiplier : 1f;
            float speed = CKInput.SprintHeld ? sprintSpeed : moveSpeed;
            _desiredVelocity = CKInput.Move * speed * limbFactor;
        }

        private void FixedUpdate()
        {
            if (vitals != null && vitals.IsDead)
            {
                _desiredVelocity = Vector2.zero;
                _rb.linearVelocity = Vector2.MoveTowards(
                    _rb.linearVelocity,
                    Vector2.zero,
                    deceleration * Time.fixedDeltaTime);
                return;
            }

            if (_dashing)
            {
                _rb.linearVelocity = _dashDirection * dashSpeed;
                return;
            }

            float rate = _desiredVelocity.sqrMagnitude > _rb.linearVelocity.sqrMagnitude
                ? acceleration
                : deceleration;

            _rb.linearVelocity = Vector2.MoveTowards(
                _rb.linearVelocity,
                _desiredVelocity,
                rate * Time.fixedDeltaTime);
        }

        private IEnumerator DashRoutine()
        {
            _dashing = true;
            _nextDash = Time.time + dashCooldown;
            _dashDirection = CKInput.Move.normalized;
            HapticsManager.Instance?.Pulse(0.18f, 0.08f, 0.08f);

            yield return new WaitForSeconds(dashDuration);
            _dashing = false;
        }
    }
}
