using System.Collections;
using System.Collections.Generic;
using CatsAndKills.AI;
using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.Tactical
{
    public enum TacticalPhase
    {
        Exploration,
        PlayerTurn,
        EnemyTurn
    }

    [DefaultExecutionOrder(-500)]
    public sealed class TacticalCombatDirector : MonoBehaviour
    {
        public static TacticalCombatDirector Instance { get; private set; }

        [SerializeField] private PlayerMotor2D player;
        [SerializeField] private NavigationGrid2D navigation;
        [SerializeField] private int maxPlayerAP = 8;
        [SerializeField] private int enemyAP = 6;
        [SerializeField] private float encounterRadius = 15f;

        private readonly List<TacticalEnemyAgent> _enemies =
            new List<TacticalEnemyAgent>();

        private readonly List<TacticalEnemyAgent> _participants =
            new List<TacticalEnemyAgent>();

        private bool _processingEnemyTurn;

        public TacticalPhase Phase { get; private set; } =
            TacticalPhase.Exploration;

        public bool IsTacticalCombat =>
            Phase != TacticalPhase.Exploration;

        public bool IsPlayerTurn =>
            Phase == TacticalPhase.PlayerTurn;

        public bool IsEnemyTurn =>
            Phase == TacticalPhase.EnemyTurn;

        public int PlayerAP { get; private set; }
        public int MaxPlayerAP => maxPlayerAP;
        public int RoundIndex { get; private set; }
        public IReadOnlyList<TacticalEnemyAgent> Participants => _participants;

        public event System.Action PhaseChanged;
        public event System.Action ActionPointsChanged;

        public void Configure(
            PlayerMotor2D playerMotor,
            NavigationGrid2D nav)
        {
            player = playerMotor;
            navigation = nav;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RegisterEnemy(
            TacticalEnemyAgent enemy)
        {
            if (enemy != null &&
                !_enemies.Contains(enemy))
            {
                _enemies.Add(enemy);

                if (IsTacticalCombat)
                {
                    enemy.SetRealtimeSuspended(true);

                    if (player != null &&
                        Vector2.Distance(
                            player.transform.position,
                            enemy.transform.position) <=
                        encounterRadius)
                    {
                        AddParticipant(enemy);
                    }
                }
            }
        }

        public void UnregisterEnemy(
            TacticalEnemyAgent enemy)
        {
            _enemies.Remove(enemy);
            _participants.Remove(enemy);
        }

        public void EnterCombat(
            TacticalEnemyAgent trigger = null)
        {
            if (player == null)
                player = FindAnyObjectByType<PlayerMotor2D>();

            if (navigation == null)
                navigation = FindAnyObjectByType<NavigationGrid2D>();

            if (player == null)
                return;

            if (IsTacticalCombat)
            {
                AddNearbyParticipants(trigger);
                return;
            }

            _participants.Clear();
            AddNearbyParticipants(trigger);

            if (_participants.Count == 0 &&
                trigger != null)
            {
                AddParticipant(trigger);
            }

            if (_participants.Count == 0)
                return;

            Phase = TacticalPhase.PlayerTurn;
            RoundIndex = 1;
            PlayerAP = maxPlayerAP;

            StopAllRealtimeEnemyBehaviour();

            PhaseChanged?.Invoke();
            ActionPointsChanged?.Invoke();
        }

        private void AddNearbyParticipants(
            TacticalEnemyAgent trigger)
        {
            for (int i = _enemies.Count - 1;
                 i >= 0;
                 i--)
            {
                TacticalEnemyAgent enemy =
                    _enemies[i];

                if (enemy == null)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                if (!enemy.IsAlive)
                    continue;

                float distance =
                    Vector2.Distance(
                        player.transform.position,
                        enemy.transform.position);

                if (enemy == trigger ||
                    enemy.IsAlerted ||
                    distance <= encounterRadius)
                {
                    AddParticipant(enemy);
                }
            }
        }

        private void AddParticipant(
            TacticalEnemyAgent enemy)
        {
            if (enemy == null ||
                !enemy.IsAlive ||
                _participants.Contains(enemy))
            {
                return;
            }

            _participants.Add(enemy);
            enemy.SetTacticalParticipation(true);
        }

        private void StopAllRealtimeEnemyBehaviour()
        {
            foreach (TacticalEnemyAgent enemy in _enemies)
            {
                enemy?.SetRealtimeSuspended(true);
            }

            foreach (TacticalEnemyAgent enemy in _participants)
            {
                enemy?.SetTacticalParticipation(true);
            }
        }

        public bool TrySpendAP(
            int amount)
        {
            if (!IsPlayerTurn ||
                amount <= 0 ||
                PlayerAP < amount)
            {
                return false;
            }

            PlayerAP -= amount;
            ActionPointsChanged?.Invoke();

            return true;
        }

        public void RefundAP(
            int amount)
        {
            if (!IsPlayerTurn ||
                amount <= 0)
            {
                return;
            }

            PlayerAP =
                Mathf.Clamp(
                    PlayerAP + amount,
                    0,
                    maxPlayerAP + 8);

            ActionPointsChanged?.Invoke();
        }

        public void GrantActionPoints(
            int amount)
        {
            if (!IsPlayerTurn ||
                amount <= 0)
            {
                return;
            }

            PlayerAP =
                Mathf.Clamp(
                    PlayerAP + amount,
                    0,
                    maxPlayerAP + 8);

            ActionPointsChanged?.Invoke();
        }

        public void EndPlayerTurn()
        {
            if (!IsPlayerTurn ||
                _processingEnemyTurn)
            {
                return;
            }

            StartCoroutine(
                RunEnemyTurn());
        }

        private IEnumerator RunEnemyTurn()
        {
            _processingEnemyTurn = true;
            Phase = TacticalPhase.EnemyTurn;
            PhaseChanged?.Invoke();

            AddNearbyParticipants(null);

            for (int i = 0;
                 i < _participants.Count;
                 i++)
            {
                TacticalEnemyAgent enemy =
                    _participants[i];

                if (enemy == null ||
                    !enemy.IsAlive)
                {
                    continue;
                }

                yield return enemy.TakeTurn(
                    enemyAP,
                    navigation);
            }

            _participants.RemoveAll(
                enemy =>
                    enemy == null ||
                    !enemy.IsAlive);

            if (_participants.Count == 0)
            {
                ExitCombat();
                yield break;
            }

            RoundIndex++;
            PlayerAP = maxPlayerAP;
            Phase = TacticalPhase.PlayerTurn;
            _processingEnemyTurn = false;

            PhaseChanged?.Invoke();
            ActionPointsChanged?.Invoke();
        }

        public void ExitCombat()
        {
            foreach (TacticalEnemyAgent enemy in _participants)
            {
                enemy?.SetTacticalParticipation(false);
            }

            foreach (TacticalEnemyAgent enemy in _enemies)
            {
                enemy?.SetRealtimeSuspended(false);
            }

            _participants.Clear();

            Phase = TacticalPhase.Exploration;
            PlayerAP = 0;
            _processingEnemyTurn = false;

            PhaseChanged?.Invoke();
            ActionPointsChanged?.Invoke();
        }
    }
}
