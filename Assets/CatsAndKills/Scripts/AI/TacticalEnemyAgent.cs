using System.Collections;
using System.Collections.Generic;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using CatsAndKills.Tactical;
using CatsAndKills.UI;
using CatsAndKills.World;
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
        [SerializeField] private GrenadeAwareness2D grenadeAwareness;
        [SerializeField] private DemolitionistCharge2D demolitionCharge;
        [SerializeField] private SuppressionReceiver2D suppression;
        [SerializeField] private CoverManager coverManager;
        [SerializeField] private Transform player;
        [SerializeField] private WorldFactionMember2D factionMember;

        private bool _participating;
        private bool _realtimeSuspended;
        private bool _hasLastKnown;
        private Vector2 _lastKnownPlayerPosition;

        public bool IsAlive =>
            vitals == null ||
            !vitals.IsDead;

        public bool IsHostileToPlayer =>
            factionMember == null ||
            factionMember.IsHostileToPlayer;

        public bool IsAlerted =>
            IsHostileToPlayer &&
            brain != null &&
            brain.IsAlerted;

        private void Awake()
        {
            CacheReferences();
        }

        private void CacheReferences()
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

            if (grenadeAwareness == null)
                grenadeAwareness = GetComponent<GrenadeAwareness2D>();

            if (demolitionCharge == null)
                demolitionCharge = GetComponent<DemolitionistCharge2D>();

            if (suppression == null)
                suppression = GetComponent<SuppressionReceiver2D>();

            if (coverManager == null)
                coverManager = FindAnyObjectByType<CoverManager>();

            if (factionMember == null)
                factionMember = GetComponent<WorldFactionMember2D>();

            if (player == null)
            {
                PlayerMotor2D playerMotor =
                    FindAnyObjectByType<PlayerMotor2D>();

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
            CacheReferences();
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

            SetRealtimeSuspended(
                participating ||
                _realtimeSuspended);
        }

        public void SetRealtimeSuspended(
            bool suspended)
        {
            _realtimeSuspended = suspended;

            if (suspended)
            {
                motor?.Stop();
                weapon?.SetTrigger(false);

                if (patrol != null)
                    patrol.enabled = false;

                if (grenadeAwareness != null)
                    grenadeAwareness.enabled = false;

                if (demolitionCharge != null)
                    demolitionCharge.enabled = false;
            }
            else if (IsAlive &&
                     !_participating)
            {
                if (patrol != null)
                    patrol.enabled = true;

                if (grenadeAwareness != null)
                    grenadeAwareness.enabled = true;

                if (demolitionCharge != null)
                    demolitionCharge.enabled = true;
            }
        }

        public IEnumerator TakeTurn(
            int actionPoints,
            NavigationGrid2D navigation)
        {
            CacheReferences();

            if (!_participating ||
                !IsAlive ||
                !IsHostileToPlayer ||
                player == null ||
                navigation == null)
            {
                yield break;
            }

            weapon?.SetTrigger(false);
            motor?.Stop();

            int ap = actionPoints;

            bool seesPlayer =
                CanSeePlayer();

            if (seesPlayer)
            {
                RememberPlayer();
            }

            // Fire is a hard tactical constraint. Units already standing in it
            // spend their first available movement escaping it.
            if (ap > 0 &&
                TacticalFireField2D.IsDangerousPoint(
                    transform.position))
            {
                List<Vector2> escape =
                    FindNearestSafePath(
                        navigation);

                int steps =
                    Mathf.Min(
                        ap,
                        Mathf.Min(
                            3,
                            escape.Count));

                if (steps > 0)
                {
                    Callout("ОГОНЬ! ОТХОЖУ!");

                    yield return MoveAlongPath(
                        escape,
                        steps);

                    ap -= steps;
                    seesPlayer = CanSeePlayer();

                    if (seesPlayer)
                        RememberPlayer();
                }
            }

            bool badlyHurt =
                vitals != null &&
                vitals.Health <=
                vitals.MaxHealth * 0.34f;

            bool pinned =
                suppression != null &&
                suppression.IsPinned;

            // Hurt or pinned soldiers value survival over damage.
            if (ap > 0 &&
                (badlyHurt || pinned))
            {
                List<Vector2> coverPath =
                    FindCoverPath(
                        navigation);

                int steps =
                    Mathf.Min(
                        ap,
                        Mathf.Min(
                            3,
                            coverPath.Count));

                if (steps > 0)
                {
                    Callout(
                        badlyHurt
                            ? "РАНЕН! В УКРЫТИЕ!"
                            : "ПРИЖАЛИ! В УКРЫТИЕ!");

                    yield return MoveAlongPath(
                        coverPath,
                        steps);

                    ap -= steps;
                    seesPlayer = CanSeePlayer();

                    if (seesPlayer)
                        RememberPlayer();
                }
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            EnemyArchetype archetype =
                brain != null
                    ? brain.Archetype
                    : EnemyArchetype.Rifleman;

            SquadRole role =
                brain != null
                    ? brain.Role
                    : SquadRole.Assault;

            // Demolitionists use grenades to deny positions, not randomly every
            // turn. They prefer medium range and do not throw into friendlies.
            if (seesPlayer &&
                grenades != null &&
                ap >= 4 &&
                distance >= 4f &&
                distance <= 8.2f &&
                (archetype == EnemyArchetype.Demolitionist ||
                 role == SquadRole.Suppress) &&
                Random.value <
                (archetype == EnemyArchetype.Demolitionist
                    ? 0.55f
                    : 0.20f))
            {
                if (grenades.TryThrowTactical())
                {
                    ap -= 4;
                    yield return new WaitForSeconds(0.92f);
                    seesPlayer = CanSeePlayer();

                    if (seesPlayer)
                        RememberPlayer();
                }
            }

            // Suppression roles and machine gunners prefer to fire before
            // moving so another unit can exploit the pressure.
            if (ap >= 3 &&
                seesPlayer &&
                distance <= FireRange(archetype) &&
                (role == SquadRole.Suppress ||
                 archetype == EnemyArchetype.MachineGunner))
            {
                if (TryShoot())
                {
                    ap -= 3;
                    yield return new WaitForSeconds(0.34f);
                }
            }

            // Flankers intentionally move sideways instead of walking directly
            // down the player's firing line.
            if (ap > 0 &&
                role == SquadRole.Flank &&
                _hasLastKnown)
            {
                List<Vector2> flank =
                    FindFlankPath(
                        navigation,
                        _lastKnownPlayerPosition);

                int steps =
                    Mathf.Min(
                        ap,
                        Mathf.Min(
                            3,
                            flank.Count));

                if (steps > 0)
                {
                    Callout(
                        FlankSideSign() > 0f
                            ? "ОБХОЖУ СЛЕВА!"
                            : "ОБХОЖУ СПРАВА!");

                    yield return MoveAlongPath(
                        flank,
                        steps);

                    ap -= steps;
                    seesPlayer = CanSeePlayer();

                    if (seesPlayer)
                        RememberPlayer();
                }
            }

            distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            // Regular riflemen and pistol users fire when they actually have a
            // clear line of sight. Smoke therefore naturally prevents this.
            if (ap >= 3 &&
                seesPlayer &&
                distance <= FireRange(archetype))
            {
                if (TryShoot())
                {
                    ap -= 3;
                    yield return new WaitForSeconds(0.34f);
                }
            }

            // If there is AP left, reposition according to role. Hold/suppress
            // units seek cover; assault units advance; units blinded by smoke
            // move toward the last observed position rather than cheating.
            if (ap > 0 &&
                motor != null)
            {
                List<Vector2> movePath = null;

                if ((role == SquadRole.Hold ||
                     role == SquadRole.Suppress) &&
                    coverManager != null)
                {
                    movePath =
                        FindCoverPath(
                            navigation);
                }

                if (movePath == null ||
                    movePath.Count == 0)
                {
                    Vector2 target =
                        _hasLastKnown
                            ? _lastKnownPlayerPosition
                            : (Vector2)transform.position;

                    movePath =
                        FindSafeAdvancePath(
                            navigation,
                            target);
                }

                int maxMove =
                    role == SquadRole.Assault
                        ? 3
                        : 2;

                int steps =
                    Mathf.Min(
                        ap,
                        Mathf.Min(
                            maxMove,
                            movePath.Count));

                if (steps > 0)
                {
                    yield return MoveAlongPath(
                        movePath,
                        steps);

                    ap -= steps;
                    seesPlayer = CanSeePlayer();

                    if (seesPlayer)
                        RememberPlayer();
                }
            }

            distance =
                Vector2.Distance(
                    transform.position,
                    player.position);

            if (ap >= 3 &&
                CanSeePlayer() &&
                distance <= FireRange(archetype))
            {
                if (TryShoot())
                {
                    ap -= 3;
                    yield return new WaitForSeconds(0.34f);
                }
            }

            motor?.Stop();

            yield return new WaitForSeconds(0.12f);
        }

        private bool CanSeePlayer()
        {
            return
                player != null &&
                perception != null &&
                perception.CanSee(player);
        }

        private void RememberPlayer()
        {
            if (player == null)
                return;

            _lastKnownPlayerPosition =
                player.position;

            _hasLastKnown = true;
        }

        private bool TryShoot()
        {
            if (weapon == null)
                return false;

            bool fired =
                weapon.TryTacticalFire();

            if (fired)
            {
                RememberPlayer();
            }

            return fired;
        }

        private float FireRange(
            EnemyArchetype archetype)
        {
            switch (archetype)
            {
                case EnemyArchetype.Pistolier:
                    return 7.4f;

                case EnemyArchetype.MachineGunner:
                    return 10.5f;

                case EnemyArchetype.Demolitionist:
                    return 8.2f;

                default:
                    return 9.2f;
            }
        }

        private List<Vector2> FindCoverPath(
            NavigationGrid2D navigation)
        {
            if (coverManager == null ||
                brain == null ||
                player == null)
            {
                return new List<Vector2>();
            }

            CoverPoint cover =
                coverManager.FindBestCover(
                    transform.position,
                    player.position,
                    brain,
                    9f);

            if (cover == null ||
                TacticalFireField2D.IsDangerousPoint(
                    cover.transform.position))
            {
                return new List<Vector2>();
            }

            List<Vector2> path =
                navigation.FindPath(
                    transform.position,
                    cover.transform.position);

            return PathTouchesFire(path)
                ? new List<Vector2>()
                : path;
        }

        private List<Vector2> FindFlankPath(
            NavigationGrid2D navigation,
            Vector2 threat)
        {
            Vector2 toThreat =
                threat -
                (Vector2)transform.position;

            if (toThreat.sqrMagnitude <
                0.01f)
            {
                return new List<Vector2>();
            }

            Vector2 forward =
                toThreat.normalized;

            Vector2 side =
                new Vector2(
                    -forward.y,
                    forward.x) *
                FlankSideSign();

            Vector2 flankTarget =
                threat +
                side * 4.2f -
                forward * 1.4f;

            List<Vector2> path =
                navigation.FindPath(
                    transform.position,
                    flankTarget);

            if (PathTouchesFire(path))
            {
                flankTarget =
                    threat -
                    side * 4.2f -
                    forward * 1.4f;

                path =
                    navigation.FindPath(
                        transform.position,
                        flankTarget);
            }

            return PathTouchesFire(path)
                ? new List<Vector2>()
                : path;
        }

        private float FlankSideSign()
        {
            int idHash =
                GetEntityId()
                    .GetHashCode();

            return (idHash & 1) == 0
                ? 1f
                : -1f;
        }

        private List<Vector2> FindNearestSafePath(
            NavigationGrid2D navigation)
        {
            float cell =
                navigation.CellSize;

            Vector2 origin =
                transform.position;

            Vector2[] candidates =
            {
                origin + Vector2.right * cell * 2f,
                origin + Vector2.left * cell * 2f,
                origin + Vector2.up * cell * 2f,
                origin + Vector2.down * cell * 2f,
                origin + new Vector2(1f, 1f).normalized * cell * 2.8f,
                origin + new Vector2(-1f, 1f).normalized * cell * 2.8f,
                origin + new Vector2(1f, -1f).normalized * cell * 2.8f,
                origin + new Vector2(-1f, -1f).normalized * cell * 2.8f
            };

            List<Vector2> best =
                new List<Vector2>();

            int bestCount =
                int.MaxValue;

            for (int i = 0;
                 i < candidates.Length;
                 i++)
            {
                if (TacticalFireField2D.IsDangerousPoint(
                        candidates[i]))
                {
                    continue;
                }

                List<Vector2> path =
                    navigation.FindPath(
                        transform.position,
                        candidates[i]);

                if (path.Count == 0 ||
                    PathTouchesFire(path))
                {
                    continue;
                }

                if (path.Count < bestCount)
                {
                    best = path;
                    bestCount = path.Count;
                }
            }

            return best;
        }

        private List<Vector2> FindSafeAdvancePath(
            NavigationGrid2D navigation,
            Vector2 target)
        {
            List<Vector2> direct =
                navigation.FindPath(
                    transform.position,
                    target);

            if (!PathTouchesFire(direct))
                return direct;

            Vector2 toTarget =
                target -
                (Vector2)transform.position;

            if (toTarget.sqrMagnitude <
                0.01f)
            {
                return new List<Vector2>();
            }

            Vector2 forward =
                toTarget.normalized;

            Vector2 side =
                new Vector2(
                    -forward.y,
                    forward.x);

            List<Vector2> best = null;
            float bestScore =
                float.PositiveInfinity;

            Vector2[] candidates =
            {
                target + side * 3.0f,
                target - side * 3.0f,
                target + side * 5.0f,
                target - side * 5.0f,
                target - forward * 2.5f
            };

            for (int i = 0;
                 i < candidates.Length;
                 i++)
            {
                List<Vector2> candidate =
                    navigation.FindPath(
                        transform.position,
                        candidates[i]);

                if (candidate.Count == 0 ||
                    PathTouchesFire(candidate))
                {
                    continue;
                }

                float score =
                    candidate.Count +
                    Vector2.Distance(
                        candidate[
                            candidate.Count - 1],
                        target) *
                    0.35f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best ??
                   new List<Vector2>();
        }

        private IEnumerator MoveAlongPath(
            List<Vector2> path,
            int steps)
        {
            if (motor == null ||
                path == null ||
                path.Count == 0 ||
                steps <= 0)
            {
                yield break;
            }

            int index =
                Mathf.Clamp(
                    steps - 1,
                    0,
                    path.Count - 1);

            Vector2 destination =
                path[index];

            if (!motor.MoveTo(destination))
                yield break;

            float timeout =
                Time.time +
                2.6f;

            while (motor.HasDestination &&
                   Time.time < timeout &&
                   IsAlive)
            {
                yield return null;
            }

            motor.Stop();
        }

        private static bool PathTouchesFire(
            List<Vector2> path)
        {
            if (path == null)
                return false;

            for (int i = 0;
                 i < path.Count;
                 i++)
            {
                if (TacticalFireField2D
                    .IsDangerousPoint(
                        path[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void Callout(
            string text)
        {
            WorldCalloutSystem.Instance?.Show(
                transform,
                text,
                1.0f);
        }
    }
}
