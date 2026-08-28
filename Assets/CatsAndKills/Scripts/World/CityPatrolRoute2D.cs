using CatsAndKills.AI;
using CatsAndKills.Narrative;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.World
{
    [DisallowMultipleComponent]
    public sealed class CityPatrolRoute2D :
        MonoBehaviour
    {
        [SerializeField] private EnemyMotor2D motor;
        [SerializeField] private WorldFactionMember2D faction;
        [SerializeField] private Vector2[] waypoints;
        [SerializeField] private float minWait = 0.8f;
        [SerializeField] private float maxWait = 2.2f;

        private int _index;
        private float _moveAt;

        public void Configure(
            EnemyMotor2D routeMotor,
            Vector2[] points)
        {
            motor = routeMotor;
            waypoints = points;

            FindClosestWaypoint();
            Schedule();
        }

        private void Awake()
        {
            if (motor == null)
                motor = GetComponent<EnemyMotor2D>();

            if (faction == null)
                faction =
                    GetComponent<
                        WorldFactionMember2D>();
        }

        private void Start()
        {
            FindClosestWaypoint();
            Schedule();
        }

        private void Update()
        {
            if (motor == null ||
                waypoints == null ||
                waypoints.Length < 2)
            {
                return;
            }

            if (NarrativeDialogueSystem.IsDialogueOpen)
            {
                motor.Stop();
                return;
            }

            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null &&
                tactical.IsTacticalCombat)
            {
                return;
            }

            if (faction == null)
                faction =
                    GetComponent<
                        WorldFactionMember2D>();

            if (faction != null &&
                faction.IsHostileToPlayer)
            {
                return;
            }

            if (motor.HasDestination)
                return;

            if (Time.time < _moveAt)
                return;

            Vector2 target =
                waypoints[
                    _index %
                    waypoints.Length];

            if (motor.MoveTo(target))
            {
                _index =
                    (_index + 1) %
                    waypoints.Length;
            }

            Schedule();
        }

        private void FindClosestWaypoint()
        {
            if (waypoints == null ||
                waypoints.Length == 0)
            {
                _index = 0;
                return;
            }

            float best =
                float.PositiveInfinity;

            int bestIndex = 0;

            for (int i = 0;
                 i < waypoints.Length;
                 i++)
            {
                float distance =
                    Vector2.SqrMagnitude(
                        (Vector2)transform.position -
                        waypoints[i]);

                if (distance < best)
                {
                    best = distance;
                    bestIndex = i;
                }
            }

            _index =
                (bestIndex + 1) %
                waypoints.Length;
        }

        private void Schedule()
        {
            _moveAt =
                Time.time +
                Random.Range(
                    Mathf.Min(
                        minWait,
                        maxWait),
                    Mathf.Max(
                        minWait,
                        maxWait));
        }
    }
}
