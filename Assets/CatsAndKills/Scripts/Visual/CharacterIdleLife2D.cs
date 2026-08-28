using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class CharacterIdleLife2D : MonoBehaviour
    {
        private enum IdleAction
        {
            None,
            Cigarette,
            LowReady,
            Shoulder,
            GrenadePlay
        }

        [SerializeField] private ThreeQuarterCharacterVisual2D characterVisual;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private WeaponVisualRecoil2D playerWeaponVisual;
        [SerializeField] private EnemyBrain enemyBrain;
        [SerializeField] private Sprite simpleSprite;
        [SerializeField] private Sprite grenadeSprite;
        [SerializeField] private Sprite smokeSprite;

        [Header("Blink")]
        [SerializeField] private Vector2 blinkInterval = new Vector2(2.2f, 5.8f);
        [SerializeField] private float blinkDuration = 0.105f;

        [Header("Idle actions")]
        [SerializeField] private Vector2 actionDelay = new Vector2(4.2f, 8.5f);
        [SerializeField] private Vector2 actionDuration = new Vector2(2.4f, 5.6f);

        private SpriteRenderer _blinkRenderer;
        private SpriteRenderer _cigaretteRenderer;
        private SpriteRenderer _cigaretteTipRenderer;
        private SpriteRenderer _smokeRenderer;
        private SpriteRenderer _grenadeRenderer;

        private IdleAction _action;
        private float _actionUntil;
        private float _nextAction;
        private float _nextBlink;
        private float _blinkUntil;
        private float _smokePhase;

        public void Configure(
            ThreeQuarterCharacterVisual2D visual,
            SpriteRenderer renderer,
            Rigidbody2D rigidbodyRef,
            CharacterVitals characterVitals,
            WeaponVisualRecoil2D weaponVisual,
            EnemyBrain brain,
            Sprite utilitySprite,
            Sprite grenade,
            Sprite smoke)
        {
            characterVisual = visual;
            bodyRenderer = renderer;
            body = rigidbodyRef;
            vitals = characterVitals;
            playerWeaponVisual = weaponVisual;
            enemyBrain = brain;
            simpleSprite = utilitySprite;
            grenadeSprite = grenade;
            smokeSprite = smoke;
        }

        private void Awake()
        {
            if (characterVisual == null)
                characterVisual = GetComponent<ThreeQuarterCharacterVisual2D>();

            if (bodyRenderer == null)
                bodyRenderer = GetComponent<SpriteRenderer>();

            if (body == null)
                body = GetComponentInParent<Rigidbody2D>();

            if (vitals == null)
                vitals = GetComponentInParent<CharacterVitals>();

            if (enemyBrain == null)
                enemyBrain = GetComponentInParent<EnemyBrain>();

            if (playerWeaponVisual == null)
            {
                HitscanWeapon2D weapon =
                    GetComponentInParent<HitscanWeapon2D>();

                if (weapon != null)
                    playerWeaponVisual = weapon.GetComponent<WeaponVisualRecoil2D>();
            }

            CreateOverlays();
            ScheduleBlink();
            ScheduleAction();
        }

        private void OnEnable()
        {
            ScheduleBlink();
            ScheduleAction();
        }

        private void OnDisable()
        {
            EndAction();
            SetOverlayEnabled(false);
        }

        private void CreateOverlays()
        {
            if (simpleSprite == null)
                return;

            _blinkRenderer =
                CreateOverlay(
                    "Blink",
                    simpleSprite,
                    new Color(0.05f, 0.035f, 0.045f, 0.96f),
                    8);

            _cigaretteRenderer =
                CreateOverlay(
                    "Cigarette",
                    simpleSprite,
                    new Color(0.88f, 0.86f, 0.77f, 1f),
                    9);

            _cigaretteTipRenderer =
                CreateOverlay(
                    "Cigarette Tip",
                    simpleSprite,
                    new Color(1f, 0.28f, 0.08f, 1f),
                    10);

            _grenadeRenderer =
                CreateOverlay(
                    "Idle Grenade",
                    grenadeSprite != null
                        ? grenadeSprite
                        : simpleSprite,
                    new Color(0.70f, 0.78f, 0.58f, 1f),
                    10);

            if (smokeSprite != null)
            {
                _smokeRenderer =
                    CreateOverlay(
                        "Cigarette Smoke",
                        smokeSprite,
                        new Color(0.65f, 0.70f, 0.78f, 0.34f),
                        7);
            }

            SetOverlayEnabled(false);
        }

        private SpriteRenderer CreateOverlay(
            string name,
            Sprite sprite,
            Color color,
            int localOrder)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                transform,
                false);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;

            if (bodyRenderer != null)
            {
                sr.sortingLayerID =
                    bodyRenderer.sortingLayerID;

                sr.sortingOrder =
                    bodyRenderer.sortingOrder +
                    localOrder;
            }

            sr.enabled = false;
            return sr;
        }

        private void Update()
        {
            if (bodyRenderer == null ||
                bodyRenderer.sprite == null ||
                characterVisual == null)
            {
                return;
            }

            UpdateBlink();

            if (!CanIdle())
            {
                EndAction();
                ScheduleAction();
                return;
            }

            if (_action != IdleAction.None)
            {
                UpdateAction();

                if (Time.time >= _actionUntil)
                {
                    EndAction();
                    ScheduleAction();
                }

                return;
            }

            if (Time.time >= _nextAction)
                BeginRandomAction();
        }

        private bool CanIdle()
        {
            if (vitals != null &&
                vitals.IsDead)
            {
                return false;
            }

            if (body != null &&
                body.linearVelocity.sqrMagnitude > 0.025f)
            {
                return false;
            }

            if (enemyBrain != null &&
                enemyBrain.IsAlerted)
            {
                return false;
            }

            CombatDirector director =
                CombatDirector.Instance;

            if (director != null &&
                director.Intensity != CombatIntensity.Calm)
            {
                return false;
            }

            return true;
        }

        private void UpdateBlink()
        {
            if (_blinkRenderer == null)
                return;

            bool canBlink =
                vitals == null ||
                !vitals.IsDead;

            if (!canBlink)
            {
                _blinkRenderer.enabled = false;
                return;
            }

            if (Time.time >= _nextBlink &&
                Time.time >= _blinkUntil)
            {
                _blinkUntil =
                    Time.time +
                    blinkDuration;

                ScheduleBlink();
            }

            bool blinking =
                Time.time <
                _blinkUntil;

            CharacterDirection8 direction =
                characterVisual.Direction;

            bool faceVisible =
                direction != CharacterDirection8.North &&
                direction != CharacterDirection8.NorthEast &&
                direction != CharacterDirection8.NorthWest;

            _blinkRenderer.enabled =
                blinking &&
                faceVisible;

            if (!_blinkRenderer.enabled)
                return;

            Bounds bounds =
                bodyRenderer.sprite.bounds;

            Vector2 anchor =
                BlinkAnchor(direction);

            _blinkRenderer.transform.localPosition =
                new Vector3(
                    Mathf.Lerp(
                        bounds.min.x,
                        bounds.max.x,
                        anchor.x),
                    Mathf.Lerp(
                        bounds.min.y,
                        bounds.max.y,
                        anchor.y),
                    0f);

            Vector2 utilitySize =
                _blinkRenderer.sprite.bounds.size;

            float width =
                bounds.size.x *
                (direction == CharacterDirection8.East ||
                 direction == CharacterDirection8.West
                    ? 0.10f
                    : 0.16f);

            float height =
                bounds.size.y *
                0.014f;

            _blinkRenderer.transform.localScale =
                new Vector3(
                    width /
                    Mathf.Max(
                        0.001f,
                        utilitySize.x),
                    height /
                    Mathf.Max(
                        0.001f,
                        utilitySize.y),
                    1f);
        }

        private static Vector2 BlinkAnchor(
            CharacterDirection8 direction)
        {
            return direction switch
            {
                CharacterDirection8.East =>
                    new Vector2(0.59f, 0.75f),

                CharacterDirection8.West =>
                    new Vector2(0.41f, 0.75f),

                CharacterDirection8.SouthEast =>
                    new Vector2(0.57f, 0.76f),

                CharacterDirection8.SouthWest =>
                    new Vector2(0.43f, 0.76f),

                _ =>
                    new Vector2(0.50f, 0.77f)
            };
        }

        private void BeginRandomAction()
        {
            int min =
                playerWeaponVisual != null
                    ? 1
                    : 1;

            int max =
                playerWeaponVisual != null
                    ? 5
                    : 3;

            _action =
                (IdleAction)Random.Range(
                    min,
                    max);

            _actionUntil =
                Time.time +
                Random.Range(
                    actionDuration.x,
                    actionDuration.y);

            switch (_action)
            {
                case IdleAction.Cigarette:
                    SetCigarette(true);
                    break;

                case IdleAction.LowReady:
                    playerWeaponVisual?.SetIdlePose(
                        WeaponIdlePose.LowReady);
                    break;

                case IdleAction.Shoulder:
                    playerWeaponVisual?.SetIdlePose(
                        WeaponIdlePose.Shoulder);
                    break;

                case IdleAction.GrenadePlay:
                    if (_grenadeRenderer != null)
                        _grenadeRenderer.enabled = true;
                    break;
            }
        }

        private void UpdateAction()
        {
            switch (_action)
            {
                case IdleAction.Cigarette:
                    UpdateCigarette();
                    break;

                case IdleAction.GrenadePlay:
                    UpdateGrenadePlay();
                    break;
            }
        }

        private void UpdateCigarette()
        {
            if (_cigaretteRenderer == null ||
                characterVisual == null)
            {
                return;
            }

            CharacterDirection8 direction =
                characterVisual.Direction;

            if (direction == CharacterDirection8.North)
            {
                SetCigarette(false);
                return;
            }

            Bounds bounds =
                bodyRenderer.sprite.bounds;

            Vector2 anchor =
                direction == CharacterDirection8.West ||
                direction == CharacterDirection8.SouthWest ||
                direction == CharacterDirection8.NorthWest
                    ? new Vector2(0.39f, 0.71f)
                    : new Vector2(0.61f, 0.71f);

            Vector3 position =
                new Vector3(
                    Mathf.Lerp(
                        bounds.min.x,
                        bounds.max.x,
                        anchor.x),
                    Mathf.Lerp(
                        bounds.min.y,
                        bounds.max.y,
                        anchor.y),
                    0f);

            _cigaretteRenderer.transform.localPosition =
                position;

            float horizontal =
                direction == CharacterDirection8.West ||
                direction == CharacterDirection8.SouthWest ||
                direction == CharacterDirection8.NorthWest
                    ? -1f
                    : 1f;

            _cigaretteRenderer.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -8f *
                    horizontal);

            Vector2 utility =
                _cigaretteRenderer.sprite.bounds.size;

            _cigaretteRenderer.transform.localScale =
                new Vector3(
                    bounds.size.x *
                    0.085f /
                    Mathf.Max(
                        0.001f,
                        utility.x),
                    bounds.size.y *
                    0.012f /
                    Mathf.Max(
                        0.001f,
                        utility.y),
                    1f);

            if (_cigaretteTipRenderer != null)
            {
                _cigaretteTipRenderer.enabled = true;

                _cigaretteTipRenderer.transform.localPosition =
                    position +
                    new Vector3(
                        horizontal *
                        bounds.size.x *
                        0.045f,
                        0f,
                        0f);

                _cigaretteTipRenderer.transform.localScale =
                    Vector3.one *
                    0.035f;
            }

            if (_smokeRenderer != null)
            {
                _smokeRenderer.enabled = true;

                _smokePhase +=
                    Time.deltaTime *
                    1.6f;

                float rise =
                    Mathf.Repeat(
                        _smokePhase,
                        1f);

                _smokeRenderer.transform.localPosition =
                    position +
                    new Vector3(
                        horizontal *
                        0.03f,
                        bounds.size.y *
                        (0.05f +
                         rise * 0.14f),
                        0f);

                float smokeScale =
                    0.12f +
                    rise *
                    0.12f;

                _smokeRenderer.transform.localScale =
                    Vector3.one *
                    smokeScale;

                Color color =
                    _smokeRenderer.color;

                color.a =
                    (1f - rise) *
                    0.34f;

                _smokeRenderer.color =
                    color;
            }
        }

        private void UpdateGrenadePlay()
        {
            if (_grenadeRenderer == null)
                return;

            Bounds bounds =
                bodyRenderer.sprite.bounds;

            float t =
                Time.time *
                3.2f;

            Vector3 center =
                new Vector3(
                    bounds.center.x,
                    Mathf.Lerp(
                        bounds.min.y,
                        bounds.max.y,
                        0.55f),
                    0f);

            float radiusX =
                bounds.size.x *
                0.18f;

            float radiusY =
                bounds.size.y *
                0.12f;

            _grenadeRenderer.transform.localPosition =
                center +
                new Vector3(
                    Mathf.Cos(t) *
                    radiusX,
                    Mathf.Abs(
                        Mathf.Sin(t)) *
                    radiusY,
                    0f);

            _grenadeRenderer.transform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    t *
                    Mathf.Rad2Deg);

            float desired =
                Mathf.Min(
                    bounds.size.x,
                    bounds.size.y) *
                0.15f;

            Vector2 spriteSize =
                _grenadeRenderer.sprite.bounds.size;

            float scale =
                desired /
                Mathf.Max(
                    0.001f,
                    Mathf.Max(
                        spriteSize.x,
                        spriteSize.y));

            _grenadeRenderer.transform.localScale =
                Vector3.one *
                scale;
        }

        private void EndAction()
        {
            if (_action == IdleAction.None)
                return;

            playerWeaponVisual?.SetIdlePose(
                WeaponIdlePose.Ready);

            SetCigarette(false);

            if (_grenadeRenderer != null)
                _grenadeRenderer.enabled = false;

            _action =
                IdleAction.None;
        }

        private void SetCigarette(
            bool enabled)
        {
            if (_cigaretteRenderer != null)
                _cigaretteRenderer.enabled = enabled;

            if (!enabled)
            {
                if (_cigaretteTipRenderer != null)
                    _cigaretteTipRenderer.enabled = false;

                if (_smokeRenderer != null)
                    _smokeRenderer.enabled = false;
            }
        }

        private void SetOverlayEnabled(
            bool enabled)
        {
            if (_blinkRenderer != null)
                _blinkRenderer.enabled = enabled;

            if (_cigaretteRenderer != null)
                _cigaretteRenderer.enabled = enabled;

            if (_cigaretteTipRenderer != null)
                _cigaretteTipRenderer.enabled = enabled;

            if (_smokeRenderer != null)
                _smokeRenderer.enabled = enabled;

            if (_grenadeRenderer != null)
                _grenadeRenderer.enabled = enabled;
        }

        private void ScheduleBlink()
        {
            _nextBlink =
                Time.time +
                Random.Range(
                    blinkInterval.x,
                    blinkInterval.y);
        }

        private void ScheduleAction()
        {
            _nextAction =
                Time.time +
                Random.Range(
                    actionDelay.x,
                    actionDelay.y);
        }
    }
}
