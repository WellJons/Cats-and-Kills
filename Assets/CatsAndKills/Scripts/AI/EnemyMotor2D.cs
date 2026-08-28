using System.Collections.Generic;
using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyMotor2D : MonoBehaviour
    {
        [SerializeField] private NavigationGrid2D navigation;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private float moveSpeed = 3.25f;
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float waypointTolerance = 0.22f;

        private Rigidbody2D _rb;
        private List<Vector2> _path = new List<Vector2>();
        private int _pathIndex;
        private bool _hasDestination;

        public bool ReachedDestination => !_hasDestination;
        public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

        public void Configure(NavigationGrid2D nav, float speed = 3.25f)
        {
            navigation = nav;
            moveSpeed = speed;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (vitals == null) vitals = GetComponent<CharacterVitals>();
        }

        public void MoveTo(Vector2 destination)
        {
            if (navigation == null) return;
            _path = navigation.FindPath(transform.position, destination);
            _pathIndex = 0;
            _hasDestination = _path.Count > 0;
        }

        public void Stop()
        {
            _path.Clear();
            _pathIndex = 0;
            _hasDestination = false;
        }

        private void FixedUpdate()
        {
            float limbFactor = vitals != null ? vitals.MovementMultiplier : 1f;
            if (vitals != null && vitals.IsDead) limbFactor = 0f;

            if (!_hasDestination || _pathIndex >= _path.Count || limbFactor <= 0f)
            {
                _rb.linearVelocity = Vector2.MoveTowards(
                    _rb.linearVelocity,
                    Vector2.zero,
                    acceleration * Time.fixedDeltaTime);
                _hasDestination = false;
                return;
            }

            Vector2 target = _path[_pathIndex];
            Vector2 delta = target - (Vector2)transform.position;

            if (delta.magnitude <= waypointTolerance)
            {
                _pathIndex++;
                if (_pathIndex >= _path.Count)
                {
                    _hasDestination = false;
                    return;
                }

                target = _path[_pathIndex];
                delta = target - (Vector2)transform.position;
            }

            Vector2 desired = delta.normalized * moveSpeed * limbFactor;
            _rb.linearVelocity = Vector2.MoveTowards(
                _rb.linearVelocity,
                desired,
                acceleration * Time.fixedDeltaTime);
        }
    }
}
