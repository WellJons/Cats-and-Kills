using CatsAndKills.Combat;
using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class ExplosiveProp2D : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private float health = 42f;
        [SerializeField] private float detonationDelay = 0.12f;
        [SerializeField] private Sprite explosionSprite;
        [SerializeField] private AudioClip explosionClip;

        private bool _triggered;

        public void Configure(
            float hitPoints,
            Sprite grenadeVisual,
            AudioClip explosionAudio)
        {
            health = hitPoints;
            explosionSprite = grenadeVisual;
            explosionClip = explosionAudio;
        }

        public void ReceiveDamage(DamageInfo info)
        {
            if (_triggered) return;

            health -= info.Amount;

            if (health <= 0f ||
                (info.Type == DamageType.Explosion && info.DismemberPower > 0.65f))
            {
                Trigger();
            }
        }

        private void Trigger()
        {
            if (_triggered) return;
            _triggered = true;

            Collider2D ownCollider = GetComponent<Collider2D>();
            if (ownCollider != null)
                ownCollider.enabled = false;

            SpriteRenderer ownRenderer = GetComponent<SpriteRenderer>();
            if (ownRenderer != null)
                ownRenderer.enabled = false;

            GameObject blast = new GameObject("Prop Explosion");
            blast.transform.position = transform.position;

            blast.AddComponent<SpriteRenderer>();
            var rb = blast.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            blast.AddComponent<CircleCollider2D>();

            Grenade2D grenade = blast.AddComponent<Grenade2D>();
            grenade.Configure(
                explosionSprite,
                explosionClip,
                gameObject,
                detonationDelay);

            Destroy(gameObject, detonationDelay + 0.25f);
        }
    }
}
