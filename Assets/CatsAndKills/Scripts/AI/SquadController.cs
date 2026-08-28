using System.Collections.Generic;
using CatsAndKills.Core;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class SquadController : MonoBehaviour
    {
        [SerializeField] private float memoryDuration = 8f;

        private readonly List<EnemyBrain> _members = new List<EnemyBrain>();
        private float _lastCallout;

        public bool HasContact { get; private set; }
        public Vector2 LastKnownPlayerPosition { get; private set; }
        public float LastReportTime { get; private set; }

        public void Register(EnemyBrain brain)
        {
            if (brain != null && !_members.Contains(brain))
            {
                _members.Add(brain);
                AssignRoles();
            }
        }

        public void Unregister(EnemyBrain brain)
        {
            _members.Remove(brain);
            AssignRoles();
        }

        private void AssignRoles()
        {
            int active = 0;

            foreach (var member in _members)
            {
                if (member == null) continue;

                SquadRole role;
                if (member.Archetype == EnemyArchetype.MachineGunner)
                    role = SquadRole.Suppress;
                else
                {
                    int pattern = active % 4;
                    role = pattern == 0
                        ? SquadRole.Assault
                        : pattern == 1
                            ? SquadRole.Hold
                            : pattern == 2
                                ? SquadRole.Flank
                                : SquadRole.Suppress;
                }

                member.AssignRole(role);
                active++;
            }
        }

        public void ReportPlayer(EnemyBrain reporter, Vector2 position)
        {
            bool firstContact = !HasContact;

            HasContact = true;
            LastKnownPlayerPosition = position;
            LastReportTime = Time.time;

            CombatDirector.Instance?.ReportCombat();

            foreach (var member in _members)
            {
                if (member != null && member != reporter)
                    member.ReceiveSquadContact(position);
            }

            if (firstContact && Time.time - _lastCallout > 1.2f)
            {
                _lastCallout = Time.time;
                WorldCalloutSystem.Instance?.Show(
                    reporter.transform,
                    "КОНТАКТ! ПЕРЕДАЮ ПОЗИЦИЮ!",
                    1.25f);
            }
        }

        public void ReportNoise(EnemyBrain reporter, Vector2 position)
        {
            if (HasContact) return;

            LastKnownPlayerPosition = position;
            LastReportTime = Time.time;
            CombatDirector.Instance?.ReportAlert();

            foreach (var member in _members)
            {
                if (member != null)
                    member.ReceiveNoiseContact(position);
            }
        }

        public int GetMemberIndex(EnemyBrain brain)
        {
            return _members.IndexOf(brain);
        }

        public int ActiveMemberCount
        {
            get
            {
                int count = 0;
                foreach (var member in _members)
                    if (member != null && member.isActiveAndEnabled) count++;
                return count;
            }
        }

        private void Update()
        {
            if (HasContact && Time.time - LastReportTime > memoryDuration)
                HasContact = false;
        }
    }
}
