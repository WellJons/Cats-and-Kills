using System.Collections.Generic;
using CatsAndKills.FX;
using UnityEngine;

namespace CatsAndKills.Damage
{
    public sealed class ModularCharacter2D : MonoBehaviour
    {
        [System.Serializable]
        public class LimbBinding
        {
            public BodyPart part;
            public Transform visual;
            public Collider2D hitbox;
        }

        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private List<LimbBinding> limbs = new List<LimbBinding>();
        [SerializeField] private SpriteRenderer[] tintTargets;

        private readonly HashSet<BodyPart> _detached = new HashSet<BodyPart>();

        public void Configure(CharacterVitals v, List<LimbBinding> bindings, SpriteRenderer[] renderers)
        {
            vitals = v;
            limbs = bindings;
            tintTargets = renderers;
        }

        public void Tint(Color color)
        {
            if (tintTargets == null) return;
            foreach (var sr in tintTargets)
                if (sr != null) sr.color *= color;
        }

        private void OnEnable()
        {
            if (vitals == null) vitals = GetComponentInParent<CharacterVitals>();
            if (vitals != null)
            {
                vitals.Dismembered += OnDismembered;
                vitals.LimbDisabled += OnLimbDisabled;
                vitals.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (vitals != null)
            {
                vitals.Dismembered -= OnDismembered;
                vitals.LimbDisabled -= OnLimbDisabled;
                vitals.Died -= OnDied;
            }
        }

        private void OnLimbDisabled(BodyPart part, DamageInfo info)
        {
            if (vitals == null) return;

            if (vitals.LeftLegDisabled && vitals.RightLegDisabled)
            {
                transform.localScale = new Vector3(1.08f, 0.78f, 1f);

                LimbBinding left = limbs.Find(x => x.part == BodyPart.LeftArm);
                LimbBinding right = limbs.Find(x => x.part == BodyPart.RightArm);

                if (left != null && left.visual != null)
                    left.visual.localPosition += Vector3.right * 0.16f;

                if (right != null && right.visual != null)
                    right.visual.localPosition += Vector3.right * 0.16f;
            }
        }

        private void OnDismembered(BodyPart part, DamageInfo info)
        {
            if (_detached.Contains(part)) return;

            LimbBinding binding = limbs.Find(x => x.part == part);
            if (binding == null || binding.visual == null) return;

            _detached.Add(part);

            var originalRenderer = binding.visual.GetComponent<SpriteRenderer>();
            GameObject detached = new GameObject("Detached " + part);
            detached.transform.position = binding.visual.position;
            detached.transform.rotation = binding.visual.rotation;
            detached.transform.localScale = binding.visual.lossyScale;

            var sr = detached.AddComponent<SpriteRenderer>();
            if (originalRenderer != null)
            {
                sr.sprite = originalRenderer.sprite;
                sr.color = originalRenderer.color;
                sr.sortingOrder = originalRenderer.sortingOrder + 1;
            }

            var rb = detached.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.6f;
            rb.angularDamping = 1.4f;
            rb.AddForce(info.Direction.normalized * Mathf.Max(2f, info.Force) + Random.insideUnitCircle * 2f, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-180f, 180f));

            var col = detached.AddComponent<CircleCollider2D>();
            col.radius = 0.18f;

            detached.AddComponent<DebrisLifetime2D>().SetLifetime(7f);

            binding.visual.gameObject.SetActive(false);
            if (binding.hitbox != null) binding.hitbox.enabled = false;

            FXService.Instance?.BloodBurst(binding.visual.position, info.Direction, 12, 1.3f);
        }

        private void OnDied()
        {
            FXService.Instance?.BloodBurst(transform.position, Random.insideUnitCircle.normalized, 8, 0.8f);
        }
    }
}
