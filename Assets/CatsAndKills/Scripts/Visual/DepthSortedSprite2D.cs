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

        private int[] _localOrders;

        public void Configure(
            SpriteRenderer[] spriteRenderers,
            int order = 1000,
            float heightOffset = 0f)
        {
            renderers = spriteRenderers;
            baseOrder = order;
            visualHeightOffset = heightOffset;
            CacheLocalOrders();
            Refresh();
        }

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>(true);

            CacheLocalOrders();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void CacheLocalOrders()
        {
            if (renderers == null)
            {
                _localOrders = null;
                return;
            }

            _localOrders = new int[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                _localOrders[i] = sr != null
                    ? Mathf.Clamp(sr.sortingOrder, -24, 24)
                    : 0;
            }
        }

        private void Refresh()
        {
            if (renderers == null)
                return;

            if (_localOrders == null ||
                _localOrders.Length != renderers.Length)
            {
                CacheLocalOrders();
            }

            int anchorOrder =
                baseOrder -
                Mathf.RoundToInt(
                    (transform.position.y - visualHeightOffset) *
                    unitsPerWorldUnit);

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr == null)
                    continue;

                sr.sortingOrder =
                    anchorOrder +
                    _localOrders[i];
            }
        }
    }
}
