using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class ThreeQuarterOccluder2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private float fadedAlpha = 0.24f;
        [SerializeField] private float fadeSpeed = 7f;
        [SerializeField] private float screenDepthThreshold = 0.25f;

        private Transform _player;
        private float _alpha = 1f;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Start()
        {
            PlayerMotor2D player =
                FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
                _player = player.transform;
        }

        private void Update()
        {
            if (_player == null)
            {
                PlayerMotor2D player =
                    FindAnyObjectByType<PlayerMotor2D>();

                if (player != null)
                    _player = player.transform;
            }

            if (_player == null || renderers == null)
                return;

            bool playerBehind =
                _player.position.y >
                transform.position.y + screenDepthThreshold;

            float target =
                playerBehind ? fadedAlpha : 1f;

            _alpha = Mathf.MoveTowards(
                _alpha,
                target,
                fadeSpeed * Time.deltaTime);

            foreach (SpriteRenderer sr in renderers)
            {
                if (sr == null) continue;

                Color c = sr.color;
                c.a = _alpha;
                sr.color = c;
            }
        }
    }
}
