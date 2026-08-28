using System.Collections.Generic;
using CatsAndKills.Core;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class FacilityAlarmDirector : MonoBehaviour
    {
        public static FacilityAlarmDirector Instance { get; private set; }

        [SerializeField] private float relayDelay = 1.8f;

        private readonly List<SquadController> _squads =
            new List<SquadController>();

        private SquadController _source;
        private Vector2 _contactPosition;
        private float _relayAt;
        private bool _pending;
        private bool _alarmRaised;

        public bool AlarmRaised => _alarmRaised;

        private void Awake()
        {
            Instance = this;
        }

        public void Register(SquadController squad)
        {
            if (squad != null && !_squads.Contains(squad))
                _squads.Add(squad);
        }

        public void Unregister(SquadController squad)
        {
            _squads.Remove(squad);
        }

        public void ReportContact(
            SquadController source,
            Vector2 position)
        {
            if (_alarmRaised || _pending)
                return;

            _source = source;
            _contactPosition = position;
            _relayAt = Time.time + relayDelay;
            _pending = true;

            CombatDirector.Instance?.ReportAlert();
        }

        private void Update()
        {
            if (!_pending || Time.time < _relayAt)
                return;

            _pending = false;
            _alarmRaised = true;

            foreach (SquadController squad in _squads)
            {
                if (squad == null || squad == _source)
                    continue;

                squad.ReceiveFacilityAlert(_contactPosition);
            }

            RadioDialogueSystem.Instance?.ShowTransient(
                "ТРЕВОГА // КОНТАКТ ПЕРЕДАН ПО ОБЪЕКТУ",
                1.5f);
        }
    }
}
