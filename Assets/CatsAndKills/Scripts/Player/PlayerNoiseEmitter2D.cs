using CatsAndKills.Combat;
using CatsAndKills.Core;
using UnityEngine;

namespace CatsAndKills.Player
{
    public sealed class PlayerNoiseEmitter2D : MonoBehaviour
    {
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private float walkInterval = 0.48f;
        [SerializeField] private float sprintInterval = 0.30f;
        [SerializeField] private float walkRadius = 2.4f;
        [SerializeField] private float sprintRadius = 5.2f;

        private float _nextStep;

        private void Awake()
        {
            if (motor == null)
                motor = GetComponent<PlayerMotor2D>();
        }

        private void Update()
        {
            if (motor == null || motor.Velocity.sqrMagnitude < 0.35f)
                return;

            bool sprinting = CKInput.SprintHeld;
            float interval = sprinting ? sprintInterval : walkInterval;

            if (Time.time < _nextStep)
                return;

            _nextStep = Time.time + interval;

            NoiseSystem.Report(
                transform.position,
                sprinting ? sprintRadius : walkRadius,
                gameObject);
        }
    }
}
