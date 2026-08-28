using CatsAndKills.Combat;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class GrenadeAwareness2D : MonoBehaviour
    {
        [SerializeField] private float dangerRadius = 4.8f;
        [SerializeField] private float reactionFuse = 2.1f;
        [SerializeField] private float evadeDistance = 5.2f;
        [SerializeField] private float returnDistance = 1.25f;

        private float _nextScan;
        private float _nextReturnAttempt;

        public bool TryGetEvadePoint(out Vector2 evadePoint)
        {
            evadePoint = transform.position;

            if (Time.time < _nextScan)
                return false;

            _nextScan = Time.time + 0.12f;

            Grenade2D[] grenades = FindObjectsByType<Grenade2D>(FindObjectsSortMode.None);
            Grenade2D threat = null;
            float best = float.MaxValue;

            foreach (Grenade2D grenade in grenades)
            {
                if (grenade == null) continue;

                float distance = Vector2.Distance(transform.position, grenade.transform.position);
                if (distance > dangerRadius) continue;
                if (grenade.RemainingFuse > reactionFuse) continue;

                float score = distance + grenade.RemainingFuse * 0.75f;
                if (score < best)
                {
                    best = score;
                    threat = grenade;
                }
            }

            if (threat == null)
                return false;

            float threatDistance = Vector2.Distance(
                transform.position,
                threat.transform.position);

            EnemyMorale2D morale = GetComponent<EnemyMorale2D>();
            bool steadyEnough = morale == null || morale.Morale >= 0.42f;

            if (steadyEnough &&
                threatDistance <= returnDistance &&
                threat.RemainingFuse >= 0.85f &&
                Time.time >= _nextReturnAttempt)
            {
                _nextReturnAttempt = Time.time + 1.2f;

                if (Random.value < 0.48f)
                {
                    Vector2 direction;

                    if (threat.Owner != null)
                        direction = ((Vector2)threat.Owner.transform.position - (Vector2)transform.position).normalized;
                    else
                        direction = ((Vector2)threat.transform.position - (Vector2)transform.position).normalized;

                    threat.Kick(direction, 8.8f, gameObject);
                    WorldCalloutSystem.Instance?.Show(transform, "НАЗАД!", 0.7f);
                    return false;
                }
            }

            Vector2 away = ((Vector2)transform.position - (Vector2)threat.transform.position).normalized;
            if (away.sqrMagnitude < 0.01f)
                away = Random.insideUnitCircle.normalized;

            Vector2 side = new Vector2(-away.y, away.x) * Random.Range(-1f, 1f);
            evadePoint = (Vector2)transform.position + (away + side * 0.35f).normalized * evadeDistance;
            return true;
        }
    }
}
