using CatsAndKills.Audio;
using CatsAndKills.Core;
using CatsAndKills.Player;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class PlayerGrenadeController : MonoBehaviour
    {
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private Sprite grenadeSprite;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip pinClip;

        [SerializeField] private int grenadeCount = 4;
        [SerializeField] private float throwForce = 8.2f;
        [SerializeField] private float returnRadius = 1.45f;
        [SerializeField] private float baseFuse = 3.15f;

        private bool _cooking;
        private float _cookStarted;

        public int GrenadeCount => grenadeCount;
        public bool IsCooking => _cooking;
        public float CookRemaining => _cooking
            ? Mathf.Max(0f, baseFuse - (Time.time - _cookStarted))
            : 0f;

        public void AddGrenades(int amount)
        {
            grenadeCount = Mathf.Clamp(grenadeCount + Mathf.Max(0, amount), 0, 9);
        }

        public void Configure(
            PlayerAim2D newAim,
            Sprite sprite,
            AudioClip explosion,
            AudioClip pin)
        {
            aim = newAim;
            grenadeSprite = sprite;
            explosionClip = explosion;
            pinClip = pin;
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

            if (CKInput.GrenadePressed && grenadeCount > 0 && !_cooking)
                BeginCook();

            if (_cooking && CKInput.GrenadeReleased)
                ThrowCooked(false);

            if (_cooking && Time.time - _cookStarted >= baseFuse)
                ThrowCooked(true);

            if (CKInput.InteractPressed)
                TryReturnGrenade();
        }

        private void BeginCook()
        {
            _cooking = true;
            _cookStarted = Time.time;

            AudioClip resolvedPin =
                pinClip != null
                    ? pinClip
                    : ProceduralAudioFactory.GrenadePin;

            if (resolvedPin != null)
                AudioSource.PlayClipAtPoint(resolvedPin, transform.position, 0.45f);
        }

        private void ThrowCooked(bool fuseExpired)
        {
            if (!_cooking || grenadeCount <= 0) return;

            float cooked = Mathf.Max(0f, Time.time - _cookStarted);
            float remainingFuse = Mathf.Max(0.05f, baseFuse - cooked);

            _cooking = false;
            grenadeCount--;
            CombatStats.Instance?.RecordGrenade();

            GameObject go = new GameObject("Player Grenade");
            go.transform.position = fuseExpired
                ? (Vector2)transform.position
                : (Vector2)transform.position + aim.AimDirection * 0.6f;

            go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>();
            var grenade = go.AddComponent<Grenade2D>();

            grenade.Configure(
                grenadeSprite,
                explosionClip,
                gameObject,
                remainingFuse);

            if (!fuseExpired)
                rb.AddForce(aim.AimDirection * throwForce, ForceMode2D.Impulse);

            rb.AddTorque(Random.Range(-240f, 240f));
        }

        private void TryReturnGrenade()
        {
            Grenade2D[] grenades = FindObjectsByType<Grenade2D>(FindObjectsSortMode.None);

            Grenade2D best = null;
            float bestDistance = float.MaxValue;

            foreach (var grenade in grenades)
            {
                float d = Vector2.Distance(transform.position, grenade.transform.position);
                if (d <= returnRadius && d < bestDistance)
                {
                    best = grenade;
                    bestDistance = d;
                }
            }

            if (best == null) return;

            best.Kick(aim.AimDirection, 9f, gameObject);
            HapticsManager.Instance?.Pulse(0.18f, 0.25f, 0.08f);
            RadioDialogueSystem.Instance?.ShowTransient("ГРАНАТА ОТПРАВЛЕНА ОБРАТНО", 0.65f);
        }
    }
}
