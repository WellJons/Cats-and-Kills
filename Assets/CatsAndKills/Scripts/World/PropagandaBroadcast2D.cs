using CatsAndKills.AI;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class PropagandaBroadcast2D : MonoBehaviour
    {
        [SerializeField] private float calmInterval = 38f;
        [SerializeField] private float alertInterval = 28f;

        private float _nextBroadcast = 16f;
        private bool _announcedAlarm;
        private int _calmIndex;
        private int _alertIndex;

        private readonly string[] _calmLines =
        {
            "Гражданам напоминается: комендантский час начинается в двадцать два ноль-ноль. Ошейник должен оставаться активным.",
            "Стабильность начинается с дисциплины. Сообщайте о повреждённых или снятых идентификационных ошейниках.",
            "Восстановление продолжается. Военная администрация благодарит граждан за спокойствие и сотрудничество."
        };

        private readonly string[] _alertLines =
        {
            "Внимание. На территории промышленного сектора действует вооружённый диверсант. Не приближайтесь к зоне операции.",
            "Силы безопасности проводят локальную зачистку. Информация о потерях противника будет опубликована после завершения операции.",
            "Несанкционированное распространение сведений о происходящем в промышленном секторе будет расценено как содействие противнику."
        };

        private void Update()
        {
            FacilityAlarmDirector alarm =
                FacilityAlarmDirector.Instance;

            bool raised = alarm != null && alarm.AlarmRaised;

            if (raised && !_announcedAlarm)
            {
                _announcedAlarm = true;
                _nextBroadcast = Time.time + 2.5f;
            }

            if (Time.time < _nextBroadcast)
                return;

            if (raised)
            {
                Say(
                    _alertLines[
                        _alertIndex++ % _alertLines.Length]);

                _nextBroadcast =
                    Time.time + alertInterval;
            }
            else
            {
                Say(
                    _calmLines[
                        _calmIndex++ % _calmLines.Length]);

                _nextBroadcast =
                    Time.time + calmInterval;
            }
        }

        private static void Say(string text)
        {
            RadioDialogueSystem.Instance?.Say(
                "ГОРОДСКАЯ СЕТЬ",
                text,
                4.2f);
        }
    }
}
