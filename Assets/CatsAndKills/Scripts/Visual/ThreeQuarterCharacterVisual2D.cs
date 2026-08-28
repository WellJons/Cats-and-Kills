using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class ThreeQuarterCharacterVisual2D : MonoBehaviour
    {
        [SerializeField] private DirectionalSpriteSet sprites;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private PlayerAim2D playerAim;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private HitscanWeapon2D playerWeapon;
        [SerializeField] private EnemyWeapon2D enemyWeapon;
        [SerializeField] private EnemyBrain enemyBrain;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private float moveThreshold = 0.12f;
        [SerializeField] private float moveFrameRate = 6.0f;

        [Header("Presentation")]
        [SerializeField] private float facingSharpness = 20f;
        [SerializeField] private float directionHysteresis = 4f;
        [SerializeField] private float offsetSharpness = 24f;
        [SerializeField] private float scaleSharpness = 24f;

        // The source concept atlas contains only move_a/move_b for each
        // direction. Procedural bob/sway and cross-fading made those two poses
        // look like a broken skeletal animation, so keep them visually stable.
        [SerializeField] private float idleBreath = 0f;
        [SerializeField] private float walkBob = 0f;
        [SerializeField] private float walkSway = 0f;
        [SerializeField] private float recoilKick = 0.060f;
        [SerializeField] private float hurtKick = 0.045f;
        [SerializeField] private float frameBlendTime = 0f;

        private SpriteRenderer _transitionRenderer;
        private CharacterCombatAnchor2D _combatAnchor;
        private float _transitionUntil;
        private bool _subscribed;
        private float _fireUntil;
        private float _recoil;
        private float _hurtImpulse;
        private float _phase;
        private Vector3 _baseScale = Vector3.one;

        private Vector2 _facing = Vector2.down;
        private Vector2 _smoothedFacing = Vector2.down;
        private Vector3 _visualOffset = Vector3.zero;
        private Vector3 _visualScale = Vector3.one;
        private float _hurtUntil;
        private CharacterDirection8 _direction = CharacterDirection8.South;

        public CharacterDirection8 Direction => _direction;
        public Vector2 Facing => _facing;

        public void Configure(
            DirectionalSpriteSet spriteSet,
            SpriteRenderer renderer,
            CharacterVitals characterVitals,
            Rigidbody2D rigidbodyRef = null,
            PlayerAim2D aim = null,
            HitscanWeapon2D weapon = null,
            Transform target = null)
        {
            sprites = spriteSet;
            bodyRenderer = renderer;
            vitals = characterVitals;
            body = rigidbodyRef;
            playerAim = aim;
            playerWeapon = weapon;
            lookTarget = target;

            EnsureCombatAnchor();
            TrySubscribe();
        }

        private void Awake()
        {
            if (bodyRenderer == null)
                bodyRenderer = GetComponentInChildren<SpriteRenderer>();

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

            if (enemyBrain == null)
                enemyBrain = GetComponentInParent<EnemyBrain>();

            _baseScale = transform.localScale;
            _visualScale = _baseScale;
            _visualOffset = Vector3.zero;
            _smoothedFacing = _facing;
            _phase = Random.Range(0f, Mathf.PI * 2f);

            EnsureCombatAnchor();

            if (frameBlendTime > 0.001f)
            {
                GameObject blend =
                    new GameObject("Frame Blend");

                blend.transform.SetParent(
                    transform,
                    false);

                _transitionRenderer =
                    blend.AddComponent<SpriteRenderer>();

                _transitionRenderer.enabled = false;

                if (bodyRenderer != null)
                {
                    _transitionRenderer.sortingLayerID =
                        bodyRenderer.sortingLayerID;

                    _transitionRenderer.sortingOrder =
                        bodyRenderer.sortingOrder + 1;
                }
            }
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            TrySubscribe();
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
            {
                vitals.Damaged += OnDamaged;
                vitals.Died += OnDied;
            }

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
            {
                vitals.Damaged -= OnDamaged;
                vitals.Died -= OnDied;
            }

            if (playerWeapon != null)
                playerWeapon.Fired -= OnWeaponFired;

            if (enemyWeapon != null)
                enemyWeapon.Fired -= OnWeaponFired;

            _subscribed = false;
        }

        private void LateUpdate()
        {
            // EnemyBrain rotates its gameplay root to aim. Directional 3/4 art
            // must remain upright and select a facing sprite instead.
            transform.rotation = Quaternion.identity;

            if (bodyRenderer == null || sprites == null)
                return;

            _recoil = Mathf.MoveTowards(
                _recoil,
                0f,
                Time.deltaTime * 11f);

            _hurtImpulse = Mathf.MoveTowards(
                _hurtImpulse,
                0f,
                Time.deltaTime * 12f);

            bool dead =
                vitals != null &&
                vitals.IsDead;

            if (!dead)
                UpdateFacing();

            UpdateSprite();
            UpdateFrameBlend();
            AnimatePresentation();
            RefreshCombatAnchor();
        }

        private void EnsureCombatAnchor()
        {
            GameObject root =
                vitals != null
                    ? vitals.gameObject
                    : body != null
                        ? body.gameObject
                        : transform.root.gameObject;

            _combatAnchor =
                root.GetComponent<CharacterCombatAnchor2D>();

            if (_combatAnchor == null)
                _combatAnchor =
                    root.AddComponent<CharacterCombatAnchor2D>();
        }

        private void RefreshCombatAnchor()
        {
            if (_combatAnchor == null)
                EnsureCombatAnchor();

            if (_combatAnchor == null ||
                bodyRenderer == null ||
                bodyRenderer.sprite == null ||
                sprites == null)
            {
                _combatAnchor?.Clear();
                return;
            }

            Bounds bounds =
                bodyRenderer.bounds;

            Vector2 muzzle01 =
                sprites.GetMuzzleAnchor01(
                    _direction);

            Vector2 aimPoint =
                new Vector2(
                    bounds.center.x,
                    Mathf.Lerp(
                        bounds.min.y,
                        bounds.max.y,
                        sprites.AimHeight01));

            Vector2 muzzlePoint =
                new Vector2(
                    Mathf.Lerp(
                        bounds.min.x,
                        bounds.max.x,
                        muzzle01.x),
                    Mathf.Lerp(
                        bounds.min.y,
                        bounds.max.y,
                        muzzle01.y));

            _combatAnchor.SetWorldPoints(
                aimPoint,
                muzzlePoint);
        }

        private void UpdateFacing()
        {
            Vector2 desired = Vector2.zero;

            if (playerAim != null)
            {
                desired = playerAim.AimDirection;
            }
            else if (lookTarget != null &&
                     (enemyBrain == null ||
                      enemyBrain.IsAlerted))
            {
                Transform root =
                    transform.parent != null
                        ? transform.parent
                        : transform;

                desired =
                    CharacterCombatGeometry2D.AimPoint(
                        lookTarget) -
                    CharacterCombatGeometry2D.AimPoint(
                        root);
            }
            else if (body != null &&
                     body.linearVelocity.sqrMagnitude > 0.02f)
            {
                desired = body.linearVelocity;
            }
            else if (transform.parent != null)
            {
                desired = transform.parent.right;
            }
            else
            {
                desired = transform.right;
            }

            if (desired.sqrMagnitude > 0.001f)
            {
                _facing = desired.normalized;

                float t =
                    1f -
                    Mathf.Exp(
                        -facingSharpness *
                        Time.deltaTime);

                _smoothedFacing =
                    Vector2.Lerp(
                        _smoothedFacing,
                        _facing,
                        t).normalized;
            }

            float angle =
                Mathf.Atan2(
                    _smoothedFacing.y,
                    _smoothedFacing.x) *
                Mathf.Rad2Deg;

            if (angle < 0f)
                angle += 360f;

            float currentCenter =
                ((int)_direction) * 45f;

            float distanceFromCurrent =
                Mathf.Abs(
                    Mathf.DeltaAngle(
                        currentCenter,
                        angle));

            if (distanceFromCurrent >
                22.5f + directionHysteresis)
            {
                int sector =
                    Mathf.RoundToInt(
                        angle / 45f) %
                    8;

                _direction =
                    (CharacterDirection8)sector;
            }
        }

        private void UpdateSprite()
        {
            Sprite next;

            if (vitals != null && vitals.IsDead)
            {
                next = sprites.GetDead(_direction);
            }
            else if (vitals != null &&
                     vitals.LeftLegDisabled &&
                     vitals.RightLegDisabled)
            {
                next = sprites.GetCrawl(_direction);
            }
            else if (Time.time < _hurtUntil)
            {
                next = sprites.GetHurt(_direction);
            }
            else if (Time.time < _fireUntil)
            {
                next = sprites.GetFire(_direction);
            }
            else if (playerWeapon != null && playerWeapon.IsReloading)
            {
                next = sprites.GetReload(_direction);
            }
            else if (body != null &&
                     body.linearVelocity.sqrMagnitude >
                     moveThreshold * moveThreshold)
            {
                float speed =
                    body.linearVelocity.magnitude;

                float cadence =
                    Mathf.Lerp(
                        moveFrameRate,
                        moveFrameRate * 1.45f,
                        Mathf.InverseLerp(
                            2.5f,
                            7.5f,
                            speed));

                int phase =
                    Mathf.FloorToInt(
                        Time.time *
                        Mathf.Max(
                            1f,
                            cadence)) %
                    8;

                next =
                    sprites.GetWalkFrame(
                        _direction,
                        phase);
            }
            else
            {
                next = sprites.GetIdle(_direction);
            }

            bool nextFlip =
                sprites.ShouldFlipX(_direction);

            if (next != null)
            {
                if (bodyRenderer.sprite != null &&
                    bodyRenderer.sprite != next &&
                    _transitionRenderer != null)
                {
                    _transitionRenderer.sprite =
                        bodyRenderer.sprite;

                    _transitionRenderer.flipX =
                        bodyRenderer.flipX;

                    _transitionRenderer.color =
                        bodyRenderer.color;

                    _transitionRenderer.enabled = true;

                    _transitionUntil =
                        Time.time +
                        frameBlendTime;
                }

                bodyRenderer.sprite = next;
                bodyRenderer.enabled = true;
                bodyRenderer.color =
                    vitals != null && vitals.IsDead
                        ? new Color(0.58f, 0.60f, 0.66f, 0.96f)
                        : Time.time < _hurtUntil
                            ? new Color(1f, 0.60f, 0.64f, 1f)
                            : Color.white;
            }

            bodyRenderer.flipX = nextFlip;
        }

        private void UpdateFrameBlend()
        {
            if (_transitionRenderer == null ||
                !_transitionRenderer.enabled)
            {
                return;
            }

            if (bodyRenderer != null)
            {
                _transitionRenderer.sortingLayerID =
                    bodyRenderer.sortingLayerID;

                _transitionRenderer.sortingOrder =
                    bodyRenderer.sortingOrder + 1;
            }

            float remaining =
                _transitionUntil -
                Time.time;

            if (remaining <= 0f ||
                frameBlendTime <= 0.001f)
            {
                _transitionRenderer.enabled = false;
                return;
            }

            float alpha =
                Mathf.Clamp01(
                    remaining /
                    frameBlendTime);

            Color c =
                _transitionRenderer.color;

            c.a = alpha * 0.12f;
            _transitionRenderer.color = c;
        }

        private void AnimatePresentation()
        {
            if (transform.parent == null)
                return;

            float t = Time.time;
            bool dead = vitals != null && vitals.IsDead;
            bool crawling =
                !dead &&
                vitals != null &&
                vitals.LeftLegDisabled &&
                vitals.RightLegDisabled;

            bool moving =
                !dead &&
                body != null &&
                body.linearVelocity.sqrMagnitude >
                moveThreshold * moveThreshold;

            Vector3 world = transform.parent.position;
            Vector3 scale = _baseScale;

            if (dead)
            {
                world += Vector3.down * 0.12f;
                scale = new Vector3(
                    _baseScale.x * 1.06f,
                    _baseScale.y * 0.76f,
                    _baseScale.z);
            }
            else if (crawling)
            {
                float crawl = Mathf.Sin(t * 7.5f + _phase);
                world +=
                    Vector3.down * 0.17f +
                    Vector3.right * crawl * 0.018f;

                scale = new Vector3(
                    _baseScale.x * 1.10f,
                    _baseScale.y * 0.62f,
                    _baseScale.z);
            }
            else if (moving)
            {
                float step =
                    t * moveFrameRate * Mathf.PI + _phase;

                world +=
                    Vector3.up *
                    Mathf.Abs(Mathf.Sin(step)) *
                    walkBob;

                world +=
                    Vector3.right *
                    Mathf.Sin(step) *
                    walkSway;
            }
            else
            {
                float breath =
                    Mathf.Sin(t * 2.2f + _phase) *
                    idleBreath;

                world += Vector3.up * breath;
                scale.y *= 1f + breath;
            }

            if (_recoil > 0f)
            {
                world +=
                    (Vector3)(-_facing * recoilKick * _recoil);
            }

            if (_hurtImpulse > 0f)
            {
                world +=
                    (Vector3)(-_facing * hurtKick * _hurtImpulse);

                world +=
                    Vector3.up *
                    Mathf.Sin(t * 50f) *
                    0.016f *
                    _hurtImpulse;
            }

            Vector3 targetOffset =
                world -
                transform.parent.position;

            float positionT =
                1f -
                Mathf.Exp(
                    -offsetSharpness *
                    Time.deltaTime);

            float scaleT =
                1f -
                Mathf.Exp(
                    -scaleSharpness *
                    Time.deltaTime);

            _visualOffset =
                Vector3.Lerp(
                    _visualOffset,
                    targetOffset,
                    positionT);

            _visualScale =
                Vector3.Lerp(
                    _visualScale,
                    scale,
                    scaleT);

            transform.position =
                transform.parent.position +
                _visualOffset;

            transform.localScale =
                _visualScale;
        }

        private void OnDamaged(DamageInfo info)
        {
            _hurtUntil = Time.time + 0.18f;
            _hurtImpulse = 1f;
        }

        private void OnDied()
        {
            _fireUntil = 0f;
            _hurtUntil = 0f;
            _recoil = 0f;
            _hurtImpulse = 0f;

            UpdateSprite();
            RefreshCombatAnchor();
        }

        private void OnWeaponFired()
        {
            _fireUntil = Time.time + 0.10f;
            _recoil = 1f;
        }
    }
}
