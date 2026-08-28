using CatsAndKills.AI;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class DestructibleCover : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private float health = 85f;
        [SerializeField] private bool rebuildNavigation = true;

        public void Configure(float hp, bool rebuildNav = true)
        {
            health = hp;
            rebuildNavigation = rebuildNav;
        }

        public void ReceiveDamage(DamageInfo info)
        {
            health -= info.Amount;

            FXService.Instance?.Spark(info.Point, -info.Direction, 3);

            if (health <= 0f)
                Break(info);
        }

        private void Break(DamageInfo info)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            for (int i = 0; i < 6; i++)
            {
                GameObject shard = new GameObject("Cover Debris");
                shard.transform.position = transform.position + (Vector3)Random.insideUnitCircle * 0.3f;
                shard.transform.localScale = Vector3.one * Random.Range(0.12f, 0.28f);

                var dsr = shard.AddComponent<SpriteRenderer>();
                if (sr != null)
                {
                    dsr.sprite = sr.sprite;
                    dsr.color = sr.color * Random.Range(0.6f, 0.95f);
                    dsr.sortingOrder = sr.sortingOrder + 1;
                }

                var rb = shard.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearDamping = 2f;
                rb.AddForce(
                    (info.Direction + Random.insideUnitCircle).normalized * Random.Range(1.5f, 5f),
                    ForceMode2D.Impulse);

                shard.AddComponent<DebrisLifetime2D>().SetLifetime(Random.Range(3f, 6f), true);
            }

            Destroy(gameObject);

            if (rebuildNavigation)
            {
                NavigationGrid2D nav = FindFirstObjectByType<NavigationGrid2D>();
                if (nav != null) nav.Invoke(nameof(NavigationGrid2D.Build), 0.05f);
            }
        }
    }
}
