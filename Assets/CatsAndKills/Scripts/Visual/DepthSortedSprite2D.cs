using UnityEngine;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class DepthSortedSprite2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private int baseOrder = 1000;
        [SerializeField] private int unitsPerWorldUnit = 32;
        [SerializeField] private float visualHeightOffset;

        public void Configure(
            SpriteRenderer[] spriteRenderers,
            int order = 1000,
            float heightOffset = 0f)
        {
            renderers = spriteRenderers;
            baseOrder = order;
            visualHeightOffset = heightOffset;
            Refresh();
        }

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (renderers == null) return;

            int anchorOrder =
                baseOrder -
                Mathf.RoundToInt(
                    (transform.position.y - visualHeightOffset) *
                    unitsPerWorldUnit);

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr == null) continue;

                // Preserve local layering between body / weapon / foreground pieces.
                int local = Mathf.Clamp(sr.sortingOrder, -24, 24);
                sr.sortingOrder = anchorOrder + local;
            }
        }
    }
}
