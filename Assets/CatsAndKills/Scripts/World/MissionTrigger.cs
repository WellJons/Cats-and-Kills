using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.World
{
    public enum MissionTriggerType
    {
        Warehouse,
        Administration,
        Extraction
    }

    public sealed class MissionTrigger : MonoBehaviour
    {
        [SerializeField] private MissionDirector mission;
        [SerializeField] private MissionTriggerType triggerType;
        private bool _used;

        public void Configure(MissionDirector director, MissionTriggerType type)
        {
            mission = director;
            triggerType = type;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_used || mission == null) return;

            bool isPlayer =
                other.CompareTag("Player") ||
                other.GetComponentInParent<PlayerMotor2D>() != null;

            if (!isPlayer) return;

            if (triggerType == MissionTriggerType.Extraction && !mission.TerminalDone)
                return;

            _used = true;

            switch (triggerType)
            {
                case MissionTriggerType.Warehouse:
                    mission.EnterWarehouse();
                    break;

                case MissionTriggerType.Administration:
                    mission.EnterAdministration();
                    break;

                case MissionTriggerType.Extraction:
                    mission.CompleteMission();
                    break;
            }
        }
    }
}
