using System.Collections;
using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyDeathPresentation2D : MonoBehaviour
    {
        [SerializeField] private CharacterVitals vitals;
        [SerializeField] private float collisionDisableDelay = 0.18f;
        [SerializeField] private float corpseDarken = 0.42f;

        private SpriteRenderer[] _renderers;
        private Collider2D[] _colliders;
        private Rigidbody2D _body;

        private void Awake()
        {
            if (vitals == null) vitals = GetComponent<CharacterVitals>();
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
            _colliders = GetComponentsInChildren<Collider2D>(true);
            _body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (vitals != null)
                vitals.Died += OnDied;
        }

        private void OnDisable()
        {
            if (vitals != null)
                vitals.Died -= OnDied;
        }

        private void OnDied()
        {
            EnemyWeapon2D weapon = GetComponent<EnemyWeapon2D>();
            if (weapon != null) weapon.enabled = false;

            EnemyGrenadeThrower grenades = GetComponent<EnemyGrenadeThrower>();
            if (grenades != null) grenades.enabled = false;

            EnemyMotor2D motor = GetComponent<EnemyMotor2D>();
            if (motor != null) motor.Stop();

            foreach (SpriteRenderer sr in _renderers)
            {
                if (sr == null) continue;
                Color c = sr.color;
                c.r *= corpseDarken;
                c.g *= corpseDarken;
                c.b *= corpseDarken;
                sr.color = c;
                sr.sortingOrder = Mathf.Min(sr.sortingOrder, 6);
            }

            if (_body != null)
            {
                _body.linearVelocity *= 0.35f;
                _body.angularVelocity *= 0.35f;
            }

            StartCoroutine(DisableBlockingCollision());
        }

        private IEnumerator DisableBlockingCollision()
        {
            yield return new WaitForSeconds(collisionDisableDelay);

            foreach (Collider2D col in _colliders)
            {
                if (col == null) continue;
                col.enabled = false;
            }

            if (_body != null)
                _body.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
