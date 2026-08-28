using CatsAndKills.Narrative;
using CatsAndKills.Player;
using CatsAndKills.Tactical;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    [DisallowMultipleComponent]
    public sealed class CityAmbientChatter2D :
        MonoBehaviour
    {
        [SerializeField] private string[] lines;
        [SerializeField] private float minInterval = 8f;
        [SerializeField] private float maxInterval = 18f;
        [SerializeField] private float hearingDistance = 8f;

        private static float _nextGlobalChatter;

        private float _nextLocal;
        private Transform _player;

        public void Configure(
            string[] ambientLines,
            float minimum = 8f,
            float maximum = 18f)
        {
            lines = ambientLines;
            minInterval = minimum;
            maxInterval = maximum;

            Schedule();
        }

        private void Start()
        {
            PlayerMotor2D player =
                FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
                _player = player.transform;

            Schedule();
        }

        private void Update()
        {
            if (lines == null ||
                lines.Length == 0 ||
                Time.time < _nextLocal ||
                Time.time < _nextGlobalChatter ||
                NarrativeDialogueSystem.IsDialogueOpen)
            {
                return;
            }

            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null &&
                tactical.IsTacticalCombat)
            {
                Schedule();
                return;
            }

            if (_player == null)
            {
                PlayerMotor2D player =
                    FindAnyObjectByType<PlayerMotor2D>();

                if (player != null)
                    _player = player.transform;
            }

            if (_player == null ||
                Vector2.Distance(
                    transform.position,
                    _player.position) >
                hearingDistance)
            {
                Schedule();
                return;
            }

            string line =
                lines[
                    Random.Range(
                        0,
                        lines.Length)];

            WorldCalloutSystem.Instance?.Show(
                transform,
                line,
                1.65f);

            _nextGlobalChatter =
                Time.time +
                Random.Range(
                    2.8f,
                    5.2f);

            Schedule();
        }

        private void Schedule()
        {
            _nextLocal =
                Time.time +
                Random.Range(
                    Mathf.Min(
                        minInterval,
                        maxInterval),
                    Mathf.Max(
                        minInterval,
                        maxInterval));
        }
    }
}
