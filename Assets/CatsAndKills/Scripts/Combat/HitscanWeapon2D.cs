using System.Collections;
using CatsAndKills.Audio;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class HitscanWeapon2D : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition definition;
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform casingPort;
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private WeaponVisualRecoil2D visualRecoil;
        [SerializeField] private MuzzleFlash2D muzzleFlash;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private CharacterVitals ownerVitals;

        private float _nextShotTime;
        private float _recoilAngle;
        private float _horizontalRecoil;
        private bool _reloading;
        private bool _semiLatch;

        public int Magazine { get; private set; }
        public int Reserve { get; private set; }
        public WeaponDefinition Definition => definition;
        public bool IsReloading => _reloading;

        public void Configure(
            WeaponDefinition newDefinition,
            PlayerAim2D newAim,
            PlayerMotor2D newMotor,
            Transform newMuzzle,
            Transform newCasingPort,
            CameraFollow2D newCamera,
            SpriteRenderer newRenderer,
            WeaponVisualRecoil2D newVisualRecoil,
            MuzzleFlash2D newMuzzleFlash,
            AudioSource newAudio)
        {
            aim = newAim;
            motor = newMotor;
            muzzle = newMuzzle;
            casingPort = newCasingPort;
            cameraFollow = newCamera;
            weaponRenderer = newRenderer;
            visualRecoil = newVisualRecoil;
            muzzleFlash = newMuzzleFlash;
            audioSource = newAudio;
            ownerVitals = GetComponentInParent<CharacterVitals>();
            SetDefinition(newDefinition, true);
        }

        public void SetDefinition(WeaponDefinition newDefinition, bool refill = false)
        {
            definition = newDefinition;
            if (definition == null) return;

            if (weaponRenderer != null && definition.weaponSprite != null)
                weaponRenderer.sprite = definition.weaponSprite;

            if (refill || Magazine <= 0)
            {
                Magazine = definition.magazineSize;
                Reserve = definition.startingReserve;
            }

            _recoilAngle = 0f;
            _horizontalRecoil = 0f;
        }

        public void SetAmmo(int magazine, int reserve)
        {
            if (definition == null) return;
            Magazine = Mathf.Clamp(magazine, 0, definition.magazineSize);
            Reserve = Mathf.Max(0, reserve);
        }

        private void Update()
        {
            if (definition == null || aim == null) return;

            RecoverRecoil();

            if (CKInput.ReloadPressed)
                TryReload();

            bool wantsFire = definition.automatic ? CKInput.FireHeld : CKInput.FirePressed;
            if (wantsFire)
                TryFire();
        }

        private void RecoverRecoil()
        {
            _recoilAngle = Mathf.MoveTowards(
                _recoilAngle,
                0f,
                definition.recoilRecovery * Time.deltaTime);

            _horizontalRecoil = Mathf.MoveTowards(
                _horizontalRecoil,
                0f,
                definition.recoilRecovery * 0.7f * Time.deltaTime);
        }

        private void TryFire()
        {
            if (_reloading || Time.time < _nextShotTime) return;
            if (ownerVitals != null && !ownerVitals.CanUsePrimaryWeapon) return;

            if (Magazine <= 0)
            {
                TryReload();
                return;
            }

            _nextShotTime = Time.time + 1f / Mathf.Max(0.01f, definition.fireRate);
            Magazine--;
            CombatStats.Instance?.RecordShot(
                Mathf.Max(1, definition.pellets));

            float movement01 = motor != null ? Mathf.Clamp01(motor.Velocity.magnitude / 7f) : 0f;
            float stability = ownerVitals != null ? ownerVitals.WeaponStabilityMultiplier : 1f;
            float spread = Mathf.Lerp(definition.baseSpread, definition.movingSpread, movement01) * stability;

            _recoilAngle = Mathf.Min(
                definition.recoilMax * stability,
                _recoilAngle + definition.recoilPerShot * stability);

            _horizontalRecoil += Random.Range(
                -definition.recoilHorizontal,
                definition.recoilHorizontal);

            for (int pellet = 0; pellet < Mathf.Max(1, definition.pellets); pellet++)
            {
                float randomSpread = Random.Range(-spread, spread);
                float shotAngle =
                    randomSpread +
                    _horizontalRecoil +
                    Random.Range(0f, _recoilAngle);

                Vector2 direction =
                    Quaternion.Euler(0f, 0f, shotAngle) * aim.AimDirection;

                FireRay(direction, definition.pellets > 1
                    ? DamageType.Pellet
                    : DamageType.Bullet);
            }

            visualRecoil?.Kick(
                definition.visualKickDistance,
                definition.visualKickRotation);

            muzzleFlash?.Flash();

            if (casingPort != null)
                FXService.Instance?.EjectCasing(
                    casingPort.position,
                    (Vector2)transform.up + Random.insideUnitCircle * 0.35f);

            AudioClip shotClip =
                definition.shotClip != null
                    ? definition.shotClip
                    : ProceduralAudioFactory.GetWeaponClip(definition.weaponName);

            if (shotClip != null)
            {
                if (audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.96f, 1.04f);
                    audioSource.PlayOneShot(shotClip, definition.shotVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(
                        shotClip,
                        transform.position,
                        definition.shotVolume);
                }
            }

            cameraFollow?.AddImpulse(
                -aim.AimDirection,
                definition.cameraKick,
                definition.cameraKickDecay);

            HapticsManager.Instance?.Pulse(
                definition.rumbleLow,
                definition.rumbleHigh,
                definition.rumbleDuration);

            NoiseSystem.Report(transform.position, 18f, gameObject);
            CombatDirector.Instance?.ReportCombat();
        }

        private void FireRay(Vector2 direction, DamageType damageType)
        {
            Vector2 origin = muzzle != null
                ? (Vector2)muzzle.position
                : (Vector2)transform.position;

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                direction,
                definition.range,
                definition.hitMask);

            Vector2 endPoint = origin + direction * definition.range;

            if (hit.collider != null)
            {
                endPoint = hit.point;

                var receiver = hit.collider.GetComponent<IDamageReceiver>();
                if (receiver == null)
                    receiver = hit.collider.GetComponentInParent<IDamageReceiver>();

                CharacterVitals hitVitals =
                    hit.collider.GetComponentInParent<CharacterVitals>();

                if (receiver != null && hitVitals != null)
                    CombatStats.Instance?.RecordHit();

                receiver?.ReceiveDamage(new DamageInfo(
                    definition.damage,
                    hit.point,
                    direction,
                    definition.impactForce,
                    gameObject,
                    damageType,
                    definition.dismemberPower));

                if (hitVitals != null &&
                    hitVitals.transform.root != transform.root)
                {
                    CrosshairUI.Instance?.FlashHit(hitVitals.IsDead);
                }

                if (receiver == null)
                {
                    FXService.Instance?.Spark(hit.point, hit.normal, 4);
                    FXService.Instance?.BulletDecal(hit.point, hit.normal);
                }
                else
                    FXService.Instance?.BloodBurst(hit.point, direction, definition.pellets > 1 ? 3 : 5, 0.55f);

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForceAtPosition(
                        direction * definition.impactForce,
                        hit.point,
                        ForceMode2D.Impulse);
                }
            }

            SuppressionSystem.ReportShot(
                origin,
                endPoint,
                definition.pellets > 1 ? 0.18f : 0.30f,
                gameObject);

            if (definition.pellets <= 1 || Random.value < 0.28f)
            {
                FXService.Instance?.Tracer(
                    origin,
                    endPoint,
                    new Color(1f, 0.74f, 0.34f, 0.95f),
                    definition.pellets > 1 ? 0.018f : 0.032f);
            }

            Debug.DrawLine(origin, endPoint, Color.red, 0.1f);
        }

        private void TryReload()
        {
            if (_reloading || definition == null) return;
            if (Magazine >= definition.magazineSize || Reserve <= 0) return;

            StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            _reloading = true;
            visualRecoil?.SetReloading(true);

            AudioClip reloadClip =
                definition.reloadClip != null
                    ? definition.reloadClip
                    : ProceduralAudioFactory.Reload;

            if (reloadClip != null && audioSource != null)
                audioSource.PlayOneShot(reloadClip, 0.5f);

            yield return new WaitForSeconds(definition.reloadTime);

            int needed = definition.magazineSize - Magazine;
            int moved = Mathf.Min(needed, Reserve);
            Magazine += moved;
            Reserve -= moved;

            _reloading = false;
            visualRecoil?.SetReloading(false);
        }
    }
}
