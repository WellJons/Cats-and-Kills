using System.Collections.Generic;
using CatsAndKills.AI;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.Visual
{
    public sealed class TacticalGridOverlay2D : MonoBehaviour
    {
        [SerializeField] private NavigationGrid2D navigation;
        [SerializeField] private Transform player;
        [SerializeField] private TacticalCombatDirector tactical;

        private readonly List<SpriteRenderer> _cells =
            new List<SpriteRenderer>();

        private Sprite _square;
        private int _lastAP = -1;
        private TacticalPhase _lastPhase =
            TacticalPhase.Exploration;

        public void Configure(
            NavigationGrid2D nav,
            Transform playerTransform,
            TacticalCombatDirector director)
        {
            navigation = nav;
            player = playerTransform;
            tactical = director;
        }

        private void Awake()
        {
            CreateSquareSprite();
        }

        private void OnDestroy()
        {
            if (_square != null &&
                _square.texture != null)
            {
                Destroy(_square.texture);
            }
        }

        private void Update()
        {
            if (tactical == null)
                tactical = TacticalCombatDirector.Instance;

            if (tactical == null ||
                navigation == null ||
                player == null)
            {
                HideAll();
                return;
            }

            if (_lastPhase != tactical.Phase ||
                _lastAP != tactical.PlayerAP)
            {
                _lastPhase = tactical.Phase;
                _lastAP = tactical.PlayerAP;
                Refresh();
            }
        }

        private void Refresh()
        {
            HideAll();

            if (!tactical.IsPlayerTurn ||
                tactical.PlayerAP <= 0)
            {
                return;
            }

            List<Vector2> reachable =
                navigation.GetReachableCells(
                    player.position,
                    tactical.PlayerAP);

            EnsurePool(
                reachable.Count);

            float size =
                navigation.CellSize *
                0.82f;

            for (int i = 0;
                 i < reachable.Count;
                 i++)
            {
                SpriteRenderer sr =
                    _cells[i];

                sr.gameObject.SetActive(true);
                sr.transform.position =
                    reachable[i];

                sr.transform.localScale =
                    new Vector3(
                        size,
                        size,
                        1f);

                sr.color =
                    new Color(
                        0.18f,
                        0.70f,
                        0.92f,
                        0.13f);

                sr.sortingOrder =
                    -30;
            }
        }

        private void EnsurePool(
            int count)
        {
            while (_cells.Count < count)
            {
                GameObject go =
                    new GameObject(
                        "Reachable Cell");

                go.transform.SetParent(
                    transform,
                    false);

                SpriteRenderer sr =
                    go.AddComponent<SpriteRenderer>();

                sr.sprite =
                    _square;

                _cells.Add(sr);
            }
        }

        private void HideAll()
        {
            for (int i = 0;
                 i < _cells.Count;
                 i++)
            {
                if (_cells[i] != null)
                    _cells[i].gameObject.SetActive(false);
            }
        }

        private void CreateSquareSprite()
        {
            Texture2D texture =
                new Texture2D(
                    1,
                    1,
                    TextureFormat.RGBA32,
                    false);

            texture.name =
                "Tactical Grid Cell";

            texture.SetPixel(
                0,
                0,
                Color.white);

            texture.Apply();

            _square =
                Sprite.Create(
                    texture,
                    new Rect(
                        0f,
                        0f,
                        1f,
                        1f),
                    new Vector2(
                        0.5f,
                        0.5f),
                    1f);

            _square.name =
                "Tactical Grid Cell";
        }
    }
}
