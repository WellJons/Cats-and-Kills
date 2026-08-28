using CatsAndKills.Audio;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyWeapon2D : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;
        [SerializeField] private Transform muzzle;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shotClip;
        [SerializeField] private MuzzleFlash2D muzzleFlash;

        [Header("Weapon")]
        [SerializeField] private float range = 20f;
        [SerializeField] private float damage = 18f;
        [SerializeField] private float fireRate = 7.5f;
        [SerializeField] private float spreadDegrees = 2.2f;
        [SerializeField] private float impactForce = 2.2f;
        [SerializeField] private float coverDamageMultiplier = 1f;
        [SerializeField] private int minBurst = 2;
        [SerializeField] private int maxBurst = 5;
        [SerializeField] private float minBurstPause = 0.3f;
        [SerializeField] private float maxBurstPause = 0.75f;
        [SerializeField] private LayerMask hitMask = ~0;

        private CharacterVitals _ownerVitals;
        private SuppressionReceiver2D _suppression;
        private float _nextShot;
        private int _shotsRemaining;
        private bool _triggerHeld;
        private bool _suppressing;

        public event System.Action Fired;

        public void Configure(
            Transform targetTransform,
            Transform muzzleRef,
            AudioSource source,
            AudioClip clip,
            MuzzleFlash2D flash,
            LayerMask mask)
        {
            target = targetTransform;
            muzzle = muzzleRef;
            audioSource = source;
            shotClip = clip;
            muzzleFlash = flash;
            hitMask = mask;

            CacheRuntimeReferences();
        }

        public void ConfigureStats(
            float newDamage,
            float newFireRate,
            float newSpread,
            int newMinBurst,
            int newMaxBurst,
            float newRange = 20f,
            float newCoverDamageMultiplier = 1f)
        {
            damage = newDamage;
            fireRate = newFireRate;
            spreadDegrees = newSpread;
            minBurst = newMinBurst;
            maxBurst = newMaxBurst;
            range = newRange;
            coverDamageMultiplier =
                Mathf.Max(
                    0.1f,
                    newCoverDamageMultiplier);
        }

        private void Awake()
        {
            CacheRuntimeReferences();
        }

        private void OnEnable()
        {
            CacheRuntimeReferences();

            _shotsRemaining = 0;
            _nextShot =
                Time.time +
                Random.Range(
                    0.08f,
                    0.24f);
        }

        private void OnDisable()
        {
            _triggerHeld = false;
            _shotsRemaining = 0;
        }

        private void CacheRuntimeReferences()
        {
            if (_ownerVitals == null)
            {
                _ownerVitals =
                    GetComponent<CharacterVitals>();
            }

            if (_suppression == null)
            {
                _suppression =
                    GetComponent<SuppressionReceiver2D>();
            }

            if (target == null)
            {
                PlayerMotor2D player =
                    FindAnyObjectByType<PlayerMotor2D>();

                if (player != null)
                    target = player.transform;
            }
        }

        public void SetTrigger(
            bool held,
            bool suppressing = false)
        {
            if (_ownerVitals != null &&
                _ownerVitals.IsDead)
            {
                _triggerHeld = false;
                return;
            }

            _triggerHeld = held;
            _suppressing = suppressing;
        }

        private void Update()
        {
            if (!_triggerHeld)
                return;

            if (target == null)
            {
                CacheRuntimeReferences();

                if (target == null)
                    return;
            }

            if (_ownerVitals != null &&
                (!_ownerVitals.CanUsePrimaryWeapon ||
                 _ownerVitals.IsDead))
            {
                return;
            }

            if (!CanFireSafely())
                return;

            if (Time.time < _nextShot)
                return;

            if (_shotsRemaining <= 0)
            {
                _shotsRemaining =
                    Random.Range(
                        Mathf.Max(
                            1,
                            minBurst),
                        Mathf.Max(
                            minBurst + 1,
                            maxBurst + 1));

                float pause =
                    _suppressing
                        ? Random.Range(
                            0.08f,
                            0.20f)
                        : Random.Range(
                            minBurstPause,
                            maxBurstPause);

                _nextShot =
                    Time.time +
                    pause;

                return;
            }

            Fire();

            _shotsRemaining--;

            _nextShot =
                Time.time +
                1f /
                Mathf.Max(
                    0.1f,
                    fireRate);
        }

        private bool CanFireSafely()
        {
            if (target == null)
                return false;

            Vector2 targetPoint =
                CharacterCombatGeometry2D.AimPoint(
                    target);

            Vector2 directionToTarget =
                targetPoint -
                CharacterCombatGeometry2D.AimPoint(
                    transform);

            if (directionToTarget.sqrMagnitude <
                0.001f)
            {
                return false;
            }

            Vector2 origin =
                CharacterCombatGeometry2D.MuzzlePoint(
                    transform,
                    directionToTarget);

            Vector2 delta =
                targetPoint -
                origin;

            float distance =
                delta.magnitude;

            if (distance < 0.01f ||
                distance > range + 0.35f)
            {
                return false;
            }

            RaycastHit2D[] hits =
                Physics2D.RaycastAll(
                    origin,
                    delta / distance,
                    distance);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (hit.collider.transform.root ==
                    transform.root)
                {
                    continue;
                }

                if (hit.collider.transform.root ==
                    target.root)
                {
                    return true;
                }

                EnemyBrain friendly =
                    hit.collider.GetComponentInParent<
                        EnemyBrain>();

                if (friendly != null)
                    return false;

                if (!hit.collider.isTrigger)
                    return false;
            }

            // If no blocking collider was found, the target is still reachable.
            // The player may have only trigger body-part hitboxes active.
            return true;
        }

        private void Fire()
        {
            Fired?.Invoke();

            Vector2 targetPoint =
                CharacterCombatGeometry2D.AimPoint(
                    target);

            Vector2 provisionalDirection =
                targetPoint -
                CharacterCombatGeometry2D.AimPoint(
                    transform);

            Vector2 origin =
                CharacterCombatGeometry2D.MuzzlePoint(
                    transform,
                    provisionalDirection);

            Vector2 toTarget =
                targetPoint -
                origin;

            if (toTarget.sqrMagnitude <
                0.001f)
            {
                return;
            }

            float extra =
                _suppressing
                    ? 1.25f
                    : 1f;

            float stability =
                _ownerVitals != null
                    ? _ownerVitals
                        .WeaponStabilityMultiplier
                    : 1f;

            float suppressionSpread =
                _suppression != null
                    ? Mathf.Lerp(
                        1f,
                        2.1f,
                        _suppression.Suppression)
                    : 1f;

            float error =
                Random.Range(
                    -spreadDegrees *
                    extra *
                    stability *
                    suppressionSpread,
                    spreadDegrees *
                    extra *
                    stability *
                    suppressionSpread);

            Vector2 direction =
                Quaternion.Euler(
                    0f,
                    0f,
                    error) *
                toTarget.normalized;

            CharacterCombatGeometry2D
                .PlaceMuzzleTransform(
                    muzzle,
                    transform,
                    direction);

            RaycastHit2D hit =
                CharacterCombatGeometry2D
                    .FirstMeaningfulHit(
                        origin,
                        direction,
                        range,
                        hitMask,
                        transform.root);

            Vector2 endPoint =
                origin +
                direction *
                range;

            if (hit.collider != null)
            {
                endPoint = hit.point;

                IDamageReceiver receiver =
                    hit.collider
                        .GetComponent<
                            IDamageReceiver>();

                if (receiver == null)
                {
                    receiver =
                        hit.collider
                            .GetComponentInParent<
                                IDamageReceiver>();
                }

                float appliedDamage =
                    damage;

                if (hit.collider
                    .GetComponentInParent<
                        DestructibleCover>() != null)
                {
                    appliedDamage *=
                        coverDamageMultiplier;
                }

                receiver?.ReceiveDamage(
                    new DamageInfo(
                        appliedDamage,
                        hit.point,
                        direction,
                        impactForce,
                        gameObject,
                        DamageType.Bullet,
                        0.03f));

                if (receiver != null)
                {
                    FXService.Instance
                        ?.BloodBurst(
                            hit.point,
                            direction,
                            4,
                            0.45f);
                }
                else
                {
                    FXService.Instance
                        ?.Spark(
                            hit.point,
                            hit.normal,
                            3);

                    FXService.Instance
                        ?.BulletDecal(
                            hit.point,
                            hit.normal);
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody
                        .AddForceAtPosition(
                            direction *
                            impactForce,
                            hit.point,
                            ForceMode2D.Impulse);
                }
            }

            muzzleFlash?.Flash();

            AudioClip resolvedShot =
                shotClip;

            if (resolvedShot == null)
            {
                if (fireRate > 10f)
                {
                    resolvedShot =
                        ProceduralAudioFactory
                            .MachineGunShot;
                }
                else if (fireRate < 5f)
                {
                    resolvedShot =
                        ProceduralAudioFactory
                            .PistolShot;
                }
                else
                {
                    resolvedShot =
                        ProceduralAudioFactory
                            .RifleShot;
                }
            }

            if (resolvedShot != null)
            {
                if (audioSource != null)
                {
                    audioSource.pitch =
                        Random.Range(
                            0.96f,
                            1.04f);

                    audioSource.PlayOneShot(
                        resolvedShot,
                        0.58f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(
                        resolvedShot,
                        transform.position,
                        0.55f);
                }
            }

            NoiseSystem.Report(
                transform.position,
                15f,
                gameObject);

            SuppressionSystem.ReportShot(
                origin,
                endPoint,
                _suppressing
                    ? 0.36f
                    : 0.22f,
                gameObject);

            if (_suppressing ||
                Random.value < 0.50f)
            {
                FXService.Instance?.Tracer(
                    origin,
                    endPoint,
                    new Color(
                        1f,
                        0.46f,
                        0.20f,
                        0.88f),
                    _suppressing
                        ? 0.030f
                        : 0.022f);
            }

            Debug.DrawLine(
                origin,
                endPoint,
                new Color(
                    1f,
                    0.55f,
                    0.2f),
                0.08f);
        }
    }
}
