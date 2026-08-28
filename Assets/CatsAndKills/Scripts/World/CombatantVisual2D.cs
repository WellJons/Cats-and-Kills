using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class CombatantVisual2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer rendererRef;

        public void Configure(SpriteRenderer sr)
        {
            rendererRef = sr;
        }

        public void SetColor(Color color)
        {
            if (rendererRef != null)
                rendererRef.color = color;
        }
    }
}
