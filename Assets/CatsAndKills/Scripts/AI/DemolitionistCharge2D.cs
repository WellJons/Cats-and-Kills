using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.UI;
using CatsAndKills.World;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class DemolitionistCharge2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private float triggerDistance = 1.65f;
        [SerializeField] private float fuse = 0.72f;
        [SerializeField] private WorldFactionMember2D factionMember;

        private bool _armed;
        private float _detonateAt;

        public bool Armed => _armed;

        public void Configure(
            Transform targetTransform,
            CharacterVitals characterVitals)
        {
            target = targetTransform;
            vitals = characterVitals;
        }

        private void Awake()
        {
            if (vitals == null)
                vitals = GetComponent<CharacterVitals>();

            if (factionMember == null)
                factionMember = GetComponent<WorldFactionMember2D>();
        }

        private void Update()
        {
            if (vitals != null && vitals.IsDead)
                return;

            if (factionMember == null)
                factionMember = GetComponent<WorldFactionMember2D>();

            if (factionMember != null &&
                !factionMember.IsHostileToPlayer)
            {
                return;
            }

            if (!_armed)
            {
                if (target == null) return;

                float distance =
                    Vector2.Distance(transform.position, target.position);

                bool desperate =
                    vitals != null &&
                    vitals.Health <= vitals.MaxHealth * 0.28f &&
                    distance <= 3.2f;

                if (distance <= triggerDistance || desperate)
                    Arm();

                return;
            }

            if (Time.time >= _detonateAt)
                Detonate();
        }

        private void Arm()
        {
            if (_armed) return;
            _armed = true;
            _detonateAt = Time.time + fuse;

            WorldCalloutSystem.Instance?.Show(
                transform,
                "НЕ ПОДХОДИ!",
                fuse);

            EnemyWeapon2D weapon = GetComponent<EnemyWeapon2D>();
            weapon?.SetTrigger(false);
        }

        private void Detonate()
        {
            if (!_armed) return;
            _armed = false;

            GameObject blast = new GameObject("Demolitionist Blast");
            blast.transform.position = transform.position;

            blast.AddComponent<SpriteRenderer>();
            var rb = blast.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            blast.AddComponent<CircleCollider2D>();

            Grenade2D grenade = blast.AddComponent<Grenade2D>();
            grenade.Configure(
                null,
                null,
                gameObject,
                0.05f);

            enabled = false;
        }
    }
}
