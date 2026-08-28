using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class ConceptAtlasCharacterVisual2D : MonoBehaviour
    {
        [SerializeField] private Texture2D atlas;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private PlayerAim2D playerAim;
        [SerializeField] private HitscanWeapon2D playerWeapon;
        [SerializeField] private EnemyWeapon2D enemyWeapon;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private float pixelsPerUnit = 128f;
        [SerializeField] private float moveThreshold = 0.12f;
        [SerializeField] private float moveFrameRate = 8f;
        [Header("Procedural Animation")]
        [SerializeField] private float idleBreathAmount = 0.012f;
        [SerializeField] private float moveBobAmount = 0.045f;
        [SerializeField] private float moveSwayAmount = 0.018f;
        [SerializeField] private float fireKickAmount = 0.075f;
        [SerializeField] private float hurtKickAmount = 0.060f;

        private const int SourceColumns = 7;
        private const int SourceRows = 5;

        private readonly Sprite[,] _sprites =
            new Sprite[SourceRows, SourceColumns];

        private Vector2 _facing = Vector2.down;
        private float _hurtUntil;
        private float _fireUntil;
        private float _fireKick;
        private float _hurtKick;
        private float _phase;
        private bool _built;
        private bool _subscribed;
        private Vector3 _baseLocalScale = Vector3.one;

        public void Configure(
            Texture2D sourceAtlas,
            SpriteRenderer renderer,
            CharacterVitals characterVitals,
            Rigidbody2D rigidbodyRef,
            PlayerAim2D aim,
            HitscanWeapon2D hitscan,
            EnemyWeapon2D enemyGun,
            Transform target,
            float ppu = 128f)
        {
            atlas = sourceAtlas;
            bodyRenderer = renderer;
            vitals = characterVitals;
            body = rigidbodyRef;
            playerAim = aim;
            playerWeapon = hitscan;
            enemyWeapon = enemyGun;
            lookTarget = target;
            pixelsPerUnit = ppu;

            _baseLocalScale = transform.localScale;

            BuildSprites();
            RefreshImmediate();
            TrySubscribe();
        }

        private void Awake()
        {
            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();

            if (vitals == null)
                vitals = GetComponentInParent<CharacterVitals>();

            if (body == null)
                body = GetComponentInParent<Rigidbody2D>();

            if (playerAim == null)
                playerAim = GetComponentInParent<PlayerAim2D>();

            if (playerWeapon == null)
                playerWeapon = GetComponentInParent<HitscanWeapon2D>();

            if (enemyWeapon == null)
                enemyWeapon = GetComponentInParent<EnemyWeapon2D>();

            _baseLocalScale = transform.localScale;
            _phase = Random.Range(0f, Mathf.PI * 2f);

            BuildSprites();
            RefreshImmediate();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();

            if (!_built)
                BuildSprites();

            RefreshImmediate();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed)
                return;

            if (vitals != null)
                vitals.Damaged += OnDamaged;

            if (playerWeapon != null)
                playerWeapon.Fired += OnWeaponFired;

            if (enemyWeapon != null)
                enemyWeapon.Fired += OnWeaponFired;

            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (vitals != null)
                vitals.Damaged -= OnDamaged;

            if (playerWeapon != null)
                playerWeapon.Fired -= OnWeaponFired;

            if (enemyWeapon != null)
                enemyWeapon.Fired -= OnWeaponFired;

            _subscribed = false;
        }

        private void OnDestroy()
        {
            for (int row = 0; row < SourceRows; row++)
            {
                for (int col = 0; col < SourceColumns; col++)
                {
                    if (_sprites[row, col] != null)
                        Destroy(_sprites[row, col]);
                }
            }
        }

        private void LateUpdate()
        {
            // EnemyBrain rotates the gameplay root toward its target.
            // Directional 3/4 art must stay upright on screen instead of
            // inheriting that transform rotation.
            transform.rotation = Quaternion.identity;

            if (!_built)
                BuildSprites();

            if (!_built || bodyRenderer == null)
                return;

            _fireKick = Mathf.MoveTowards(
                _fireKick,
                0f,
                Time.unscaledDeltaTime * 10f);

            _hurtKick = Mathf.MoveTowards(
                _hurtKick,
                0f,
                Time.unscaledDeltaTime * 12f);

            RefreshImmediate();
        }

        private void BuildSprites()
        {
            if (_built || atlas == null)
                return;

            for (int row = 0; row < SourceRows; row++)
            {
                for (int col = 0; col < SourceColumns; col++)
                {
                    int x0 =
                        Mathf.RoundToInt(
                            col * atlas.width /
                            (float)SourceColumns);

                    int x1 =
                        Mathf.RoundToInt(
                            (col + 1) * atlas.width /
                            (float)SourceColumns);

                    int top0 =
                        Mathf.RoundToInt(
                            row * atlas.height /
                            (float)SourceRows);

                    int top1 =
                        Mathf.RoundToInt(
                            (row + 1) * atlas.height /
                            (float)SourceRows);

                    int width = Mathf.Max(1, x1 - x0);
                    int height = Mathf.Max(1, top1 - top0);
                    int y =
                        atlas.height -
                        top1;

                    _sprites[row, col] =
                        Sprite.Create(
                            atlas,
                            new Rect(
                                x0,
                                y,
                                width,
                                height),
                            new Vector2(0.5f, 0.075f),
                            pixelsPerUnit,
                            0,
                            SpriteMeshType.FullRect);
                }
            }

            _built = true;
        }

        private void RefreshImmediate()
        {
            CharacterDirection8 direction =
                ResolveDirection();

            int sourceColumn =
                SourceColumn(direction);

            bool flipX =
                direction == CharacterDirection8.NorthEast;

            int row = ResolveStateRow();

            Sprite next =
                _sprites[
                    Mathf.Clamp(row, 0, SourceRows - 1),
                    Mathf.Clamp(
                        sourceColumn,
                        0,
                        SourceColumns - 1)];

            if (next != null)
            {
                bodyRenderer.sprite = next;
                bodyRenderer.flipX = flipX;
                bodyRenderer.enabled = true;

                bool dead =
                    vitals != null &&
                    vitals.IsDead;

                bool hurt =
                    Time.unscaledTime <
                    _hurtUntil;

                bodyRenderer.color =
                    dead
                        ? new Color(
                            0.58f,
                            0.60f,
                            0.66f,
                            0.96f)
                        : hurt
                            ? new Color(
                                1f,
                                0.58f,
                                0.62f,
                                1f)
                            : Color.white;

                bool crawling =
                    !dead &&
                    vitals != null &&
                    vitals.LeftLegDisabled &&
                    vitals.RightLegDisabled;

                bool reloading =
                    !dead &&
                    playerWeapon != null &&
                    playerWeapon.IsReloading;

                ApplyProceduralAnimation(
                    row,
                    dead,
                    crawling,
                    reloading);
            }
        }

        private void ApplyProceduralAnimation(
            int row,
            bool dead,
            bool crawling,
            bool reloading)
        {
            if (transform.parent == null)
                return;

            float t =
                Time.unscaledTime;

            Vector3 world =
                transform.parent.position;

            Vector3 scale =
                _baseLocalScale;

            if (dead)
            {
                world +=
                    Vector3.down * 0.12f;

                scale = new Vector3(
                    _baseLocalScale.x * 1.08f,
                    _baseLocalScale.y * 0.74f,
                    _baseLocalScale.z);
            }
            else if (crawling)
            {
                float crawl =
                    Mathf.Sin(
                        t * 7.5f +
                        _phase);

                world +=
                    Vector3.down * 0.17f +
                    Vector3.right *
                    crawl *
                    0.018f;

                scale = new Vector3(
                    _baseLocalScale.x * 1.10f,
                    _baseLocalScale.y * 0.60f,
                    _baseLocalScale.z);
            }
            else if (reloading)
            {
                float reloadPulse =
                    Mathf.Sin(
                        t * 7.0f +
                        _phase);

                world +=
                    Vector3.up *
                    Mathf.Abs(reloadPulse) *
                    0.022f;

                world +=
                    Vector3.right *
                    reloadPulse *
                    0.010f;

                scale = new Vector3(
                    _baseLocalScale.x *
                    (1f - reloadPulse * 0.010f),
                    _baseLocalScale.y *
                    (1f + reloadPulse * 0.014f),
                    _baseLocalScale.z);
            }
            else if (row == 1 || row == 2)
            {
                float step =
                    t *
                    moveFrameRate *
                    Mathf.PI +
                    _phase;

                world +=
                    Vector3.up *
                    Mathf.Abs(
                        Mathf.Sin(step)) *
                    moveBobAmount;

                world +=
                    Vector3.right *
                    Mathf.Sin(step) *
                    moveSwayAmount;

                float squash =
                    Mathf.Sin(step * 2f) *
                    0.012f;

                scale = new Vector3(
                    _baseLocalScale.x *
                    (1f + squash),
                    _baseLocalScale.y *
                    (1f - squash),
                    _baseLocalScale.z);
            }
            else
            {
                float breath =
                    Mathf.Sin(
                        t * 2.2f +
                        _phase) *
                    idleBreathAmount;

                world +=
                    Vector3.up *
                    breath;

                scale = new Vector3(
                    _baseLocalScale.x *
                    (1f - breath * 0.20f),
                    _baseLocalScale.y *
                    (1f + breath),
                    _baseLocalScale.z);
            }

            if (_fireKick > 0f)
            {
                world +=
                    (Vector3)(
                        -_facing *
                        fireKickAmount *
                        _fireKick);

                scale.x *=
                    1f + 0.018f *
                    _fireKick;

                scale.y *=
                    1f - 0.012f *
                    _fireKick;
            }

            if (_hurtKick > 0f)
            {
                world +=
                    (Vector3)(
                        -_facing *
                        hurtKickAmount *
                        _hurtKick);

                world +=
                    Vector3.up *
                    Mathf.Sin(
                        t * 52f) *
                    0.018f *
                    _hurtKick;
            }

            transform.position = world;
            transform.localScale = scale;
        }

        private CharacterDirection8 ResolveDirection()
        {
            Vector2 desired = Vector2.zero;

            if (playerAim != null)
            {
                desired = playerAim.AimDirection;
            }
            else if (lookTarget != null)
            {
                desired =
                    (Vector2)lookTarget.position -
                    (Vector2)transform.position;
            }
            else if (body != null &&
                     body.linearVelocity.sqrMagnitude > 0.02f)
            {
                desired = body.linearVelocity;
            }

            if (desired.sqrMagnitude > 0.001f)
                _facing = desired.normalized;

            float angle =
                Mathf.Atan2(
                    _facing.y,
                    _facing.x) *
                Mathf.Rad2Deg;

            if (angle < 0f)
                angle += 360f;

            return (CharacterDirection8)(
                Mathf.RoundToInt(angle / 45f) % 8);
        }

        private int ResolveStateRow()
        {
            if (vitals != null && vitals.IsDead)
                return 4;

            if (vitals != null &&
                vitals.LeftLegDisabled &&
                vitals.RightLegDisabled)
            {
                return 4;
            }

            if (Time.unscaledTime < _hurtUntil)
                return 4;

            if (Time.unscaledTime < _fireUntil)
                return 3;

            if (body != null &&
                body.linearVelocity.sqrMagnitude >
                moveThreshold * moveThreshold)
            {
                return
                    Mathf.FloorToInt(
                        Time.unscaledTime *
                        Mathf.Max(
                            1f,
                            moveFrameRate)) %
                    2 == 0
                        ? 1
                        : 2;
            }

            return 0;
        }

        private static int SourceColumn(
            CharacterDirection8 direction)
        {
            // Actual generated atlas layout:
            // 0 E, 1 SE, 2 N, 3 NW, 4 W, 5 S, 6 SW.
            // NE is reconstructed by mirroring NW.
            switch (direction)
            {
                case CharacterDirection8.East:
                    return 0;

                case CharacterDirection8.NorthEast:
                    return 3;

                case CharacterDirection8.North:
                    return 2;

                case CharacterDirection8.NorthWest:
                    return 3;

                case CharacterDirection8.West:
                    return 4;

                case CharacterDirection8.SouthWest:
                    return 6;

                case CharacterDirection8.South:
                    return 5;

                case CharacterDirection8.SouthEast:
                    return 1;

                default:
                    return 5;
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            _hurtUntil =
                Time.unscaledTime + 0.18f;

            _hurtKick = 1f;
        }

        private void OnWeaponFired()
        {
            _fireUntil =
                Time.unscaledTime + 0.10f;

            _fireKick = 1f;
        }
    }
}
