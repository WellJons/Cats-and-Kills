using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.AI
{
    public sealed class EnemyMorale2D : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float morale = 0.72f;
        [SerializeField] private float recoveryPerSecond = 0.035f;
        [SerializeField] private float woundPenalty = 0.16f;
        [SerializeField] private float suppressionPenalty = 0.22f;

        private CharacterVitals _vitals;
        private SuppressionReceiver2D _suppression;
        private float _lastHealth;

        public float Morale => morale;
        public bool Shaken => morale < 0.42f;
        public bool Broken => morale < 0.19f;

        public void Configure(float startingMorale)
        {
            morale = Mathf.Clamp01(startingMorale);
        }

        private void Awake()
        {
            _vitals = GetComponent<CharacterVitals>();
            _suppression = GetComponent<SuppressionReceiver2D>();

            if (_vitals != null)
                _lastHealth = _vitals.Health;
        }

        private void Update()
        {
            if (_vitals == null || _vitals.IsDead) return;

            if (_vitals.Health < _lastHealth)
            {
                float fraction = (_lastHealth - _vitals.Health) / Mathf.Max(1f, _vitals.MaxHealth);
                morale = Mathf.Clamp01(morale - woundPenalty * Mathf.Clamp01(fraction * 3f));
                _lastHealth = _vitals.Health;
            }

            if (_suppression != null && _suppression.IsUnderFire)
            {
                morale = Mathf.Clamp01(
                    morale - suppressionPenalty * _suppression.Suppression * Time.deltaTime);
            }
            else
            {
                morale = Mathf.MoveTowards(morale, 0.72f, recoveryPerSecond * Time.deltaTime);
            }
        }

        public void WitnessAllyDeath(float distance)
        {
            float impact = Mathf.Lerp(0.18f, 0.04f, Mathf.Clamp01(distance / 10f));
            morale = Mathf.Clamp01(morale - impact);
        }
    }
}
