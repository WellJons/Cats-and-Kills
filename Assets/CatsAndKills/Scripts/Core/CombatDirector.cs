using UnityEngine;

namespace CatsAndKills.Core
{
    public enum CombatIntensity
    {
        Calm,
        Alert,
        Combat
    }

    public sealed class CombatDirector : MonoBehaviour
    {
        public static CombatDirector Instance { get; private set; }

        [SerializeField] private float combatMemory = 5.5f;
        [SerializeField] private float alertMemory = 12f;

        private float _lastCombat;
        private float _lastAlert;

        public CombatIntensity Intensity { get; private set; } = CombatIntensity.Calm;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ReportAlert()
        {
            _lastAlert = Time.unscaledTime;
            if (Intensity == CombatIntensity.Calm)
                SetIntensity(CombatIntensity.Alert);
        }

        public void ReportCombat()
        {
            _lastCombat = Time.unscaledTime;
            _lastAlert = Time.unscaledTime;
            SetIntensity(CombatIntensity.Combat);
        }

        private void Update()
        {
            float now = Time.unscaledTime;

            if (Intensity == CombatIntensity.Combat && now - _lastCombat > combatMemory)
                SetIntensity(CombatIntensity.Alert);

            if (Intensity == CombatIntensity.Alert && now - _lastAlert > alertMemory)
                SetIntensity(CombatIntensity.Calm);
        }

        private void SetIntensity(CombatIntensity value)
        {
            if (Intensity == value) return;
            Intensity = value;
        }
    }
}
