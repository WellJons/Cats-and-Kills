using System.Collections;
using System.Collections.Generic;
using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.Narrative;
using CatsAndKills.Tactical;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CatsAndKills.Player
{
    [DisallowMultipleComponent]
    public sealed class TacticalPlayerController : MonoBehaviour
    {
        private enum TargetMode
        {
            Move,
            Grenade,
            Molotov,
            Smoke
        }

        [SerializeField] private NavigationGrid2D navigation;
        [SerializeField] private TacticalCombatDirector tactical;
        [SerializeField] private HitscanWeapon2D weapon;
        [SerializeField] private PlayerGrenadeController grenades;
        [SerializeField] private TacticalUtilityBelt utilityBelt;
        [SerializeField] private TacticalOverwatchController overwatch;
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private float movementSpeed = 6.5f;

        private TargetMode _targetMode;
        private bool _moving;

        public bool IsMoving => _moving;
        public bool GrenadeTargeting => _targetMode == TargetMode.Grenade;
        public bool MolotovTargeting => _targetMode == TargetMode.Molotov;
        public bool SmokeTargeting => _targetMode == TargetMode.Smoke;
        public TacticalUtilityBelt UtilityBelt => utilityBelt;
        public TacticalOverwatchController Overwatch => overwatch;

        public void Configure(
            NavigationGrid2D nav,
            TacticalCombatDirector director,
            HitscanWeapon2D playerWeapon,
            PlayerGrenadeController grenadeController,
            TacticalUtilityBelt belt,
            TacticalOverwatchController overwatchController,
            PlayerAim2D playerAim,
            Camera cameraRef)
        {
            navigation = nav;
            tactical = director;
            weapon = playerWeapon;
            grenades = grenadeController;
            utilityBelt = belt;
            overwatch = overwatchController;
            aim = playerAim;
            worldCamera = cameraRef;
        }

        private void Awake()
        {
            if (body == null)
                body = GetComponent<Rigidbody2D>();

            if (vitals == null)
                vitals = GetComponent<CharacterVitals>();

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (tactical == null)
                tactical = TacticalCombatDirector.Instance;

            if (navigation == null)
                navigation = FindAnyObjectByType<NavigationGrid2D>();

            if (weapon == null)
                weapon = GetComponentInChildren<HitscanWeapon2D>(true);

            if (grenades == null)
                grenades = GetComponent<PlayerGrenadeController>();

            if (utilityBelt == null)
                utilityBelt = GetComponent<TacticalUtilityBelt>();

            if (overwatch == null)
                overwatch = GetComponent<TacticalOverwatchController>();

            if (aim == null)
                aim = GetComponent<PlayerAim2D>();
        }

        private void Update()
        {
            if (tactical == null)
                tactical = TacticalCombatDirector.Instance;

            if (tactical == null ||
                !tactical.IsPlayerTurn ||
                _moving ||
                NarrativeDialogueSystem.IsDialogueOpen)
            {
                return;
            }

            if (CKInput.EndTurnPressed)
            {
                _targetMode = TargetMode.Move;
                tactical.EndPlayerTurn();
                return;
            }

            if (CKInput.GrenadePressed)
            {
                _targetMode =
                    _targetMode == TargetMode.Grenade
                        ? TargetMode.Move
                        : TargetMode.Grenade;

                return;
            }

            if (CKInput.MolotovPressed)
            {
                _targetMode =
                    _targetMode == TargetMode.Molotov
                        ? TargetMode.Move
                        : TargetMode.Molotov;

                return;
            }

            if (CKInput.SmokePressed)
            {
                _targetMode =
                    _targetMode == TargetMode.Smoke
                        ? TargetMode.Move
                        : TargetMode.Smoke;

                return;
            }

            if (CKInput.OverwatchPressed)
            {
                if (tactical.TrySpendAP(3))
                {
                    if (overwatch == null ||
                        !overwatch.Arm())
                    {
                        tactical.RefundAP(3);
                    }
                    else
                    {
                        _targetMode =
                            TargetMode.Move;

                        tactical.EndPlayerTurn();
                    }
                }

                return;
            }

            if (CKInput.ReloadPressed)
            {
                if (tactical.TrySpendAP(2))
                {
                    if (weapon == null ||
                        !weapon.TacticalReloadInstant())
                    {
                        tactical.RefundAP(2);
                    }
                    else
                    {
                        MaybeEndTurn();
                    }
                }

                return;
            }

            Vector2Int step =
                CKInput.TacticalStepPressed;

            if (step != Vector2Int.zero)
            {
                TryStep(step);
                return;
            }

            if (CKInput.TacticalShootPressed)
            {
                Vector2 target =
                    MouseWorld();

                if (tactical.TrySpendAP(3))
                {
                    if (weapon == null ||
                        !weapon.TacticalFireAt(target))
                    {
                        tactical.RefundAP(3);
                    }
                    else
                    {
                        MaybeEndTurn();
                    }
                }

                return;
            }

            if (CKInput.TacticalMoveClickPressed)
            {
                Vector2 target =
                    MouseWorld();

                if (_targetMode ==
                    TargetMode.Grenade)
                {
                    if (tactical.TrySpendAP(4))
                    {
                        if (grenades == null ||
                            !grenades.ThrowTacticalAt(target))
                        {
                            tactical.RefundAP(4);
                        }
                        else
                        {
                            _targetMode =
                                TargetMode.Move;

                            StartCoroutine(
                                ResolveUtilityAction(
                                    0.88f));
                        }
                    }

                    return;
                }

                if (_targetMode ==
                    TargetMode.Molotov)
                {
                    if (tactical.TrySpendAP(4))
                    {
                        if (utilityBelt == null ||
                            !utilityBelt.ThrowMolotovAt(target))
                        {
                            tactical.RefundAP(4);
                        }
                        else
                        {
                            _targetMode =
                                TargetMode.Move;

                            StartCoroutine(
                                ResolveUtilityAction(
                                    0.58f));
                        }
                    }

                    return;
                }

                if (_targetMode ==
                    TargetMode.Smoke)
                {
                    if (tactical.TrySpendAP(3))
                    {
                        if (utilityBelt == null ||
                            !utilityBelt.ThrowSmokeAt(target))
                        {
                            tactical.RefundAP(3);
                        }
                        else
                        {
                            _targetMode =
                                TargetMode.Move;

                            StartCoroutine(
                                ResolveUtilityAction(
                                    0.58f));
                        }
                    }

                    return;
                }

                TryMoveTo(target);
            }
        }

        private Vector2 MouseWorld()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            Vector2 screen =
                CKInput.MouseScreenPosition;

            Vector3 world =
                worldCamera.ScreenToWorldPoint(
                    new Vector3(
                        screen.x,
                        screen.y,
                        -worldCamera.transform.position.z));

            return world;
        }

        private void TryStep(
            Vector2Int step)
        {
            if (navigation == null ||
                tactical == null ||
                tactical.PlayerAP < 1)
            {
                return;
            }

            Vector2 target =
                navigation.SnapToCell(
                    (Vector2)transform.position +
                    new Vector2(
                        step.x,
                        step.y) *
                    navigation.CellSize);

            var path =
                navigation.FindPath(
                    transform.position,
                    target);

            if (path.Count != 1)
                return;

            if (!tactical.TrySpendAP(1))
                return;

            StartCoroutine(
                MovePath(path));
        }

        private void TryMoveTo(
            Vector2 target)
        {
            if (navigation == null ||
                tactical == null)
            {
                return;
            }

            var path =
                navigation.FindPath(
                    transform.position,
                    target);

            if (path.Count <= 0)
                return;

            int cost =
                path.Count;

            if (cost >
                tactical.PlayerAP)
            {
                return;
            }

            if (!tactical.TrySpendAP(cost))
                return;

            StartCoroutine(
                MovePath(path));
        }

        private IEnumerator MovePath(
            List<Vector2> path)
        {
            _moving = true;

            foreach (Vector2 waypoint in path)
            {
                while (Vector2.Distance(
                           transform.position,
                           waypoint) > 0.035f)
                {
                    Vector2 next =
                        Vector2.MoveTowards(
                            transform.position,
                            waypoint,
                            movementSpeed *
                            Time.deltaTime);

                    if (body != null)
                        body.MovePosition(next);
                    else
                        transform.position = next;

                    yield return null;
                }

                if (body != null)
                    body.position = waypoint;
                else
                    transform.position = waypoint;

                if (TacticalFireField2D.ApplyTraversalBurn(
                        vitals,
                        waypoint,
                        7f))
                {
                    UI.WorldCalloutSystem.Instance?.Show(
                        transform,
                        "ГОРЮ!",
                        0.7f);

                    if (vitals != null &&
                        vitals.IsDead)
                    {
                        break;
                    }
                }
            }

            if (body != null)
                body.linearVelocity = Vector2.zero;

            _moving = false;
            MaybeEndTurn();
        }

        private IEnumerator ResolveUtilityAction(
            float duration)
        {
            _moving = true;

            yield return new WaitForSeconds(
                Mathf.Max(
                    0.1f,
                    duration));

            _moving = false;
            MaybeEndTurn();
        }

        private void MaybeEndTurn()
        {
            if (_moving ||
                tactical == null ||
                !tactical.IsPlayerTurn ||
                tactical.PlayerAP > 0)
            {
                return;
            }

            tactical.EndPlayerTurn();
        }
    }
}
