using System.Collections.Generic;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class CoverManager : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleMask;
        private readonly List<CoverPoint> _points = new List<CoverPoint>();

        public void Configure(LayerMask mask)
        {
            obstacleMask = mask;
        }

        private void Awake()
        {
            Refresh();
        }

        public void Refresh()
        {
            _points.Clear();
            _points.AddRange(
                FindObjectsByType<CoverPoint>(FindObjectsSortMode.None));
        }

        public CoverPoint FindBestCover(
            Vector2 agent,
            Vector2 threat,
            EnemyBrain requester,
            float maxDistance = 14f)
        {
            CoverPoint best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var point in _points)
            {
                if (point == null) continue;
                if (point.IsOccupied && point.Occupant != requester) continue;

                Vector2 cover = point.transform.position;
                float distance = Vector2.Distance(agent, cover);

                if (distance > maxDistance) continue;

                RaycastHit2D block =
                    Physics2D.Linecast(threat, cover, obstacleMask);

                if (block.collider == null)
                    continue;

                float threatDistance = Vector2.Distance(threat, cover);
                float safety = Mathf.Clamp01((threatDistance - 1.5f) / 5f);

                float score =
                    point.Quality * 3.2f -
                    distance * 0.18f +
                    safety * 1.6f +
                    Random.Range(-0.15f, 0.15f);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = point;
                }
            }

            return best;
        }
    }
}
