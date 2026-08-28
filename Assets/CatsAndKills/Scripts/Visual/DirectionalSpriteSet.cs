using UnityEngine;

namespace CatsAndKills.Visual
{
    [CreateAssetMenu(
        menuName = "Cats and Kills/Visual/Directional Sprite Set",
        fileName = "DirectionalSpriteSet")]
    public sealed class DirectionalSpriteSet : ScriptableObject
    {
        [Header("One sprite per direction: E, NE, N, NW, W, SW, S, SE")]
        [SerializeField] private Sprite[] idle = new Sprite[8];
        [SerializeField] private Sprite[] move = new Sprite[8];
        [SerializeField] private Sprite[] fire = new Sprite[8];
        [SerializeField] private Sprite[] reload = new Sprite[8];
        [SerializeField] private Sprite[] hurt = new Sprite[8];
        [SerializeField] private Sprite[] crawl = new Sprite[8];
        [SerializeField] private Sprite[] dead = new Sprite[8];

        public Sprite GetIdle(CharacterDirection8 direction) =>
            Get(idle, direction);

        public Sprite GetMove(CharacterDirection8 direction) =>
            Get(move, direction, GetIdle(direction));

        public Sprite GetFire(CharacterDirection8 direction) =>
            Get(fire, direction, GetIdle(direction));

        public Sprite GetReload(CharacterDirection8 direction) =>
            Get(reload, direction, GetIdle(direction));

        public Sprite GetHurt(CharacterDirection8 direction) =>
            Get(hurt, direction, GetIdle(direction));

        public Sprite GetCrawl(CharacterDirection8 direction) =>
            Get(crawl, direction, GetMove(direction));

        public Sprite GetDead(CharacterDirection8 direction) =>
            Get(dead, direction, GetIdle(direction));

        private static Sprite Get(
            Sprite[] sprites,
            CharacterDirection8 direction,
            Sprite fallback = null)
        {
            int index = (int)direction;

            if (sprites == null ||
                index < 0 ||
                index >= sprites.Length ||
                sprites[index] == null)
            {
                return fallback;
            }

            return sprites[index];
        }
    }
}
