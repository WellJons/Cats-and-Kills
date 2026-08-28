using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class MissionTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private MissionDirector mission;
        private bool _used;

        public string InteractionPrompt => _used ? "ТЕРМИНАЛ УЖЕ СКОПИРОВАН" : "СКОПИРОВАТЬ ДАННЫЕ [E]";

        public void Configure(MissionDirector director)
        {
            mission = director;
        }

        public void Interact()
        {
            if (_used) return;
            _used = true;
            mission?.TerminalCompleted();
            RadioDialogueSystem.Instance?.Say("СВЯЗЬ", "Копирование завершено. У тебя компания. Уходи через южный двор.", 3.4f);
        }
    }
}
