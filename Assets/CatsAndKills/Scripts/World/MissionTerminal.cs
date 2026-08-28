using CatsAndKills.Player;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class MissionTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private MissionDirector mission;
        [SerializeField] private float downloadDuration = 4.5f;
        [SerializeField] private float cancelDistance = 1.8f;

        private bool _used;
        private bool _downloading;
        private float _startedAt;
        private Transform _operator;

        public string InteractionPrompt
        {
            get
            {
                if (_used)
                    return "АРХИВ УЖЕ СКОПИРОВАН";

                if (_downloading)
                {
                    float progress =
                        Mathf.Clamp01(
                            (Time.time - _startedAt) /
                            Mathf.Max(0.01f, downloadDuration));

                    return $"СКАЧИВАНИЕ АРХИВА  {progress * 100f:0}%";
                }

                return "НАЧАТЬ КОПИРОВАНИЕ [E]";
            }
        }

        public void Configure(MissionDirector director)
        {
            mission = director;
        }

        public void Interact()
        {
            if (_used || _downloading)
                return;

            PlayerMotor2D player =
                FindFirstObjectByType<PlayerMotor2D>();

            if (player == null)
                return;

            _operator = player.transform;
            _downloading = true;
            _startedAt = Time.time;

            RadioDialogueSystem.Instance?.Say(
                "ТЕРМИНАЛ",
                "Соединение установлено. Не отходите от терминала до завершения копирования.",
                2.4f);
        }

        private void Update()
        {
            if (!_downloading || _used)
                return;

            if (_operator == null ||
                Vector2.Distance(
                    _operator.position,
                    transform.position) > cancelDistance)
            {
                _downloading = false;
                _operator = null;

                RadioDialogueSystem.Instance?.ShowTransient(
                    "КОПИРОВАНИЕ ПРЕРВАНО",
                    1f);

                return;
            }

            if (Time.time - _startedAt < downloadDuration)
                return;

            CompleteDownload();
        }

        private void CompleteDownload()
        {
            _downloading = false;
            _used = true;

            mission?.TerminalCompleted();

            RadioDialogueSystem.Instance?.Say(
                "СВЯЗЬ",
                "Копирование завершено. У тебя компания. Уходи через южный двор.",
                3.4f);
        }
    }
}
