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

        private int _grenades = 1;
        private float _nextThrow;
        private float _minDistance = 4f;
        private float _maxDistance = 10.5f;

        public void Configure(
            Transform targetTransform,
            Sprite sprite,
            AudioClip explosion,
            int count)
        {
            target = targetTransform;
            grenadeSprite = sprite;
            explosionClip = explosion;
            _grenades = count;
            _nextThrow = Time.time + Random.Range(4f, 8f);
        }

        public bool TryThrow(bool hasVisual, float aggression)
        {
            if (!hasVisual || target == null || _grenades <= 0)
                return false;

            if (Time.time < _nextThrow)
                return false;

            float distance = Vector2.Distance(transform.position, target.position);
            if (distance < _minDistance || distance > _maxDistance)
                return false;

            float chance = Mathf.Lerp(0.18f, 0.46f, aggression);
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
            _grenades--;
            _nextThrow = Time.time + Random.Range(8f, 13f);

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
