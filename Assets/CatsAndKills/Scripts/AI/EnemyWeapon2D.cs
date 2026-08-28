using CatsAndKills.Audio;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyWeapon2D : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shotClip;
        [SerializeField] private MuzzleFlash2D muzzleFlash;

        private Transform _target;
        private CharacterVitals _ownerVitals;
        private SuppressionReceiver2D _suppression;
        private float _range = 20f;
        private float _damage = 18f;
        private float _fireRate = 7.5f;
        private float _spreadDegrees = 2.2f;
        private float _impactForce = 2.2f;
        private float _coverDamageMultiplier = 1f;
        private int _minBurst = 2;
        private int _maxBurst = 5;
        private float _minBurstPause = 0.3f;
        private float _maxBurstPause = 0.75f;
        private LayerMask _hitMask = ~0;

        private float _nextShot;
        private int _shotsRemaining;
        private bool _triggerHeld;
        private bool _suppressing;

        public event System.Action Fired;

        public void Configure(
            Transform target,
            Transform muzzleRef,
            AudioSource source,
            AudioClip clip,
            MuzzleFlash2D flash,
            LayerMask hitMask)
        {
            _target = target;
            _ownerVitals = GetComponent<CharacterVitals>();
            _suppression = GetComponent<SuppressionReceiver2D>();
            muzzle = muzzleRef;
            audioSource = source;
            shotClip = clip;
            muzzleFlash = flash;
            _hitMask = hitMask;
        }

        public void ConfigureStats(
            float damage,
            float fireRate,
            float spread,
            int minBurst,
            int maxBurst,
            float range = 20f,
            float coverDamageMultiplier = 1f)
        {
            _damage = damage;
            _fireRate = fireRate;
            _spreadDegrees = spread;
            _minBurst = minBurst;
            _maxBurst = maxBurst;
            _range = range;
            _coverDamageMultiplier = Mathf.Max(0.1f, coverDamageMultiplier);
        }

        public void SetTrigger(bool held, bool suppressing = false)
        {
            _triggerHeld = held;
            _suppressing = suppressing;
        }

        private void Update()
        {
            if (!_triggerHeld || _target == null) return;
            if (_ownerVitals != null && !_ownerVitals.CanUsePrimaryWeapon) return;
            if (!CanFireSafely()) return;
            if (Time.time < _nextShot) return;

            if (_shotsRemaining <= 0)
            {
                _shotsRemaining = Random.Range(
                    _minBurst,
                    Mathf.Max(_minBurst + 1, _maxBurst + 1));

                float pause = _suppressing
                    ? Random.Range(0.12f, 0.28f)
                    : Random.Range(_minBurstPause, _maxBurstPause);

                _nextShot = Time.time + pause;
                return;
            }

            Fire();
            _shotsRemaining--;
            _nextShot = Time.time + 1f / Mathf.Max(0.1f, _fireRate);
        }

        private bool CanFireSafely()
        {
            if (_target == null) return false;

            Vector2 toTarget =
                CharacterCombatGeometry2D.AimPoint(
                    _target);

            Vector2 directionToTarget =
                toTarget -
                CharacterCombatGeometry2D.AimPoint(
                    transform);

            Vector2 origin =
                CharacterCombatGeometry2D.MuzzlePoint(
                    transform,
                    directionToTarget);

            Vector2 delta =
                toTarget -
                origin;
            float distance = delta.magnitude;
            if (distance < 0.01f) return false;

            RaycastHit2D[] hits = Physics2D.RaycastAll(
                origin,
                delta / distance,
                distance);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.transform.root == transform.root) continue;

                if (hit.collider.transform.root == _target.root)
                    return true;

                EnemyBrain friendly =
                    hit.collider.GetComponentInParent<EnemyBrain>();

                if (friendly != null)
                    return false;

                if (!hit.collider.isTrigger)
                    return false;
            }

            return true;
        }

        private void Fire()
        {
            Fired?.Invoke();

            Vector2 targetPoint =
                CharacterCombatGeometry2D.AimPoint(
                    _target);

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

            if (toTarget.sqrMagnitude < 0.001f)
                return;

            float extra = _suppressing ? 1.35f : 1f;
            float stability = _ownerVitals != null ? _ownerVitals.WeaponStabilityMultiplier : 1f;
            float suppressionSpread = _suppression != null
                ? Mathf.Lerp(1f, 2.3f, _suppression.Suppression)
                : 1f;

            float error = Random.Range(
                -_spreadDegrees * extra * stability * suppressionSpread,
                _spreadDegrees * extra * stability * suppressionSpread);

            Vector2 direction =
                Quaternion.Euler(0f, 0f, error) * toTarget.normalized;

            CharacterCombatGeometry2D.PlaceMuzzleTransform(
                muzzle,
                transform,
                direction);

            RaycastHit2D hit =
                CharacterCombatGeometry2D.FirstMeaningfulHit(
                    origin,
                    direction,
                    _range,
                    _hitMask,
                    transform.root);

            Vector2 endPoint = origin + direction * _range;

            if (hit.collider != null)
            {
                endPoint = hit.point;

                var receiver = hit.collider.GetComponent<IDamageReceiver>();
                if (receiver == null)
                    receiver = hit.collider.GetComponentInParent<IDamageReceiver>();

                float appliedDamage = _damage;
                if (hit.collider.GetComponentInParent<DestructibleCover>() != null)
                    appliedDamage *= _coverDamageMultiplier;

                receiver?.ReceiveDamage(new DamageInfo(
                    appliedDamage,
                    hit.point,
                    direction,
                    _impactForce,
                    gameObject,
                    DamageType.Bullet,
                    0.03f));

                if (receiver != null)
                    FXService.Instance?.BloodBurst(hit.point, direction, 4, 0.45f);
                else
                {
                    FXService.Instance?.Spark(hit.point, hit.normal, 3);
                    FXService.Instance?.BulletDecal(hit.point, hit.normal);
                }

                if (hit.rigidbody != null)
                    hit.rigidbody.AddForceAtPosition(
                        direction * _impactForce,
                        hit.point,
                        ForceMode2D.Impulse);
            }

            muzzleFlash?.Flash();

            AudioClip resolvedShot = shotClip;

            if (resolvedShot == null)
            {
                if (_fireRate > 10f)
                    resolvedShot = ProceduralAudioFactory.MachineGunShot;
                else if (_fireRate < 5f)
                    resolvedShot = ProceduralAudioFactory.PistolShot;
                else
                    resolvedShot = ProceduralAudioFactory.RifleShot;
            }

            if (resolvedShot != null)
            {
                if (audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.96f, 1.04f);
                    audioSource.PlayOneShot(resolvedShot, 0.58f);
                }
                else
                    AudioSource.PlayClipAtPoint(resolvedShot, transform.position, 0.55f);
            }

            NoiseSystem.Report(transform.position, 15f, gameObject);
            SuppressionSystem.ReportShot(origin, endPoint, _suppressing ? 0.36f : 0.22f, gameObject);

            if (_suppressing || Random.value < 0.42f)
            {
                FXService.Instance?.Tracer(
                    origin,
                    endPoint,
                    new Color(1f, 0.46f, 0.20f, 0.88f),
                    _suppressing ? 0.030f : 0.022f);
            }

            Debug.DrawLine(origin, endPoint, new Color(1f, 0.55f, 0.2f), 0.08f);
        }
    }
}
