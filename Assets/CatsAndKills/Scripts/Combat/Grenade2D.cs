using System.Collections.Generic;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using UnityEngine;

namespace CatsAndKills.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class Grenade2D : MonoBehaviour
    {
        [SerializeField] private float fuse = 3.2f;
        [SerializeField] private float radius = 4.0f;
        [SerializeField] private float maxDamage = 130f;
        [SerializeField] private float explosionForce = 12f;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private float _explodeAt;
        private bool _exploded;
        private GameObject _owner;

        public float RemainingFuse => Mathf.Max(0f, _explodeAt - Time.time);
        public GameObject Owner => _owner;

        public void Configure(
            Sprite sprite,
            AudioClip clip,
            GameObject owner,
            float fuseSeconds = 3.2f)
        {
            _owner = owner;
            fuse = fuseSeconds;
            explosionClip = clip;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = 25;
            transform.localScale = Vector3.one * 0.5f;
        }

        private void Start()
        {
            _explodeAt = Time.time + fuse;

            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.75f;
            rb.angularDamping = 0.5f;

            GetComponent<CircleCollider2D>().radius = 0.32f;
        }

        public void Kick(Vector2 direction, float force, GameObject newOwner)
        {
            _owner = newOwner;

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity *= 0.3f;
                rb.AddForce(
                    direction.normalized * force,
                    ForceMode2D.Impulse);

                rb.AddTorque(Random.Range(-220f, 220f));
            }
        }

        private void Update()
        {
            if (!_exploded && Time.time >= _explodeAt)
                Explode();
        }

        private void Explode()
        {
            if (_exploded) return;
            _exploded = true;

            if (explosionClip != null)
                AudioSource.PlayClipAtPoint(
                    explosionClip,
                    transform.position,
                    0.95f);

            HapticsManager.Instance?.Pulse(0.75f, 0.55f, 0.22f);

            Camera.main?
                .GetComponent<CatsAndKills.Player.CameraFollow2D>()?
                .AddImpulse(Random.insideUnitCircle, 0.42f, 12f);

            Collider2D[] colliders =
                Physics2D.OverlapCircleAll(transform.position, radius);

            var nearestHitbox =
                new Dictionary<CharacterVitals, BodyPartHitbox>();

            var nearestDistance =
                new Dictionary<CharacterVitals, float>();

            var allBodyHitboxes = new List<BodyPartHitbox>();

            foreach (Collider2D col in colliders)
            {
                if (col == null) continue;

                BodyPartHitbox body =
                    col.GetComponent<BodyPartHitbox>();

                if (body != null && body.Owner != null)
                {
                    allBodyHitboxes.Add(body);

                    float d = Vector2.Distance(
                        transform.position,
                        col.bounds.center);

                    if (!nearestDistance.ContainsKey(body.Owner) ||
                        d < nearestDistance[body.Owner])
                    {
                        nearestDistance[body.Owner] = d;
                        nearestHitbox[body.Owner] = body;
                    }
                }

                Vector2 delta =
                    (Vector2)col.bounds.center -
                    (Vector2)transform.position;

                float distance = Mathf.Max(0.25f, delta.magnitude);
                float falloff =
                    Mathf.Clamp01(1f - distance / radius);

                if (col.attachedRigidbody != null)
                {
                    col.attachedRigidbody.AddForce(
                        delta.normalized *
                        explosionForce *
                        falloff,
                        ForceMode2D.Impulse);
                }

                var destructible =
                    col.GetComponent<DestructibleCover>();

                destructible?.ReceiveDamage(
                    new DamageInfo(
                        maxDamage * falloff,
                        col.ClosestPoint(transform.position),
                        delta.normalized,
                        explosionForce,
                        _owner,
                        DamageType.Explosion,
                        1f));
            }

            foreach (var pair in nearestHitbox)
            {
                CharacterVitals owner = pair.Key;
                BodyPartHitbox body = pair.Value;

                float distance = nearestDistance[owner];
                float falloff =
                    Mathf.Clamp01(1f - distance / radius);

                Vector2 point = body.transform.position;
                Vector2 direction =
                    (point - (Vector2)transform.position).normalized;

                float damage =
                    maxDamage *
                    Mathf.Lerp(0.25f, 1f, falloff);

                body.ReceiveDamage(
                    new DamageInfo(
                        damage,
                        point,
                        direction,
                        explosionForce * falloff,
                        _owner,
                        DamageType.Explosion,
                        Mathf.Lerp(0.2f, 1f, falloff)));

                FXService.Instance?.BloodBurst(
                    point,
                    direction,
                    Mathf.RoundToInt(
                        Mathf.Lerp(5, 14, falloff)),
                    Mathf.Lerp(0.7f, 1.6f, falloff));
            }

            var processedLimbs = new HashSet<BodyPartHitbox>();

            foreach (BodyPartHitbox limb in allBodyHitboxes)
            {
                if (limb == null ||
                    processedLimbs.Contains(limb) ||
                    limb.Part == BodyPart.Torso ||
                    limb.Part == BodyPart.Head)
                    continue;

                processedLimbs.Add(limb);

                float distance =
                    Vector2.Distance(
                        transform.position,
                        limb.transform.position);

                float falloff =
                    Mathf.Clamp01(1f - distance / radius);

                if (falloff < 0.52f)
                    continue;

                Vector2 direction =
                    ((Vector2)limb.transform.position -
                     (Vector2)transform.position).normalized;

                limb.ReceiveDamage(
                    new DamageInfo(
                        Mathf.Lerp(12f, 32f, falloff),
                        limb.transform.position,
                        direction,
                        explosionForce * falloff,
                        _owner,
                        DamageType.Explosion,
                        Mathf.Lerp(0.58f, 1f, falloff)));
            }

            FXService.Instance?.ExplosionBurst(transform.position);
            FXService.Instance?.Spark(
                transform.position,
                Vector2.up,
                16);

            CombatDirector.Instance?.ReportCombat();
            NoiseSystem.Report(transform.position, 24f, _owner);

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
