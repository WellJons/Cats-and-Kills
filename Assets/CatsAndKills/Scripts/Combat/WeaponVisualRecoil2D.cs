using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public enum WeaponIdlePose
    {
        Ready,
        LowReady,
        Shoulder
    }

    [DefaultExecutionOrder(11000)]
    public sealed class WeaponVisualRecoil2D : MonoBehaviour
    {
        [SerializeField] private Transform characterRoot;
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private SpriteRenderer weaponRenderer;
        [SerializeField] private HitscanWeapon2D weapon;
        [SerializeField] private bool anchorToCharacter = true;
        [SerializeField] private float visualLengthMultiplier = 1.05f;

        private Vector3 _basePos;
        private Quaternion _baseRot;
        private Vector3 _baseScale;
        private float _kickDistance;
        private float _kickRotation;
        private float _returnSharpness = 24f;
        private float _reloadBlend;
        private bool _reloading;
        private WeaponIdlePose _idlePose;
        private float _lowReadyBlend;
        private float _shoulderBlend;

        private void Awake()
        {
            _basePos = transform.localPosition;
            _baseRot = transform.localRotation;
            _baseScale = transform.localScale;

            if (weaponRenderer == null)
                weaponRenderer = GetComponent<SpriteRenderer>();

            if (weapon == null)
                weapon = GetComponent<HitscanWeapon2D>();

            if (aim == null)
                aim = GetComponentInParent<PlayerAim2D>();

            if (characterRoot == null &&
                aim != null)
            {
                characterRoot = aim.transform;
            }
        }

        public void ConfigureAnchor(
            Transform root,
            PlayerAim2D aimSource,
            SpriteRenderer renderer,
            HitscanWeapon2D weaponSource)
        {
            characterRoot = root;
            aim = aimSource;
            weaponRenderer = renderer;
            weapon = weaponSource;
            anchorToCharacter = true;

            if (weaponRenderer != null)
            {
                weaponRenderer.enabled = true;
                weaponRenderer.forceRenderingOff = false;
            }
        }

        public void Kick(
            float distance,
            float rotation)
        {
            _kickDistance += distance;
            _kickRotation +=
                Random.Range(
                    -rotation,
                    rotation);
        }

        public void SetReloading(
            bool value)
        {
            _reloading = value;
        }

        public void SetIdlePose(
            WeaponIdlePose pose)
        {
            _idlePose = pose;
        }

        private void LateUpdate()
        {
            float dt =
                Mathf.Max(
                    0f,
                    Time.deltaTime);

            float t =
                1f -
                Mathf.Exp(
                    -_returnSharpness *
                    dt);

            _kickDistance =
                Mathf.Lerp(
                    _kickDistance,
                    0f,
                    t);

            _kickRotation =
                Mathf.Lerp(
                    _kickRotation,
                    0f,
                    t);

            _reloadBlend =
                Mathf.MoveTowards(
                    _reloadBlend,
                    _reloading ? 1f : 0f,
                    dt * 5.8f);

            _lowReadyBlend =
                Mathf.MoveTowards(
                    _lowReadyBlend,
                    _idlePose ==
                    WeaponIdlePose.LowReady
                        ? 1f
                        : 0f,
                    dt * 4.2f);

            _shoulderBlend =
                Mathf.MoveTowards(
                    _shoulderBlend,
                    _idlePose ==
                    WeaponIdlePose.Shoulder
                        ? 1f
                        : 0f,
                    dt * 3.8f);

            if (anchorToCharacter &&
                TryApplyCharacterAnchoredPose())
            {
                return;
            }

            Vector3 reloadOffset =
                new Vector3(
                    -0.12f,
                    -0.18f,
                    0f) *
                _reloadBlend;

            float reloadRotation =
                -38f *
                _reloadBlend;

            transform.localPosition =
                _basePos +
                Vector3.left *
                _kickDistance +
                reloadOffset;

            transform.localRotation =
                _baseRot *
                Quaternion.Euler(
                    0f,
                    0f,
                    _kickRotation +
                    reloadRotation);

            transform.localScale =
                _baseScale;
        }

        private bool TryApplyCharacterAnchoredPose()
        {
            if (characterRoot == null ||
                aim == null ||
                weaponRenderer == null ||
                weaponRenderer.sprite == null)
            {
                return false;
            }

            Vector2 direction =
                aim.AimDirection.sqrMagnitude >
                0.001f
                    ? aim.AimDirection.normalized
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
                    anchorDistance *
                    visualLengthMultiplier *
                    GetWeaponLengthMultiplier(),
                    0.30f,
                    1.05f);

            float spriteWidth =
                Mathf.Max(
                    0.01f,
                    weaponRenderer.sprite.bounds.size.x);

            float scale =
                targetLength /
                spriteWidth;

            Vector2 perpendicular =
                new Vector2(
                    -direction.y,
                    direction.x);

            Vector2 center =
                muzzlePoint -
                direction *
                (targetLength * 0.50f);

            center -=
                direction *
                _kickDistance;

            center +=
                (-direction * 0.10f -
                 perpendicular * 0.10f) *
                _reloadBlend;

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
                direction.x <
                -0.01f
                    ? -1f
                    : 1f;

            float reloadRotation =
                -34f *
                _reloadBlend *
                handedness;

            float lowReadyRotation =
                -24f *
                _lowReadyBlend *
                handedness;

            float shoulderRotation =
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
                    angle +
                    _kickRotation +
                    reloadRotation +
                    lowReadyRotation +
                    shoulderRotation);

            transform.localScale =
                Vector3.one *
                scale;

            weaponRenderer.flipY =
                direction.x <
                -0.01f;

            weaponRenderer.enabled = true;
            weaponRenderer.forceRenderingOff = false;

            return true;
        }

        private float GetWeaponLengthMultiplier()
        {
            WeaponDefinition definition =
                weapon != null
                    ? weapon.Definition
                    : null;

            if (definition == null ||
                string.IsNullOrEmpty(
                    definition.weaponName))
            {
                return 1f;
            }

            string name =
                definition.weaponName
                    .ToLowerInvariant();

            if (name.Contains("pistol"))
                return 0.62f;

            if (name.Contains("ks-12") ||
                name.Contains("shotgun"))
            {
                return 1.08f;
            }

            return 1f;
        }
    }
}
