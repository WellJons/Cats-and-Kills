using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyPatrol2D : MonoBehaviour
    {
        [SerializeField] private EnemyMotor2D motor;
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private float radius = 3.5f;
        [SerializeField] private float minWait = 1.2f;
        [SerializeField] private float maxWait = 3.4f;

        private Vector2 _origin;
        private float _moveAt;

        public void Configure(
            EnemyMotor2D enemyMotor,
            EnemyBrain enemyBrain,
            float patrolRadius)
        {
            motor = enemyMotor;
            brain = enemyBrain;
            radius = patrolRadius;
        }

        private void Start()
        {
            _origin = transform.position;
            _moveAt = Time.time + Random.Range(minWait, maxWait);
        }

        private void Update()
        {
            if (motor == null || brain == null)
                return;

            if (brain.IsAlerted)
                return;

            if (!motor.ReachedDestination)
                return;

            if (Time.time < _moveAt)
                return;

            Vector2 offset = Random.insideUnitCircle * radius;
            motor.MoveTo(_origin + offset);

            _moveAt =
                Time.time +
                Random.Range(minWait, maxWait);
        }
    }
}
