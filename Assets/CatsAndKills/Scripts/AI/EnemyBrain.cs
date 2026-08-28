using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyBrain : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Investigate,
            MoveToCover,
            HoldCover,
            Flank,
            Advance,
            Engage,
            Retreat
        }

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private EnemyMotor2D motor;
        [SerializeField] private EnemyPerception2D perception;
        [SerializeField] private EnemyWeapon2D weapon;
        [SerializeField] private EnemyGrenadeThrower grenades;
        [SerializeField] private SquadController squad;
        [SerializeField] private CoverManager coverManager;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private SuppressionReceiver2D suppression;
        [SerializeField] private GrenadeAwareness2D grenadeAwareness;
        [SerializeField] private EnemyMorale2D morale;

        [Header("Identity")]
        [SerializeField] private EnemyArchetype archetype = EnemyArchetype.Rifleman;
        [SerializeField] private SquadRole role = SquadRole.Assault;

        [Header("Decision")]
        [SerializeField] private float decisionInterval = 0.48f;
        [SerializeField] private float preferredRange = 6.5f;
        [SerializeField] private float flankDistance = 5f;
        [SerializeField] private float suppressionHold = 1.8f;
        [SerializeField] private float firingRange = 13f;
        [SerializeField] private float localMemoryDuration = 10f;
        [SerializeField] private float searchRadius = 4.5f;

        [Header("Personality")]
        [Range(0f, 1f)] [SerializeField] private float aggression = 0.55f;
        [Range(0f, 1f)] [SerializeField] private float teamwork = 0.75f;
        [Range(0f, 1f)] [SerializeField] private float courage = 0.65f;

        private State _state;
        private float _nextDecision;
        private Vector2 _knownPlayerPos;
        private bool _hasKnowledge;
        private bool _hadVisual;
        private CoverPoint _cover;
        private float _stateUntil;
        private float _lastCommand;
        private float _lastKnowledgeAt = -999f;
        private float _nextSearchMove;
        private int _searchStep;

        public EnemyArchetype Archetype => archetype;
        public SquadRole Role => role;
        public bool IsAlerted => _hasKnowledge || _hadVisual;

        public void Configure(
            Transform newPlayer,
            EnemyMotor2D newMotor,
            EnemyPerception2D newPerception,
            EnemyWeapon2D newWeapon,
            EnemyGrenadeThrower newGrenades,
            SquadController newSquad,
            CoverManager newCoverManager,
            CharacterVitals newVitals,
            EnemyArchetype type)
        {
            player = newPlayer;
            motor = newMotor;
            perception = newPerception;
            weapon = newWeapon;
            grenades = newGrenades;
            squad = newSquad;
            coverManager = newCoverManager;
            vitals = newVitals;
            archetype = type;

            ApplyArchetype();
        }

        public void AssignRole(SquadRole newRole)
        {
            role = newRole;
        }

        private void ApplyArchetype()
        {
            switch (archetype)
            {
                case EnemyArchetype.Pistolier:
                    aggression = Random.Range(0.25f, 0.48f);
                    courage = Random.Range(0.28f, 0.55f);
                    teamwork = Random.Range(0.35f, 0.62f);
                    preferredRange = 5.5f;
                    firingRange = 10.5f;
                    break;

                case EnemyArchetype.Rifleman:
                    aggression = Random.Range(0.48f, 0.72f);
                    courage = Random.Range(0.56f, 0.78f);
                    teamwork = Random.Range(0.65f, 0.9f);
                    preferredRange = 7f;
                    firingRange = 14f;
                    break;

                case EnemyArchetype.MachineGunner:
                    aggression = 0.42f;
                    courage = 0.88f;
                    teamwork = 0.9f;
                    preferredRange = 9f;
                    firingRange = 16f;
                    role = SquadRole.Suppress;
                    break;

                case EnemyArchetype.Demolitionist:
                    aggression = 0.92f;
                    courage = 0.94f;
                    teamwork = 0.35f;
                    preferredRange = 5f;
                    firingRange = 11f;
                    break;
            }
        }

        private void Awake()
        {
            if (motor == null) motor = GetComponent<EnemyMotor2D>();
            if (perception == null) perception = GetComponent<EnemyPerception2D>();
            if (weapon == null) weapon = GetComponent<EnemyWeapon2D>();
            if (grenades == null) grenades = GetComponent<EnemyGrenadeThrower>();
            if (vitals == null) vitals = GetComponent<CharacterVitals>();
            if (suppression == null) suppression = GetComponent<SuppressionReceiver2D>();
            if (grenadeAwareness == null) grenadeAwareness = GetComponent<GrenadeAwareness2D>();
            if (morale == null) morale = GetComponent<EnemyMorale2D>();
        }

        private void OnEnable()
        {
            squad?.Register(this);
            if (vitals != null)
            {
                vitals.Died += OnDied;
                vitals.Damaged += OnDamaged;
            }
        }

        private void Start()
        {
            squad?.Register(this);
        }

        private void OnDisable()
        {
            weapon?.SetTrigger(false);
            squad?.Unregister(this);
            ReleaseCover();

            if (vitals != null)
            {
                vitals.Died -= OnDied;
                vitals.Damaged -= OnDamaged;
            }
        }

        private void Update()
        {
            if (vitals != null && vitals.IsDead) return;

            if (grenadeAwareness != null &&
                grenadeAwareness.TryGetEvadePoint(out Vector2 evadePoint))
            {
                ReleaseCover();
                _state = State.Retreat;
                weapon?.SetTrigger(false);
                motor?.MoveTo(evadePoint);

                if (Time.time - _lastCommand > 1.1f)
                    Callout("ГРАНАТА! ВРОССЫПНУЮ!");

                return;
            }

            bool sees = player != null && perception != null && perception.CanSee(player);

            if (sees)
            {
                _knownPlayerPos = player.position;
                _hasKnowledge = true;
                _lastKnowledgeAt = Time.time;
                _searchStep = 0;
                squad?.ReportPlayer(this, _knownPlayerPos);

                if (!_hadVisual)
                    Callout("КОНТАКТ!");
            }
            else if (!_hasKnowledge && perception != null && perception.HasRecentNoise)
            {
                _knownPlayerPos = perception.HeardPosition;
                _hasKnowledge = true;
                _lastKnowledgeAt = Time.time;
                squad?.ReportNoise(this, _knownPlayerPos);
                _state = State.Investigate;
            }

            _hadVisual = sees;
            FaceThreat();

            float distance = player != null
                ? Vector2.Distance(transform.position, player.position)
                : float.MaxValue;

            bool suppressing = role == SquadRole.Suppress;
            bool shouldFire =
                sees &&
                distance <= firingRange &&
                (_state == State.Engage ||
                 _state == State.HoldCover ||
                 _state == State.MoveToCover ||
                 _state == State.Advance);

            weapon?.SetTrigger(shouldFire, suppressing);

            if (sees)
                grenades?.TryThrow(true, aggression);

            if (_state == State.MoveToCover &&
                motor != null &&
                motor.ReachedDestination &&
                _cover != null)
            {
                _state = State.HoldCover;
                _stateUntil = Time.time + suppressionHold;
            }

            if (_state == State.Flank &&
                motor != null &&
                motor.ReachedDestination)
            {
                _state = State.Engage;
            }

            if (Time.time >= _nextDecision)
            {
                _nextDecision =
                    Time.time +
                    decisionInterval +
                    Random.Range(-0.08f, 0.12f);

                Decide(sees);
            }
        }

        public void ReceiveSquadContact(Vector2 position)
        {
            if (teamwork <= 0.1f) return;

            _knownPlayerPos = position;
            _hasKnowledge = true;
            _lastKnowledgeAt = Time.time;

            if (_state == State.Idle)
            {
                _state = State.Investigate;
                motor?.MoveTo(position);
            }
        }

        public void ReceiveNoiseContact(Vector2 position)
        {
            if (_hasKnowledge) return;

            _knownPlayerPos = position;
            _hasKnowledge = true;
            _lastKnowledgeAt = Time.time;
            _state = State.Investigate;
            motor?.MoveTo(position);
        }

        public void ReceiveAreaAlert(Vector2 approximatePosition)
        {
            if (_hadVisual) return;

            _knownPlayerPos = approximatePosition;
            _hasKnowledge = true;
            _lastKnowledgeAt = Time.time;

            if (_cover == null)
                TryTakeCover();
            else
            {
                _state = State.HoldCover;
                motor?.Stop();
            }

            if (Time.time - _lastCommand > 1.8f)
                Callout("ТРЕВОГА. ДЕРЖИМ СЕКТОР.");
        }

        private void Decide(bool seesPlayer)
        {
            if (!_hasKnowledge && squad != null && squad.HasContact)
            {
                _knownPlayerPos = squad.LastKnownPlayerPosition;
                _hasKnowledge = true;
            }

            if (!_hasKnowledge)
            {
                _state = State.Idle;
                return;
            }

            float distance = Vector2.Distance(
                transform.position,
                _knownPlayerPos);

            if (vitals != null && !vitals.CanUsePrimaryWeapon)
            {
                weapon?.SetTrigger(false);

                if (vitals.LeftLegDisabled && vitals.RightLegDisabled)
                {
                    _state = State.Retreat;
                    motor?.Stop();

                    if (Time.time - _lastCommand > 2.5f)
                        Callout("НЕ МОГУ СРАЖАТЬСЯ!");

                    return;
                }

                BeginRetreat();
                return;
            }

            if (archetype == EnemyArchetype.Demolitionist &&
                seesPlayer &&
                distance <= 5.2f)
            {
                _state = State.Advance;
                ReleaseCover();
                motor?.MoveTo(_knownPlayerPos);
                return;
            }

            if (morale != null && morale.Broken)
            {
                BeginRetreat();
                return;
            }

            if (suppression != null && suppression.IsPinned)
            {
                if (_cover == null || _state != State.HoldCover)
                    TryTakeCover();

                if (Time.time - _lastCommand > 1.2f)
                    Callout("ПРИЖАЛ! НУЖНО УКРЫТИЕ!");

                return;
            }

            if (!seesPlayer)
            {
                if (Time.time - _lastKnowledgeAt > localMemoryDuration)
                {
                    _hasKnowledge = false;
                    _searchStep = 0;
                    ReleaseCover();
                    _state = State.Idle;
                    motor?.Stop();

                    if (Time.time - _lastCommand > 2.5f)
                        Callout("ПОТЕРЯЛ ЕГО.");

                    return;
                }

                if (_state == State.HoldCover && Time.time < _stateUntil)
                    return;

                _state = State.Investigate;

                if (motor != null && motor.ReachedDestination)
                {
                    if (Time.time >= _nextSearchMove)
                        BeginSearchStep();
                }
                else if (motor != null && _searchStep == 0)
                {
                    motor.MoveTo(_knownPlayerPos);
                }

                return;
            }

            if (archetype == EnemyArchetype.Pistolier &&
                vitals != null &&
                vitals.Health < vitals.MaxHealth * 0.38f &&
                courage < 0.5f)
            {
                BeginRetreat();
                return;
            }

            switch (role)
            {
                case SquadRole.Suppress:
                    if (_state == State.MoveToCover)
                        break;

                    if (_cover == null)
                        TryTakeCover();
                    else
                    {
                        _state = State.HoldCover;
                        motor?.Stop();
                        if (Time.time - _lastCommand > 3f)
                            Callout("ПРИЖМУ ЕГО! ОБХОДИТЕ!");
                    }
                    break;

                case SquadRole.Flank:
                    if (_state != State.Flank && distance > 3.5f)
                        BeginFlank(Random.value > 0.5f ? 1f : -1f);
                    else if (_state != State.Flank)
                    {
                        _state = State.Engage;
                        motor?.Stop();
                    }
                    break;

                case SquadRole.Hold:
                    if (_state == State.MoveToCover)
                        break;

                    if (_cover == null)
                        TryTakeCover();
                    else
                    {
                        _state = State.HoldCover;
                        motor?.Stop();
                    }
                    break;

                default:
                    if (distance > preferredRange * 1.25f && aggression > 0.5f)
                    {
                        _state = State.Advance;
                        motor?.MoveTo(
                            _knownPlayerPos -
                            ((Vector2)_knownPlayerPos - (Vector2)transform.position).normalized *
                            preferredRange * 0.65f);
                    }
                    else if (distance < preferredRange * 0.55f && courage < 0.82f)
                        TryTakeCover();
                    else
                    {
                        _state = State.Engage;
                        motor?.Stop();
                    }
                    break;
            }
        }

        private void TryTakeCover()
        {
            if (coverManager == null)
            {
                _state = State.Engage;
                return;
            }

            CoverPoint candidate = coverManager.FindBestCover(
                transform.position,
                _knownPlayerPos,
                this,
                archetype == EnemyArchetype.MachineGunner ? 16f : 12f);

            if (candidate == null || !candidate.TryReserve(this))
            {
                _state = State.Engage;
                return;
            }

            ReleaseCover();

            if (motor == null ||
                !motor.MoveTo(candidate.transform.position))
            {
                candidate.Release(this);
                _cover = null;
                _state = State.Engage;
                return;
            }

            _cover = candidate;
            _state = State.MoveToCover;
            _stateUntil = Time.time + suppressionHold;

            Callout(archetype == EnemyArchetype.MachineGunner
                ? "ЗАНИМАЮ ПОЗИЦИЮ!"
                : "ПРИКРОЙ!");
        }

        private void BeginFlank(float side)
        {
            Vector2 toThreat =
                (_knownPlayerPos - (Vector2)transform.position).normalized;

            Vector2 perpendicular =
                new Vector2(-toThreat.y, toThreat.x) * side;

            float extra =
                archetype == EnemyArchetype.Pistolier
                    ? 0.72f
                    : 1f;

            Vector2 flankPoint =
                _knownPlayerPos +
                perpendicular * flankDistance * extra -
                toThreat * 1.6f;

            ReleaseCover();

            if (motor == null ||
                !motor.MoveTo(flankPoint))
            {
                Vector2 oppositePoint =
                    _knownPlayerPos -
                    perpendicular * flankDistance * extra -
                    toThreat * 1.6f;

                if (motor == null ||
                    !motor.MoveTo(oppositePoint))
                {
                    _state = State.Engage;
                    motor?.Stop();
                    return;
                }

                side = -side;
            }

            _state = State.Flank;

            Callout(side > 0f
                ? "ОБХОЖУ СЛЕВА!"
                : "ОБХОЖУ СПРАВА!");
        }

        private void BeginSearchStep()
        {
            if (motor == null) return;

            _searchStep++;
            _nextSearchMove = Time.time + Random.Range(0.55f, 1.1f);

            Vector2 offset = Random.insideUnitCircle;
            if (offset.sqrMagnitude < 0.05f)
                offset = Vector2.right;

            offset = offset.normalized *
                     Random.Range(
                         Mathf.Min(1.2f + _searchStep * 0.25f, searchRadius),
                         searchRadius);

            motor.MoveTo(_knownPlayerPos + offset);

            if (_searchStep == 1 && Time.time - _lastCommand > 1.5f)
                Callout("ПРОВЕРЯЮ ПОСЛЕДНЮЮ ПОЗИЦИЮ!");
        }

        private void BeginRetreat()
        {
            if (player == null) return;

            Vector2 away =
                ((Vector2)transform.position - (Vector2)player.position).normalized;

            _state = State.Retreat;
            ReleaseCover();
            motor?.MoveTo((Vector2)transform.position + away * 6f);
            Callout("ОТХОДИМ!");
        }

        private void FaceThreat()
        {
            if (!_hasKnowledge) return;

            Vector2 delta =
                _knownPlayerPos - (Vector2)transform.position;

            if (delta.sqrMagnitude < 0.001f) return;

            perception?.SetFacing(delta);
        }

        private void OnDamaged(DamageInfo info)
        {
            _knownPlayerPos = info.Source != null
                ? info.Source.transform.position
                : _knownPlayerPos;

            _hasKnowledge = true;
            _lastKnowledgeAt = Time.time;
            CombatDirector.Instance?.ReportCombat();

            if (Random.value < 0.35f)
                Callout(vitals != null && vitals.Health < 35f
                    ? "Я РАНЕН!"
                    : "ПО МНЕ РАБОТАЮТ!");
        }

        private void Callout(string text)
        {
            _lastCommand = Time.time;
            WorldCalloutSystem.Instance?.Show(transform, text, 1.05f);
        }

        private void ReleaseCover()
        {
            if (_cover != null)
            {
                _cover.Release(this);
                _cover = null;
            }
        }

        private void OnDied()
        {
            weapon?.SetTrigger(false);
            motor?.Stop();
            ReleaseCover();
            enabled = false;
        }
    }
}
