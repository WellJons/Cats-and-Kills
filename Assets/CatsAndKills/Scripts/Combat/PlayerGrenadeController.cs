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

        public int GrenadeCount => grenadeCount;

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
            if (CKInput.GrenadePressed && grenadeCount > 0)
                ThrowGrenade();

            if (CKInput.InteractPressed)
                TryReturnGrenade();
        }

        private void ThrowGrenade()
        {
            grenadeCount--;
            CombatStats.Instance?.RecordGrenade();

            if (pinClip != null)
                AudioSource.PlayClipAtPoint(pinClip, transform.position, 0.45f);

            GameObject go = new GameObject("Player Grenade");
            go.transform.position = (Vector2)transform.position + aim.AimDirection * 0.6f;

            var sr = go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>();
            var grenade = go.AddComponent<Grenade2D>();

            grenade.Configure(grenadeSprite, explosionClip, gameObject, 3.15f);
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
