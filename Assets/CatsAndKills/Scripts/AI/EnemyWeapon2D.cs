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
        private int _minBurst = 2;
        private int _maxBurst = 5;
        private float _minBurstPause = 0.3f;
        private float _maxBurstPause = 0.75f;
        private LayerMask _hitMask = ~0;

        private float _nextShot;
        private int _shotsRemaining;
        private bool _triggerHeld;
        private bool _suppressing;

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
            float range = 20f)
        {
            _damage = damage;
            _fireRate = fireRate;
            _spreadDegrees = spread;
            _minBurst = minBurst;
            _maxBurst = maxBurst;
            _range = range;
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

        private void Fire()
        {
            Vector2 origin = muzzle != null
                ? (Vector2)muzzle.position
                : (Vector2)transform.position;

            Vector2 toTarget = (Vector2)_target.position - origin;
            if (toTarget.sqrMagnitude < 0.001f) return;

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

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                direction,
                _range,
                _hitMask);

            Vector2 endPoint = origin + direction * _range;

            if (hit.collider != null)
            {
                endPoint = hit.point;

                var receiver = hit.collider.GetComponent<IDamageReceiver>();
                if (receiver == null)
                    receiver = hit.collider.GetComponentInParent<IDamageReceiver>();

                receiver?.ReceiveDamage(new DamageInfo(
                    _damage,
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

            if (shotClip != null)
            {
                if (audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.96f, 1.04f);
                    audioSource.PlayOneShot(shotClip, 0.58f);
                }
                else
                    AudioSource.PlayClipAtPoint(shotClip, transform.position, 0.55f);
            }

            NoiseSystem.Report(transform.position, 15f, gameObject);
            SuppressionSystem.ReportShot(origin, endPoint, _suppressing ? 0.36f : 0.22f, gameObject);
            Debug.DrawLine(origin, endPoint, new Color(1f, 0.55f, 0.2f), 0.08f);
        }
    }
}
