using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.Visual
{
    [DefaultExecutionOrder(11000)]
    [DisallowMultipleComponent]
    public sealed class EnemyWeaponVisual2D : MonoBehaviour
    {
        [SerializeField] private Transform characterRoot;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private EnemyWeapon2D weapon;
        [SerializeField] private ThreeQuarterCharacterVisual2D characterVisual;
        [SerializeField] private CharacterVitals vitals;

        private WeaponIdlePose _idlePose;
        private float _lowReadyBlend;
        private float _shoulderBlend;
        private float _kick;
        private bool _subscribed;

        public void Configure(
            Transform root,
            SpriteRenderer renderer,
            EnemyWeapon2D enemyWeapon,
            ThreeQuarterCharacterVisual2D visual)
        {
            characterRoot = root;
            weaponRenderer = renderer;
            weapon = enemyWeapon;
            characterVisual = visual;

            if (characterRoot != null)
                vitals = characterRoot.GetComponent<CharacterVitals>();

            Subscribe();
        }

        private void Awake()
        {
            if (characterRoot == null)
                characterRoot = transform.root;

            if (weaponRenderer == null)
                weaponRenderer = GetComponent<SpriteRenderer>();

            if (weapon == null &&
                characterRoot != null)
            {
                weapon =
                    characterRoot.GetComponent<EnemyWeapon2D>();
            }

            if (characterVisual == null &&
                characterRoot != null)
            {
                characterVisual =
                    characterRoot.GetComponentInChildren<
                        ThreeQuarterCharacterVisual2D>(
                        true);
            }

            if (vitals == null &&
                characterRoot != null)
            {
                vitals =
                    characterRoot.GetComponent<CharacterVitals>();
            }

            Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void SetIdlePose(
            WeaponIdlePose pose)
        {
            _idlePose = pose;
        }

        private void Subscribe()
        {
            if (_subscribed ||
                weapon == null)
            {
                return;
            }

            weapon.Fired += OnFired;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            if (weapon != null)
                weapon.Fired -= OnFired;

            _subscribed = false;
        }

        private void OnFired()
        {
            _kick =
                Mathf.Max(
                    _kick,
                    0.085f);
        }

        private void LateUpdate()
        {
            if (weaponRenderer == null ||
                characterVisual == null ||
                characterRoot == null ||
                weaponRenderer.sprite == null)
            {
                return;
            }

            if (vitals != null &&
                vitals.IsDead)
            {
                weaponRenderer.enabled = false;
                return;
            }

            weaponRenderer.enabled = true;
            weaponRenderer.forceRenderingOff = false;

            float dt =
                Mathf.Max(
                    0f,
                    Time.deltaTime);

            _kick =
                Mathf.MoveTowards(
                    _kick,
                    0f,
                    dt * 0.85f);

            _lowReadyBlend =
                Mathf.MoveTowards(
                    _lowReadyBlend,
                    _idlePose ==
                    WeaponIdlePose.LowReady
                        ? 1f
                        : 0f,
                    dt * 4.0f);

            _shoulderBlend =
                Mathf.MoveTowards(
                    _shoulderBlend,
                    _idlePose ==
                    WeaponIdlePose.Shoulder
                        ? 1f
                        : 0f,
                    dt * 3.8f);

            Vector2 direction =
                characterVisual.Facing.sqrMagnitude > 0.001f
                    ? characterVisual.Facing.normalized
                    : Vector2.right;

            Vector2 aimPoint =
                CharacterCombatGeometry2D.AimPoint(
                    characterRoot);

            Vector2 muzzlePoint =
                CharacterCombatGeometry2D.MuzzlePoint(
                    characterRoot,
                    direction);

            float anchorDistance =
                Vector2.Distance(
                    aimPoint,
                    muzzlePoint);

            float targetLength =
                Mathf.Clamp(
                    anchorDistance * 1.03f,
                    0.30f,
                    1.05f);

            float spriteWidth =
                Mathf.Max(
                    0.01f,
                    weaponRenderer.sprite.bounds.size.x);

            float scale =
                targetLength /
                spriteWidth;

            Vector2 center =
                muzzlePoint -
                direction *
                (targetLength * 0.50f);

            center -=
                direction *
                _kick;

            center +=
                Vector2.down *
                0.18f *
                _lowReadyBlend;

            Vector2 shoulderTarget =
                aimPoint -
                direction *
                (targetLength * 0.08f) +
                Vector2.up *
                0.16f;

            center =
                Vector2.Lerp(
                    center,
                    shoulderTarget,
                    _shoulderBlend);

            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x) *
                Mathf.Rad2Deg;

            float handedness =
                direction.x < -0.01f
                    ? -1f
                    : 1f;

            angle +=
                -24f *
                _lowReadyBlend *
                handedness;

            angle +=
                58f *
                _shoulderBlend *
                handedness;

            transform.position =
                new Vector3(
                    center.x,
                    center.y,
                    transform.position.z);

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle);

            transform.localScale =
                Vector3.one *
                scale;

            weaponRenderer.flipY =
                direction.x < -0.01f;
        }
    }
}
