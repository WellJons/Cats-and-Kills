using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class ReinforcementDirector : MonoBehaviour
    {
        [SerializeField] private GameObject[] units;
        [SerializeField] private float deploymentDelay = 1.4f;

        private MissionDirector _mission;
        private bool _scheduled;
        private bool _deployed;
        private float _deployAt;

        public void Configure(GameObject[] reinforcementUnits)
        {
            units = reinforcementUnits;

            if (units == null) return;
            foreach (GameObject unit in units)
            {
                if (unit != null)
                    unit.SetActive(false);
            }
        }

        private void Update()
        {
            if (_deployed) return;

            if (_mission == null)
                _mission = FindFirstObjectByType<MissionDirector>();

            if (_mission == null || !_mission.TerminalDone)
                return;

            if (!_scheduled)
            {
                _scheduled = true;
                _deployAt = Time.time + deploymentDelay;

                RadioDialogueSystem.Instance?.Say(
                    "ПЕРЕХВАТ",
                    "Группа быстрого реагирования, вход через южные ворота. Цель внутри комплекса.",
                    2.8f);

                return;
            }

            if (Time.time < _deployAt)
                return;

            Deploy();
        }

        private void Deploy()
        {
            _deployed = true;

            if (units != null)
            {
                foreach (GameObject unit in units)
                {
                    if (unit != null)
                        unit.SetActive(true);
                }
            }

            RadioDialogueSystem.Instance?.ShowTransient(
                "ПОДКРЕПЛЕНИЕ ПРИБЫЛО",
                1.4f);
        }
    }
}
