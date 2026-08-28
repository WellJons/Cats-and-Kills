using CatsAndKills.Damage;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CatsAndKills.Player
{
    public sealed class PlayerDeathController : MonoBehaviour
    {
        [SerializeField] private CharacterVitals vitals;
        private bool _dead;
        private GUIStyle _title;
        private GUIStyle _hint;

        public void Configure(CharacterVitals v)
        {
            vitals = v;
        }

        private void OnEnable()
        {
            if (vitals == null) vitals = GetComponent<CharacterVitals>();
            if (vitals != null) vitals.Died += OnDied;
        }

        private void OnDisable()
        {
            if (vitals != null) vitals.Died -= OnDied;
        }

        private void OnDied()
        {
            _dead = true;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            var weapon = GetComponentInChildren<CatsAndKills.Combat.HitscanWeapon2D>();
            if (weapon != null) weapon.enabled = false;

            var grenades = GetComponent<CatsAndKills.Combat.PlayerGrenadeController>();
            if (grenades != null) grenades.enabled = false;
        }

        private void Update()
        {
            if (!_dead) return;

            bool restart =
                (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

            if (restart)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnGUI()
        {
            if (!_dead) return;

            if (_title == null)
            {
                _title = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 42,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                _title.normal.textColor = new Color(0.95f, 0.12f, 0.18f);

                _hint = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter
                };
                _hint.normal.textColor = Color.white;
            }

            GUI.Label(
                new Rect(0, Screen.height * 0.40f, Screen.width, 60),
                "ОПЕРАТОР УБИТ",
                _title);

            GUI.Label(
                new Rect(0, Screen.height * 0.40f + 62, Screen.width, 40),
                "ENTER / START — повторить операцию",
                _hint);
        }
    }
}
