using System.Collections;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class MissionDirector : MonoBehaviour
    {
        [SerializeField] private GameObject extractionMarker;
        [SerializeField] private Door2D commandDoor;

        public string CurrentObjective { get; private set; } =
            "ЦЕЛЬ: проникнуть в административный корпус";

        public bool TerminalDone { get; private set; }
        public bool MissionComplete { get; private set; }

        public void Configure(GameObject extraction, Door2D door)
        {
            extractionMarker = extraction;
            commandDoor = door;

            if (extractionMarker != null)
                extractionMarker.SetActive(false);
        }

        private IEnumerator Start()
        {
            if (CheckpointSystem.HasCheckpoint)
            {
                if (!string.IsNullOrEmpty(CheckpointSystem.Label) &&
                    CheckpointSystem.Label.Contains("ADMIN"))
                {
                    CurrentObjective = "ЦЕЛЬ: получить данные из терминала";
                }
                else
                {
                    CurrentObjective = "ЦЕЛЬ: пройти через склад и найти путь в штаб";
                }

                yield break;
            }

            while (Time.timeScale <= 0f)
                yield return null;

            RadioDialogueSystem.Instance?.Say(
                "ОПЕРАТОР",
                "Объект ещё работает. Войди через складской сектор и найди архивный терминал. Без лишнего шума, если получится.",
                4.6f);

            Invoke(nameof(SecondLine), 5.2f);
        }

        private void SecondLine()
        {
            RadioDialogueSystem.Instance?.Say(
                "ГГ",
                "После того, что вы мне оставили на шее, слово «если» звучит особенно убедительно.",
                3.7f);
        }

        public void EnterWarehouse()
        {
            CurrentObjective = "ЦЕЛЬ: пройти через склад и найти путь в штаб";
            RadioDialogueSystem.Instance?.Say(
                "ОПЕРАТОР",
                "Внутри две группы. Они связаны общей сетью. Засветишься у одной — вторая узнает.",
                3.5f);
        }

        public void EnterAdministration()
        {
            CurrentObjective = "ЦЕЛЬ: получить данные из терминала";
            commandDoor?.SetLocked(false);
            RadioDialogueSystem.Instance?.Say(
                "ГГ",
                "Вижу административный блок. Ошейник снова фонит.",
                2.5f);
        }

        public void TerminalCompleted()
        {
            TerminalDone = true;
            CurrentObjective = "ЦЕЛЬ: покинуть объект через южный двор";
            if (extractionMarker != null) extractionMarker.SetActive(true);
        }

        public void CompleteMission()
        {
            if (!TerminalDone || MissionComplete) return;
            MissionComplete = true;
            CurrentObjective = "ОПЕРАЦИЯ ЗАВЕРШЕНА";

            RadioDialogueSystem.Instance?.Say(
                "ОПЕРАТОР",
                "Есть данные. Не задерживайся.",
                2.1f);

            RadioDialogueSystem.Instance?.Say(
                "ГГ",
                "Поздно. Они уже знают, что я здесь.",
                2.2f);
        }
    }
}
