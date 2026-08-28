using System.Linq;
using CatsAndKills.AI;
using CatsAndKills.Damage;
using CatsAndKills.Narrative;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class DistrictVerticalSliceDirector :
        MonoBehaviour
    {
        [SerializeField] private MissionDirector mission;

        private NarrativeWorldState _state;
        private string _lastObjective;
        private bool _announcedClear;
        private bool _securityDispatched;

        public void Configure(
            MissionDirector missionDirector)
        {
            mission = missionDirector;
        }

        private void Start()
        {
            if (mission == null)
                mission =
                    FindAnyObjectByType<
                        MissionDirector>();

            _state =
                NarrativeWorldState.Instance;

            SetObjective(
                "ЦЕЛЬ: осмотреть квартал и поговорить с торговцем у проходной");

            RadioDialogueSystem.Instance?.Say(
                "ГОРОДСКАЯ СЕТЬ",
                "Гражданам напоминается: после двадцати двух часов перемещение между секторами допускается только при активном идентификационном ошейнике.",
                4.3f);
        }

        private void Update()
        {
            if (_state == null)
                _state =
                    NarrativeWorldState.Instance;

            if (_state == null)
                return;

            if (_state.HasFlag(
                    "slice_ambush_started"))
            {
                if (AnyLivingGang())
                {
                    SetObjective(
                        "ЦЕЛЬ: пережить засаду в складском переулке");
                    return;
                }

                if (!_state.HasFlag(
                        "slice_ambush_cleared"))
                {
                    _state.SetFlag(
                        "slice_ambush_cleared");

                    RadioDialogueSystem.Instance?.Say(
                        "ГГ",
                        "Переулок чист. Теперь интересно, зачем меня сюда отправили.",
                        3.0f);
                }

                if (!_announcedClear)
                {
                    _announcedClear = true;

                    DispatchSecurityInvestigation();

                    SetObjective(
                        "ЦЕЛЬ: вернуться к механику и потребовать объяснений");
                }

                if (_state.HasFlag(
                        "slice_mechanic_afterfight"))
                {
                    SetObjective(
                        "ЦЕЛЬ: исследовать район дальше — вертикальный срез завершён");

                    if (!_state.HasFlag(
                            "slice_demo_complete"))
                    {
                        _state.SetFlag(
                            "slice_demo_complete");

                        RadioDialogueSystem.Instance?.Say(
                            "СИСТЕМА",
                            "Демонстрационный цикл завершён. Решения из диалогов сохранены в состоянии мира текущей сессии.",
                            4.0f);
                    }
                }

                return;
            }

            if (_state.HasFlag(
                    "slice_mechanic_done"))
            {
                SetObjective(
                    "ЦЕЛЬ: проверить северный складской переулок");
                return;
            }

            if (_state.HasFlag(
                    "slice_vendor_done"))
            {
                SetObjective(
                    "ЦЕЛЬ: найти механика у мастерской");
            }
        }

        private bool AnyLivingGang()
        {
            return
                FindObjectsByType<
                    WorldFactionMember2D>(
                    FindObjectsSortMode.None)
                .Any(
                    member =>
                        member != null &&
                        member.Faction ==
                        WorldFaction.Gang &&
                        member.GetComponent<
                            CharacterVitals>() is
                            CharacterVitals vitals &&
                        !vitals.IsDead);
        }

        private void DispatchSecurityInvestigation()
        {
            if (_securityDispatched)
                return;

            _securityDispatched = true;

            Vector2 incident =
                new Vector2(
                    -22f,
                    8f);

            CityPatrolRoute2D[] patrols =
                FindObjectsByType<
                    CityPatrolRoute2D>(
                    FindObjectsSortMode.None);

            System.Array.Sort(
                patrols,
                (a, b) =>
                {
                    float da =
                        a != null
                            ? Vector2.SqrMagnitude(
                                (Vector2)a.transform.position -
                                incident)
                            : float.PositiveInfinity;

                    float db =
                        b != null
                            ? Vector2.SqrMagnitude(
                                (Vector2)b.transform.position -
                                incident)
                            : float.PositiveInfinity;

                    return da.CompareTo(db);
                });

            int dispatched = 0;

            for (int i = 0;
                 i < patrols.Length &&
                 dispatched < 2;
                 i++)
            {
                CityPatrolRoute2D patrol =
                    patrols[i];

                if (patrol == null)
                    continue;

                WorldFactionMember2D faction =
                    patrol.GetComponent<
                        WorldFactionMember2D>();

                if (faction == null ||
                    faction.Faction !=
                    WorldFaction.Security ||
                    faction.IsHostileToPlayer)
                {
                    continue;
                }

                patrol.Investigate(
                    incident +
                    new Vector2(
                        dispatched *
                        1.4f,
                        0f),
                    22f);

                dispatched++;
            }

            if (dispatched > 0)
            {
                RadioDialogueSystem.Instance?.ShowTransient(
                    "ГОРОДСКАЯ СЕТЬ // ПАТРУЛЬ НАПРАВЛЕН В СКЛАДСКОЙ СЕКТОР",
                    2.2f);
            }
        }

        private void SetObjective(
            string objective)
        {
            if (_lastObjective ==
                objective)
            {
                return;
            }

            _lastObjective =
                objective;

            mission?.SetObjective(
                objective);
        }
    }
}
