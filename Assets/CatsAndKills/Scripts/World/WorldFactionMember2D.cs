using UnityEngine;

namespace CatsAndKills.World
{
    public enum WorldFaction
    {
        Civilian,
        Security,
        Administration,
        Gang,
        Resistance,
        Independent
    }

    public sealed class WorldFactionMember2D :
        MonoBehaviour
    {
        [SerializeField] private WorldFaction faction =
            WorldFaction.Independent;

        [SerializeField] private bool hostileToPlayer;

        public WorldFaction Faction =>
            faction;

        public bool IsHostileToPlayer =>
            hostileToPlayer;

        public event System.Action BecameHostile;

        public void Configure(
            WorldFaction newFaction,
            bool hostile)
        {
            faction = newFaction;
            hostileToPlayer = hostile;
        }

        public void BecomeHostile()
        {
            if (hostileToPlayer)
                return;

            hostileToPlayer = true;
            BecameHostile?.Invoke();
        }

        public void SetHostile(
            bool hostile)
        {
            if (hostile)
            {
                BecomeHostile();
                return;
            }

            hostileToPlayer = false;
        }
    }
}
