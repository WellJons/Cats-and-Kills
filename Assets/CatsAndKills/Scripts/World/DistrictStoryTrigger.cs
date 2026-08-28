using CatsAndKills.AI;
using CatsAndKills.Narrative;
using CatsAndKills.Tactical;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class DistrictStoryTrigger : MonoBehaviour
    {
        [SerializeField] private string requiredFlag =
            "slice_mechanic_done";

        [SerializeField] private string triggerFlag =
            "slice_ambush_started";

        [SerializeField] private WorldFaction factionToActivate =
            WorldFaction.Gang;

        private bool _triggered;

        public void Configure(
            string required,
            string trigger,
            WorldFaction faction)
        {
            requiredFlag = required;
            triggerFlag = trigger;
            factionToActivate = faction;
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            if (_triggered ||
                other == null ||
                !other.transform.root.CompareTag("Player"))
            {
                return;
            }

            NarrativeWorldState state =
                NarrativeWorldState.Instance;

            if (state == null)
                return;

            if (!string.IsNullOrWhiteSpace(
                    requiredFlag) &&
                !state.HasFlag(
                    requiredFlag))
            {
                RadioDialogueSystem.Instance
                    ?.ShowTransient(
                        "Здесь пока нечего делать.",
                        1.1f);

                return;
            }

            _triggered = true;

            if (!string.IsNullOrWhiteSpace(
                    triggerFlag))
            {
                state.SetFlag(
                    triggerFlag);
            }

            TacticalEnemyAgent first =
                null;

            foreach (WorldFactionMember2D member in
                     FindObjectsByType<WorldFactionMember2D>(
                         FindObjectsSortMode.None))
            {
                if (member == null ||
                    member.Faction !=
                    factionToActivate)
                {
                    continue;
                }

                member.BecomeHostile();

                TacticalEnemyAgent agent =
                    member.GetComponent<
                        TacticalEnemyAgent>();

                if (first == null &&
                    agent != null &&
                    agent.IsAlive)
                {
                    first = agent;
                }
            }

            RadioDialogueSystem.Instance?.Say(
                "НЕИЗВЕСТНЫЙ",
                "Стой. Дальше ты не пойдёшь.",
                2.2f);

            if (first != null)
            {
                TacticalCombatDirector tactical =
                    TacticalCombatDirector.Instance;

                tactical?.EnterCombat(first);

                if (state.HasFlag(
                        "slice_vendor_asked_patrol") &&
                    tactical != null &&
                    tactical.IsPlayerTurn)
                {
                    tactical.GrantActionPoints(2);

                    RadioDialogueSystem.Instance?.Say(
                        "ГГ",
                        "Торговец предупреждал про склад. Я хотя бы не застигнут врасплох.",
                        2.8f);

                    RadioDialogueSystem.Instance?.ShowTransient(
                        "ИНФОРМАЦИЯ ИЗ ДИАЛОГА // +2 AP В ПЕРВОМ РАУНДЕ",
                        1.8f);
                }
            }
        }
    }
}
