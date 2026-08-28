using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class CoverPoint : MonoBehaviour
    {
        [SerializeField] private float quality = 1f;
        public float Quality => quality;
        public EnemyBrain Occupant { get; private set; }
        public bool IsOccupied => Occupant != null;

        public bool TryReserve(EnemyBrain brain)
        {
            if (brain == null) return false;
            if (Occupant != null && Occupant != brain) return false;
            Occupant = brain;
            return true;
        }

        public void Release(EnemyBrain brain)
        {
            if (Occupant == brain)
                Occupant = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsOccupied ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.18f);
        }
    }
}
