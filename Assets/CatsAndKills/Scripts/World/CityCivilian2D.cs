using CatsAndKills.AI;
using CatsAndKills.Narrative;
using CatsAndKills.Player;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.World
{
    [DisallowMultipleComponent]
    public sealed class CityCivilian2D :
        MonoBehaviour
    {
        [SerializeField] private NavigationGrid2D navigation;
        [SerializeField] private EnemyMotor2D motor;
        [SerializeField] private float wanderRadius = 4.5f;
        [SerializeField] private Vector2 idleRange =
            new Vector2(1.2f, 4.4f);

        private Vector2 _home;
        private float _nextMove;
        private bool _wasTactical;

        public void Configure(
            NavigationGrid2D nav,
            EnemyMotor2D civilianMotor,
            float radius)
        {
            navigation = nav;
            motor = civilianMotor;
            wanderRadius = radius;
            _home = transform.position;
        }

        private void Awake()
        {
            if (navigation == null)
                navigation =
                    FindAnyObjectByType<
                        NavigationGrid2D>();

            if (motor == null)
                motor =
                    GetComponent<EnemyMotor2D>();

            _home =
                transform.position;

            ScheduleMove();
        }

        private void Update()
        {
            if (NarrativeDialogueSystem.IsDialogueOpen)
            {
                motor?.Stop();
                return;
            }

            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            bool tacticalActive =
                tactical != null &&
                tactical.IsTacticalCombat;

            if (tacticalActive)
            {
                if (!_wasTactical)
                    FleeFromCombat();

                _wasTactical = true;
                return;
            }

            if (_wasTactical)
            {
                _wasTactical = false;
                ScheduleMove();
            }

            if (Time.time <
                _nextMove)
            {
                return;
            }

            if (motor != null &&
                motor.HasDestination)
            {
                return;
            }

            Wander();
        }

        private void Wander()
        {
            if (motor == null)
                return;

            Vector2 offset =
                Random.insideUnitCircle;

            if (offset.sqrMagnitude <
                0.05f)
            {
                offset = Vector2.right;
            }

            Vector2 destination =
                _home +
                offset.normalized *
                Random.Range(
                    1.0f,
                    wanderRadius);

            if (!motor.MoveTo(
                    destination))
            {
                ScheduleMove();
                return;
            }

            ScheduleMove();
        }

        private void FleeFromCombat()
        {
            if (motor == null)
                return;

            PlayerMotor2D player =
                FindAnyObjectByType<
                    PlayerMotor2D>();

            Vector2 away =
                player != null
                    ? (Vector2)transform.position -
                      (Vector2)player.transform.position
                    : Random.insideUnitCircle;

            if (away.sqrMagnitude <
                0.05f)
            {
                away = Vector2.right;
            }

            motor.MoveTo(
                (Vector2)transform.position +
                away.normalized *
                Random.Range(
                    5f,
                    9f));
        }

        private void ScheduleMove()
        {
            _nextMove =
                Time.time +
                Random.Range(
                    Mathf.Min(
                        idleRange.x,
                        idleRange.y),
                    Mathf.Max(
                        idleRange.x,
                        idleRange.y));
        }
    }
}
