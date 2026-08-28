using CatsAndKills.Audio;
using UnityEngine;

namespace CatsAndKills.FX
{
    public sealed class FXService : MonoBehaviour
    {
        public static FXService Instance { get; private set; }

        [Header("Sprites")]
        public Sprite bloodSprite;
        public Sprite sparkSprite;
        public Sprite casingSprite;
        public Sprite bulletHoleSprite;
        public Sprite explosionSprite;
        public Sprite smokeSprite;

        [Header("Audio")]
        public AudioClip casingClip;

        private void Awake()
        {
            Instance = this;
        }

        public void BloodBurst(Vector2 position, Vector2 direction, int count = 7, float force = 1f)
        {
            if (bloodSprite != null && count >= 5)
            {
                GameObject decal = new GameObject("Blood Decal");
                decal.transform.position = position + Random.insideUnitCircle * 0.08f;
                decal.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                decal.transform.localScale =
                    new Vector3(
                        Random.Range(0.24f, 0.48f),
                        Random.Range(0.14f, 0.32f),
                        1f);

                var decalRenderer = decal.AddComponent<SpriteRenderer>();
                decalRenderer.sprite = bloodSprite;
                decalRenderer.color = new Color(0.42f, 0.015f, 0.025f, 0.82f);
                decalRenderer.sortingOrder = 1;
            }

            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("Blood");
                go.transform.position = position;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = bloodSprite;
                sr.sortingOrder = 20;
                sr.color = new Color(0.72f, 0.03f, 0.07f, 0.95f);
                go.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearDamping = 2.2f;
                Vector2 dir = (direction.normalized + Random.insideUnitCircle * 0.85f).normalized;
                rb.AddForce(dir * Random.Range(force * 1.2f, force * 4f), ForceMode2D.Impulse);

                go.AddComponent<DebrisLifetime2D>().SetLifetime(Random.Range(3f, 6f), true);
            }
        }

        public void Spark(Vector2 position, Vector2 normal, int count = 4)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("Spark");
                go.transform.position = position;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sparkSprite;
                sr.sortingOrder = 30;
                go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.11f);

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearDamping = 4f;
                Vector2 dir = (normal + Random.insideUnitCircle).normalized;
                rb.AddForce(dir * Random.Range(1.5f, 4.5f), ForceMode2D.Impulse);

                go.AddComponent<DebrisLifetime2D>().SetLifetime(Random.Range(0.15f, 0.35f), true);
            }
        }

        public void BulletDecal(Vector2 position, Vector2 normal)
        {
            if (bulletHoleSprite == null) return;

            GameObject go = new GameObject("Bullet Mark");
            go.transform.position = position + normal * 0.015f;
            go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            go.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bulletHoleSprite;
            sr.sortingOrder = 5;

            go.AddComponent<DebrisLifetime2D>().SetLifetime(18f, true);
        }

        public void ExplosionBurst(Vector2 position)
        {
            if (explosionSprite != null)
            {
                GameObject flash = new GameObject("Explosion Flash");
                flash.transform.position = position;
                flash.transform.localScale = Vector3.one * 1.7f;
                var sr = flash.AddComponent<SpriteRenderer>();
                sr.sprite = explosionSprite;
                sr.sortingOrder = 45;

                flash.AddComponent<ExpandingFadeFX>()
                    .Configure(
                        0.28f,
                        1.7f,
                        2.7f);

                flash.AddComponent<TransientLight2D>()
                    .Configure(
                        new Color(
                            1f,
                            0.34f,
                            0.08f),
                        2.6f,
                        5.0f,
                        0.24f);
            }

            if (smokeSprite != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    GameObject smoke = new GameObject("Explosion Smoke");
                    smoke.transform.position = position + Random.insideUnitCircle * 0.35f;
                    smoke.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                    smoke.transform.localScale = Vector3.one * Random.Range(0.7f, 1.2f);

                    var sr = smoke.AddComponent<SpriteRenderer>();
                    sr.sprite = smokeSprite;
                    sr.sortingOrder = 35;
                    sr.color = new Color(0.65f, 0.68f, 0.74f, Random.Range(0.28f, 0.45f));

                    smoke.AddComponent<ExpandingFadeFX>().Configure(
                        Random.Range(1.1f, 1.7f),
                        0.8f,
                        Random.Range(1.4f, 2.1f));
                }
            }
        }

        public void Tracer(
            Vector2 start,
            Vector2 end,
            Color color,
            float width = 0.035f)
        {
            GameObject go = new GameObject("Tracer");
            go.AddComponent<TracerFX2D>().Configure(
                start,
                end,
                color,
                width);
        }

        public void EjectCasing(Vector2 position, Vector2 direction)
        {
            GameObject go = new GameObject("Casing");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = casingSprite;
            sr.sortingOrder = 12;
            go.transform.localScale = Vector3.one * 0.11f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 2.5f;
            rb.AddForce((direction + Random.insideUnitCircle * 0.5f).normalized * Random.Range(1.3f, 2.7f), ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-360f, 360f));

            go.AddComponent<DebrisLifetime2D>().SetLifetime(5f, true);

            AudioClip resolvedCasing =
                casingClip != null
                    ? casingClip
                    : ProceduralAudioFactory.Casing;

            if (resolvedCasing != null && Random.value < 0.55f)
                AudioSource.PlayClipAtPoint(resolvedCasing, position, 0.16f);
        }
    }
}
