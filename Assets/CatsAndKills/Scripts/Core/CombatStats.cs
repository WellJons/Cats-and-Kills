using UnityEngine;

namespace CatsAndKills.Core
{
    public sealed class CombatStats : MonoBehaviour
    {
        public static CombatStats Instance { get; private set; }

        public int ShotsFired { get; private set; }
        public int Hits { get; private set; }
        public int Kills { get; private set; }
        public int GrenadesThrown { get; private set; }
        public float ElapsedSeconds { get; private set; }

        public float Accuracy =>
            ShotsFired > 0 ? Mathf.Clamp01((float)Hits / ShotsFired) : 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Time.timeScale > 0f)
                ElapsedSeconds += Time.deltaTime;
        }

        public void RecordShot(int projectiles = 1)
        {
            ShotsFired += Mathf.Max(1, projectiles);
        }

        public void RecordHit()
        {
            Hits++;
        }

        public void RecordKill()
        {
            Kills++;
        }

        public void RecordGrenade()
        {
            GrenadesThrown++;
        }
    }
}
