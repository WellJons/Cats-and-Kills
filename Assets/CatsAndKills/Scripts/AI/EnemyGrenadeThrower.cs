using CatsAndKills.Combat;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyGrenadeThrower : MonoBehaviour
    {
        [SerializeField] private Sprite grenadeSprite;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private Transform target;

        [Header("Behaviour")]
        [SerializeField] private int grenadeCount = 1;
        [SerializeField] private float minDistance = 4f;
        [SerializeField] private float maxDistance = 10.5f;
        [SerializeField] private float initialDelayMin = 3.5f;
        [SerializeField] private float initialDelayMax = 6.5f;

        private float _nextThrow;

        public void Configure(
            Transform targetTransform,
            Sprite sprite,
            AudioClip explosion,
            int count)
        {
            target = targetTransform;
            grenadeSprite = sprite;
            explosionClip = explosion;
            grenadeCount = Mathf.Max(0, count);
            ScheduleInitialThrow();
        }

        private void Awake()
        {
            ScheduleInitialThrow();
        }

        private void OnEnable()
        {
            ScheduleInitialThrow();
        }

        private void ScheduleInitialThrow()
        {
            if (_nextThrow > Time.time)
                return;

            _nextThrow =
                Time.time +
                Random.Range(
                    Mathf.Max(0.5f, initialDelayMin),
                    Mathf.Max(
                        initialDelayMin + 0.25f,
                        initialDelayMax));
        }

        public bool TryThrow(bool hasVisual, float aggression)
        {
            if (!hasVisual ||
                target == null ||
                grenadeCount <= 0)
            {
                return false;
            }

            if (Time.time < _nextThrow)
                return false;

            float distance = Vector2.Distance(transform.position, target.position);
            if (distance < minDistance || distance > maxDistance)
                return false;

            Collider2D[] nearby =
                Physics2D.OverlapCircleAll(target.position, 2.8f);

            foreach (Collider2D col in nearby)
            {
                if (col == null ||
                    col.transform.root == transform.root)
                    continue;

                if (col.GetComponentInParent<EnemyBrain>() != null)
                {
                    _nextThrow = Time.time + Random.Range(1.6f, 2.8f);
                    return false;
                }
            }

            float chance = Mathf.Lerp(0.08f, 0.26f, aggression);
            if (Random.value > chance)
            {
                _nextThrow = Time.time + Random.Range(2.5f, 4.5f);
                return false;
            }

            Throw();
            return true;
        }

        private void Throw()
        {
            grenadeCount--;
            _nextThrow = Time.time + Random.Range(9f, 14f);

            Vector2 origin = transform.position;
            Vector2 targetPos = target.position;
            Vector2 direction = (targetPos - origin).normalized;
            float distance = Vector2.Distance(origin, targetPos);
            float force = Mathf.Clamp(distance * 0.85f, 5.5f, 9f);

            GameObject go = new GameObject("Enemy Grenade");
            go.transform.position = origin + direction * 0.65f;

            go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>();
            var grenade = go.AddComponent<Grenade2D>();

            grenade.Configure(grenadeSprite, explosionClip, gameObject, Random.Range(2.4f, 3.1f));
            rb.AddForce(
                (direction + Random.insideUnitCircle * 0.08f).normalized * force,
                ForceMode2D.Impulse);

            rb.AddTorque(Random.Range(-220f, 220f));

            WorldCalloutSystem.Instance?.Show(transform, "ГРАНАТА!", 0.9f);
        }
    }
}
