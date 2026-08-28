using CatsAndKills.AI;
using CatsAndKills.Damage;
using CatsAndKills.Tactical;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.Combat
{
    [DisallowMultipleComponent]
    public sealed class TacticalOverwatchController :
        MonoBehaviour
    {
        [SerializeField] private HitscanWeapon2D weapon;
        [SerializeField] private float maxRange = 10.5f;
        [SerializeField] private LayerMask obstacleMask;

        private bool _armed;
        private int _armedRound;

        public bool IsArmed => _armed;

        public void Configure(
            HitscanWeapon2D playerWeapon,
            LayerMask obstacles)
        {
            weapon = playerWeapon;
            obstacleMask = obstacles;
        }

        private void Awake()
        {
            if (weapon == null)
            {
                weapon =
                    GetComponentInChildren<
                        HitscanWeapon2D>(
                        true);
            }

            if (obstacleMask.value == 0)
            {
                int obstacleLayer =
                    LayerMask.NameToLayer(
                        "Obstacles");

                if (obstacleLayer >= 0)
                {
                    obstacleMask =
                        1 << obstacleLayer;
                }
            }
        }

        private void OnEnable()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null)
                tactical.PhaseChanged +=
                    OnPhaseChanged;
        }

        private void Start()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null)
            {
                tactical.PhaseChanged -=
                    OnPhaseChanged;

                tactical.PhaseChanged +=
                    OnPhaseChanged;
            }
        }

        private void OnDisable()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null)
                tactical.PhaseChanged -=
                    OnPhaseChanged;

            _armed = false;
        }

        public bool Arm()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (_armed ||
                weapon == null ||
                tactical == null ||
                !tactical.IsPlayerTurn ||
                weapon.Magazine <= 0)
            {
                return false;
            }

            CharacterVitals vitals =
                GetComponent<CharacterVitals>();

            if (vitals != null &&
                (!vitals.CanUsePrimaryWeapon ||
                 vitals.IsDead))
            {
                return false;
            }

            _armed = true;
            _armedRound =
                tactical.RoundIndex;

            WorldCalloutSystem.Instance?.Show(
                transform,
                "НАБЛЮДАЮ СЕКТОР",
                1.1f);

            return true;
        }

        public bool TryReact(
            TacticalEnemyAgent enemy)
        {
            if (!_armed ||
                enemy == null ||
                !enemy.IsAlive ||
                !enemy.IsHostileToPlayer)
            {
                return false;
            }

            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical == null ||
                !tactical.IsEnemyTurn)
            {
                return false;
            }

            Vector2 origin =
                CharacterCombatGeometry2D.AimPoint(
                    transform);

            Vector2 target =
                CharacterCombatGeometry2D.AimPoint(
                    enemy.transform);

            float distance =
                Vector2.Distance(
                    origin,
                    target);

            if (distance >
                maxRange)
            {
                return false;
            }

            if (TacticalSmokeField2D
                .IsLineObscured(
                    origin,
                    target))
            {
                return false;
            }

            Vector2 delta =
                target -
                origin;

            if (delta.sqrMagnitude <
                0.001f)
            {
                return false;
            }

            RaycastHit2D block =
                Physics2D.Raycast(
                    origin +
                    delta.normalized *
                    0.12f,
                    delta.normalized,
                    Mathf.Max(
                        0f,
                        distance -
                        0.22f),
                    obstacleMask);

            if (block.collider != null)
                return false;

            _armed = false;

            WorldCalloutSystem.Instance?.Show(
                transform,
                "КОНТАКТ!",
                0.75f);

            bool fired =
                weapon != null &&
                weapon.TacticalFireAt(
                    target);

            if (!fired)
            {
                _armed = true;
                return false;
            }

            return true;
        }

        private void OnPhaseChanged()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (!_armed ||
                tactical == null)
            {
                return;
            }

            if (!tactical.IsTacticalCombat ||
                (tactical.IsPlayerTurn &&
                 tactical.RoundIndex >
                 _armedRound))
            {
                _armed = false;
            }
        }
    }
}
