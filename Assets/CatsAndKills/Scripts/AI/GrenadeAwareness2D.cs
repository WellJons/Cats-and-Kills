using CatsAndKills.Combat;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class GrenadeAwareness2D : MonoBehaviour
    {
        [SerializeField] private float dangerRadius = 4.8f;
        [SerializeField] private float reactionFuse = 2.1f;
        [SerializeField] private float evadeDistance = 5.2f;

        private float _nextScan;

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

            Vector2 away = ((Vector2)transform.position - (Vector2)threat.transform.position).normalized;
            if (away.sqrMagnitude < 0.01f)
                away = Random.insideUnitCircle.normalized;

            Vector2 side = new Vector2(-away.y, away.x) * Random.Range(-1f, 1f);
            evadePoint = (Vector2)transform.position + (away + side * 0.35f).normalized * evadeDistance;
            return true;
        }
    }
}
