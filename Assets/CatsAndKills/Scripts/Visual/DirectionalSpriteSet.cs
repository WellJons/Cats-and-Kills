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
        [SerializeField] private Sprite[] moveAlt = new Sprite[8];
        [SerializeField] private Sprite[] fire = new Sprite[8];
        [SerializeField] private Sprite[] reload = new Sprite[8];
        [SerializeField] private Sprite[] hurt = new Sprite[8];
        [SerializeField] private Sprite[] crawl = new Sprite[8];
        [SerializeField] private Sprite[] dead = new Sprite[8];
        [SerializeField] private bool mirrorNorthEastFromNorthWest;

        [Header("Combat anchors (normalized inside visible sprite bounds)")]
        [SerializeField, Range(0f, 1f)] private float aimHeight01 = 0.56f;
        [SerializeField] private Vector2[] muzzleAnchor01 =
        {
            new Vector2(0.86f, 0.56f), // E
            new Vector2(0.78f, 0.67f), // NE
            new Vector2(0.55f, 0.72f), // N
            new Vector2(0.22f, 0.67f), // NW
            new Vector2(0.14f, 0.56f), // W
            new Vector2(0.22f, 0.46f), // SW
            new Vector2(0.55f, 0.42f), // S
            new Vector2(0.78f, 0.46f)  // SE
        };

        public void Configure(
            Sprite[] idleSet,
            Sprite[] moveSet,
            Sprite[] fireSet,
            Sprite[] reloadSet,
            Sprite[] hurtSet,
            Sprite[] crawlSet,
            Sprite[] deadSet)
        {
            mirrorNorthEastFromNorthWest = false;
            idle = Normalize(idleSet);
            move = Normalize(moveSet);
            fire = Normalize(fireSet);
            reload = Normalize(reloadSet);
            hurt = Normalize(hurtSet);
            crawl = Normalize(crawlSet);
            dead = Normalize(deadSet);
        }

        public void ConfigureExtended(
            Sprite[] idleSet,
            Sprite[] moveSet,
            Sprite[] moveAltSet,
            Sprite[] fireSet,
            Sprite[] reloadSet,
            Sprite[] hurtSet,
            Sprite[] crawlSet,
            Sprite[] deadSet,
            bool mirrorNorthEast = false)
        {
            mirrorNorthEastFromNorthWest = mirrorNorthEast;
            idle = Normalize(idleSet);
            move = Normalize(moveSet);
            moveAlt = Normalize(moveAltSet);
            fire = Normalize(fireSet);
            reload = Normalize(reloadSet);
            hurt = Normalize(hurtSet);
            crawl = Normalize(crawlSet);
            dead = Normalize(deadSet);
        }

        public Sprite GetIdle(CharacterDirection8 direction) =>
            Get(idle, direction);

        public Sprite GetMove(CharacterDirection8 direction) =>
            Get(move, direction, GetIdle(direction));

        public Sprite GetMoveAlt(CharacterDirection8 direction) =>
            Get(moveAlt, direction, GetMove(direction));

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

        public bool ShouldFlipX(CharacterDirection8 direction) =>
            mirrorNorthEastFromNorthWest &&
            direction == CharacterDirection8.NorthEast;

        public float AimHeight01 =>
            Mathf.Clamp01(aimHeight01);

        public Vector2 GetMuzzleAnchor01(
            CharacterDirection8 direction)
        {
            int index = (int)direction;

            if (muzzleAnchor01 != null &&
                index >= 0 &&
                index < muzzleAnchor01.Length)
            {
                Vector2 configured = muzzleAnchor01[index];

                if (configured.sqrMagnitude > 0.0001f)
                {
                    return new Vector2(
                        Mathf.Clamp01(configured.x),
                        Mathf.Clamp01(configured.y));
                }
            }

            return direction switch
            {
                CharacterDirection8.East =>
                    new Vector2(0.86f, 0.56f),
                CharacterDirection8.NorthEast =>
                    new Vector2(0.78f, 0.67f),
                CharacterDirection8.North =>
                    new Vector2(0.55f, 0.72f),
                CharacterDirection8.NorthWest =>
                    new Vector2(0.22f, 0.67f),
                CharacterDirection8.West =>
                    new Vector2(0.14f, 0.56f),
                CharacterDirection8.SouthWest =>
                    new Vector2(0.22f, 0.46f),
                CharacterDirection8.South =>
                    new Vector2(0.55f, 0.42f),
                _ =>
                    new Vector2(0.78f, 0.46f)
            };
        }

        private static Sprite[] Normalize(Sprite[] source)
        {
            var result = new Sprite[8];

            if (source == null)
                return result;

            for (int i = 0; i < result.Length && i < source.Length; i++)
                result[i] = source[i];

            return result;
        }

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
