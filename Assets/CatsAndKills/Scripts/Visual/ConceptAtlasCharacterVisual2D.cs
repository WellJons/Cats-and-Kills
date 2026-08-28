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

        private const int SourceColumns = 7;
        private const int SourceRows = 5;

        private readonly Sprite[,] _sprites =
            new Sprite[SourceRows, SourceColumns];

        private Vector2 _facing = Vector2.down;
        private float _hurtUntil;
        private float _fireUntil;
        private bool _built;

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

            BuildSprites();
            RefreshImmediate();
        }

        private void OnEnable()
        {
            if (vitals != null)
                vitals.Damaged += OnDamaged;

            if (playerWeapon != null)
                playerWeapon.Fired += OnWeaponFired;

            if (enemyWeapon != null)
                enemyWeapon.Fired += OnWeaponFired;
        }

        private void OnDisable()
        {
            if (vitals != null)
                vitals.Damaged -= OnDamaged;

            if (playerWeapon != null)
                playerWeapon.Fired -= OnWeaponFired;

            if (enemyWeapon != null)
                enemyWeapon.Fired -= OnWeaponFired;
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
                bodyRenderer.color = Color.white;
            }
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
                Time.unscaledTime + 0.16f;
        }

        private void OnWeaponFired()
        {
            _fireUntil =
                Time.unscaledTime + 0.09f;
        }
    }
}
