using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using CatsAndKills.World;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        public static PrototypeHUD Instance { get; private set; }

        [SerializeField] private CharacterVitals playerVitals;
        [SerializeField] private PlayerArsenal arsenal;
        [SerializeField] private PlayerGrenadeController grenades;
        [SerializeField] private CollarAbility collar;
        [SerializeField] private MissionDirector mission;

        private float _glitch;
        private GUIStyle _large;
        private GUIStyle _small;
        private GUIStyle _objective;

        public void Configure(
            CharacterVitals vitals,
            PlayerArsenal playerArsenal,
            PlayerGrenadeController grenadeController,
            CollarAbility collarAbility,
            MissionDirector missionDirector)
        {
            playerVitals = vitals;
            arsenal = playerArsenal;
            grenades = grenadeController;
            collar = collarAbility;
            mission = missionDirector;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void SetGlitch(float amount)
        {
            _glitch = Mathf.Clamp01(amount);
        }

        public void BindMission(MissionDirector missionDirector)
        {
            mission = missionDirector;
        }

        private void Ensure()
        {
            if (_large != null) return;

            _large = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            _large.normal.textColor = Color.white;

            _small = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            _small.normal.textColor = new Color(0.8f, 0.84f, 0.9f);

            _objective = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight
            };
            _objective.normal.textColor = new Color(0.9f, 0.92f, 0.97f);
        }

        private void OnGUI()
        {
            Ensure();

            if (playerVitals != null)
            {
                float hp = Mathf.Max(0f, playerVitals.Health);
                GUI.Label(new Rect(22, Screen.height - 84, 280, 32), $"HP {hp:0}", _large);

                float width = 220f;
                GUI.Box(new Rect(22, Screen.height - 46, width, 12), GUIContent.none);
                Color old = GUI.color;
                GUI.color = new Color(0.85f, 0.08f, 0.18f);
                GUI.DrawTexture(
                    new Rect(
                        24,
                        Screen.height - 44,
                        (width - 4) * Mathf.Clamp01(hp / playerVitals.MaxHealth),
                        8),
                    Texture2D.whiteTexture);
                GUI.color = old;
            }

            if (arsenal != null && arsenal.Weapon != null && arsenal.Current != null)
            {
                string ammo = $"{arsenal.Weapon.Magazine:00} / {arsenal.Weapon.Reserve:000}";
                GUI.Label(new Rect(Screen.width - 250, Screen.height - 86, 220, 32), ammo, _large);
                GUI.Label(
                    new Rect(Screen.width - 250, Screen.height - 52, 220, 24),
                    arsenal.Current.weaponName,
                    _small);
            }

            if (grenades != null)
                GUI.Label(
                    new Rect(Screen.width - 250, Screen.height - 30, 220, 22),
                    $"GRENADES  {grenades.GrenadeCount}",
                    _small);

            if (collar != null)
            {
                string collarText = collar.IsActive
                    ? "COLLAR // OVERCLOCK"
                    : collar.Cooldown01 > 0f
                        ? $"COLLAR // {(1f - collar.Cooldown01) * 100f:0}%"
                        : "COLLAR // READY [Q]";

                GUI.Label(new Rect(22, 22, 300, 24), collarText, _small);
            }

            if (mission != null && !string.IsNullOrEmpty(mission.CurrentObjective))
                GUI.Label(
                    new Rect(Screen.width - 520, 24, 490, 60),
                    mission.CurrentObjective,
                    _objective);

            GUI.Label(
                new Rect(22, Screen.height - 24, 800, 22),
                "WASD move  •  LMB fire  •  R reload  •  G grenade  •  E return/interact  •  SPACE dash  •  Q collar  •  1/2/3 weapons",
                _small);

            if (_glitch > 0.01f)
            {
                Color old = GUI.color;
                for (int i = 0; i < 9; i++)
                {
                    float y = Random.Range(0f, Screen.height);
                    float h = Random.Range(2f, 18f);
                    GUI.color = new Color(
                        Random.value > .5f ? .95f : .15f,
                        .1f,
                        Random.value > .5f ? .45f : .95f,
                        _glitch * Random.Range(.04f, .14f));

                    GUI.DrawTexture(
                        new Rect(
                            Random.Range(-20f, 20f),
                            y,
                            Screen.width,
                            h),
                        Texture2D.whiteTexture);
                }
                GUI.color = old;
            }
        }
    }
}
