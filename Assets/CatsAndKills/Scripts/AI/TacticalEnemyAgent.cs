using System.Collections;
using CatsAndKills.Damage;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.AI
{
    [DisallowMultipleComponent]
    public sealed class TacticalEnemyAgent : MonoBehaviour
    {
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private EnemyMotor2D motor;
        [SerializeField] private EnemyPerception2D perception;
        [SerializeField] private EnemyWeapon2D weapon;
        [SerializeField] private EnemyGrenadeThrower grenades;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private EnemyPatrol2D patrol;
        [SerializeField] private Transform player;

        private bool _participating;

        public bool IsAlive =>
            vitals == null ||
            !vitals.IsDead;

        public bool IsAlerted =>
            brain != null &&
            brain.IsAlerted;

        private void Awake()
        {
            if (brain == null)
                brain = GetComponent<EnemyBrain>();

            if (motor == null)
                motor = GetComponent<EnemyMotor2D>();

            if (perception == null)
                perception = GetComponent<EnemyPerception2D>();

            if (weapon == null)
                weapon = GetComponent<EnemyWeapon2D>();

            if (grenades == null)
                grenades = GetComponent<EnemyGrenadeThrower>();

            if (vitals == null)
                vitals = GetComponent<CharacterVitals>();

            if (patrol == null)
                patrol = GetComponent<EnemyPatrol2D>();

            if (player == null)
            {
                Player.PlayerMotor2D playerMotor =
                    FindAnyObjectByType<Player.PlayerMotor2D>();

                if (playerMotor != null)
                    player = playerMotor.transform;
            }
        }

        private void OnEnable()
        {
            TacticalCombatDirector.Instance?.RegisterEnemy(this);
        }

        private void Start()
        {
            TacticalCombatDirector.Instance?.RegisterEnemy(this);
        }

        private void OnDisable()
        {
            TacticalCombatDirector.Instance?.UnregisterEnemy(this);
        }

        public void SetTacticalParticipation(
            bool participating)
        {
            _participating = participating;

            if (participating)
            {
                motor?.Stop();
                weapon?.SetTrigger(false);

                if (patrol != null)
                    patrol.enabled = false;
            }
            else if (patrol != null &&
                     IsAlive)
            {
                patrol.enabled = true;
            }
        }

        public IEnumerator TakeTurn(
            int actionPoints,
            NavigationGrid2D navigation)
        {
            if (!_participating ||
                !IsAlive ||
                player == null)
            {
                yield break;
            }

            weapon?.SetTrigger(false);

            int ap = actionPoints;

            bool seesPlayer =
                perception != null &&
                perception.CanSee(player);

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            if (seesPlayer &&
                distance >= 4.0f &&
                distance <= 8.0f &&
                grenades != null &&
                ap >= 4 &&
                Random.value < 0.16f)
            {
                if (grenades.TryThrow(
                        true,
                        0.65f))
                {
                    ap -= 4;
                    yield return new WaitForSeconds(0.45f);
                }
            }

            seesPlayer =
                perception != null &&
                perception.CanSee(player);

            distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            if (seesPlayer &&
                distance <= 9.5f &&
                ap >= 3)
            {
                if (weapon != null &&
                    weapon.TryTacticalFire())
                {
                    ap -= 3;
                    yield return new WaitForSeconds(0.32f);
                }
            }

            if (ap > 0 &&
                navigation != null &&
                motor != null)
            {
                var path =
                    navigation.FindPath(
                        transform.position,
                        player.position);

                int maxSteps =
                    Mathf.Min(
                        ap,
                        3);

                if (path.Count > 0 &&
                    maxSteps > 0)
                {
                    int stepIndex =
                        Mathf.Min(
                            maxSteps - 1,
                            path.Count - 1);

                    Vector2 destination =
                        path[stepIndex];

                    if (motor.MoveTo(destination))
                    {
                        float timeout =
                            Time.time + 2.5f;

                        while (motor.HasDestination &&
                               Time.time < timeout &&
                               IsAlive)
                        {
                            yield return null;
                        }

                        ap -= stepIndex + 1;
                    }
                }
            }

            seesPlayer =
                perception != null &&
                perception.CanSee(player);

            distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            if (seesPlayer &&
                distance <= 9.5f &&
                ap >= 3)
            {
                if (weapon != null &&
                    weapon.TryTacticalFire())
                {
                    ap -= 3;
                    yield return new WaitForSeconds(0.32f);
                }
            }

            motor?.Stop();

            yield return new WaitForSeconds(0.12f);
        }
    }
}
