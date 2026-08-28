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
        [SerializeField] private float moveThreshold = 0.12f;

        private Vector2 _facing = Vector2.down;
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
            HitscanWeapon2D weapon = null)
        {
            sprites = spriteSet;
            bodyRenderer = renderer;
            vitals = characterVitals;
            body = rigidbodyRef;
            playerAim = aim;
            playerWeapon = weapon;
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
        }

        private void OnEnable()
        {
            if (vitals != null)
                vitals.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            if (vitals != null)
                vitals.Damaged -= OnDamaged;
        }

        private void LateUpdate()
        {
            if (bodyRenderer == null || sprites == null)
                return;

            UpdateFacing();
            UpdateSprite();
        }

        private void UpdateFacing()
        {
            Vector2 desired = Vector2.zero;

            if (playerAim != null)
                desired = playerAim.AimDirection;
            else if (transform.parent != null)
                desired = transform.parent.right;
            else
                desired = transform.right;

            if (desired.sqrMagnitude > 0.001f)
                _facing = desired.normalized;

            float angle =
                Mathf.Atan2(_facing.y, _facing.x) *
                Mathf.Rad2Deg;

            if (angle < 0f)
                angle += 360f;

            int sector =
                Mathf.RoundToInt(angle / 45f) % 8;

            _direction = (CharacterDirection8)sector;
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
            else if (Time.unscaledTime < _hurtUntil)
            {
                next = sprites.GetHurt(_direction);
            }
            else if (playerWeapon != null && playerWeapon.IsReloading)
            {
                next = sprites.GetReload(_direction);
            }
            else if (body != null &&
                     body.linearVelocity.sqrMagnitude >
                     moveThreshold * moveThreshold)
            {
                next = sprites.GetMove(_direction);
            }
            else
            {
                next = sprites.GetIdle(_direction);
            }

            if (next != null)
                bodyRenderer.sprite = next;
        }

        private void OnDamaged(DamageInfo info)
        {
            _hurtUntil = Time.unscaledTime + 0.16f;
        }
    }
}
