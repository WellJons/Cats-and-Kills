using CatsAndKills.Player;
using CatsAndKills.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatsAndKills.World
{
    public static class CheckpointSystem
    {
        public static bool HasCheckpoint { get; private set; }
        public static Vector2 Position { get; private set; }
        public static string Label { get; private set; }

        public static void Set(Vector2 position, string label)
        {
            Position = position;
            Label = label;
            HasCheckpoint = true;

            RadioDialogueSystem.Instance?.ShowTransient(
                "КОНТРОЛЬНАЯ ТОЧКА // " + label,
                1.1f);
        }

        public static void Clear()
        {
            HasCheckpoint = false;
            Position = Vector2.zero;
            Label = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void HookSceneLoad()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!HasCheckpoint || scene.name != "PlayableCombatSandbox")
                return;

            GameObject restorer = new GameObject("Checkpoint Restorer");
            Object.DontDestroyOnLoad(restorer);
            restorer.AddComponent<CheckpointRestorer2D>();
        }
    }

    public sealed class CheckpointRestorer2D : MonoBehaviour
    {
        private int _frames;

        private void Update()
        {
            _frames++;
            if (_frames < 2) return;

            PlayerMotor2D player = FindFirstObjectByType<PlayerMotor2D>();
            if (player != null)
                player.transform.position = CheckpointSystem.Position;

            Destroy(gameObject);
        }
    }

    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private string checkpointName = "SECTOR";
        [SerializeField] private Vector2 respawnPosition;
        private bool _used;

        public void Configure(string label, Vector2 respawn)
        {
            checkpointName = label;
            respawnPosition = respawn;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_used) return;

            PlayerMotor2D player = other.GetComponentInParent<PlayerMotor2D>();
            if (player == null) return;

            _used = true;
            CheckpointSystem.Set(respawnPosition, checkpointName);
        }
    }
}
