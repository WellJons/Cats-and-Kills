using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using CatsAndKills.World;
using UnityEngine;

namespace CatsAndKills.UI
{
    [DisallowMultipleComponent]
    public sealed class ConceptHUD : MonoBehaviour
    {
        private CharacterVitals _vitals;
        private PlayerArsenal _arsenal;
        private PlayerGrenadeController _grenades;
        private CollarAbility _collar;
        private MissionDirector _mission;
        private PlayerSuppression2D _suppression;

        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private Sprite objectiveIconSprite;
        [SerializeField] private Sprite grenadeIconSprite;
        [SerializeField] private Sprite medkitIconSprite;

        private GUIStyle _title;
        private GUIStyle _small;
        private GUIStyle _ammo;
        private GUIStyle _objective;

        private float _damageFlash;

        private static readonly Color Panel =
            new Color(0.025f, 0.030f, 0.050f, 0.90f);

        private static readonly Color Line =
            new Color(0.55f, 0.16f, 0.42f, 0.95f);

        private static readonly Color Cyan =
            new Color(0.16f, 0.78f, 0.94f, 1f);

        private static readonly Color Red =
            new Color(0.93f, 0.10f, 0.19f, 1f);

        private void Awake()
        {
            Bind();
        }

        private void OnEnable()
        {
            Bind();

            if (_vitals != null)
                _vitals.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            if (_vitals != null)
                _vitals.Damaged -= OnDamaged;
        }

        private void Update()
        {
            if (_vitals == null)
                Bind();

            _damageFlash =
                Mathf.MoveTowards(
                    _damageFlash,
                    0f,
                    Time.unscaledDeltaTime * 2.2f);
        }

        public void ConfigureSkin(
            Sprite portrait,
            Sprite objectiveIcon,
            Sprite grenadeIcon,
            Sprite medkitIcon)
        {
            portraitSprite = portrait;
            objectiveIconSprite = objectiveIcon;
            grenadeIconSprite = grenadeIcon;
            medkitIconSprite = medkitIcon;
        }

        private void Bind()
        {
            PlayerMotor2D player =
                FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
            {
                _vitals =
                    player.GetComponent<CharacterVitals>();

                _arsenal =
                    player.GetComponent<PlayerArsenal>();

                _grenades =
                    player.GetComponent<PlayerGrenadeController>();

                _collar =
                    player.GetComponent<CollarAbility>();

                _suppression =
                    player.GetComponent<PlayerSuppression2D>();
            }

            _mission =
                FindAnyObjectByType<MissionDirector>();
        }

        private void EnsureStyles()
        {
            if (_title != null)
                return;

            _title =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 19,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };

            _title.normal.textColor =
                Color.white;

            _small =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft
                };

            _small.normal.textColor =
                new Color(
                    0.78f,
                    0.84f,
                    0.94f,
                    1f);

            _ammo =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight
                };

            _ammo.normal.textColor =
                Color.white;

            _objective =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperRight,
                    wordWrap = true
                };

            _objective.normal.textColor =
                new Color(
                    0.92f,
                    0.93f,
                    0.99f,
                    1f);
        }

        private void OnGUI()
        {
            EnsureStyles();

            float scale =
                Mathf.Clamp(
                    Screen.height / 1080f,
                    0.78f,
                    1.45f);

            Matrix4x4 oldMatrix =
                GUI.matrix;

            GUI.matrix =
                Matrix4x4.Scale(
                    new Vector3(
                        scale,
                        scale,
                        1f));

            float w =
                Screen.width / scale;

            float h =
                Screen.height / scale;

            DrawPlayerPanel(
                28f,
                h - 174f,
                390f,
                132f);

            DrawAmmoPanel(
                w - 330f,
                h - 155f,
                300f,
                112f);

            DrawObjective(
                w - 500f,
                32f,
                465f,
                120f);

            DrawSuppression(
                w,
                h);

            DrawDamageOverlay(
                w,
                h);

            GUI.matrix = oldMatrix;
        }

        private void DrawPlayerPanel(
            float x,
            float y,
            float w,
            float h)
        {
            DrawPanel(
                new Rect(x, y, w, h),
                Line);

            if (_vitals == null)
                return;

            float contentX =
                x + 18f;

            if (portraitSprite != null &&
                portraitSprite.texture != null)
            {
                DrawSpriteTexture(
                    new Rect(
                        x + 12f,
                        y + 12f,
                        86f,
                        86f),
                    portraitSprite);

                contentX =
                    x + 108f;
            }

            GUI.Label(
                new Rect(
                    contentX,
                    y + 10f,
                    170f,
                    28f),
                "CK // VETERAN",
                _title);

            float hp =
                Mathf.Max(
                    0f,
                    _vitals.Health);

            GUI.Label(
                new Rect(
                    x + 270f,
                    y + 11f,
                    95f,
                    24f),
                $"{hp:0}/{_vitals.MaxHealth:0}",
                _small);

            DrawBar(
                new Rect(
                    contentX,
                    y + 43f,
                    x + w - 24f - contentX,
                    16f),
                hp /
                Mathf.Max(
                    1f,
                    _vitals.MaxHealth),
                Red);

            string limbs =
                $"LA {Limb(_vitals.LeftArmDisabled)}   " +
                $"RA {Limb(_vitals.RightArmDisabled)}   " +
                $"LL {Limb(_vitals.LeftLegDisabled)}   " +
                $"RL {Limb(_vitals.RightLegDisabled)}";

            GUI.Label(
                new Rect(
                    contentX,
                    y + 66f,
                    x + w - 24f - contentX,
                    22f),
                limbs,
                _small);

            string collar =
                _collar == null
                    ? "COLLAR // ---"
                    : _collar.IsActive
                        ? "COLLAR // OVERCLOCK"
                        : _collar.Cooldown01 > 0f
                            ? "COLLAR // RECOVERING"
                            : "COLLAR // READY [Q]";

            GUI.Label(
                new Rect(
                    contentX,
                    y + 92f,
                    x + w - 24f - contentX,
                    21f),
                collar,
                _small);

            if (_collar != null)
            {
                float ready =
                    _collar.IsActive
                        ? 1f
                        : 1f -
                          _collar.Cooldown01;

                DrawBar(
                    new Rect(
                        contentX,
                        y + 115f,
                        x + w - 24f - contentX,
                        7f),
                    ready,
                    Cyan);
            }
        }

        private void DrawAmmoPanel(
            float x,
            float y,
            float w,
            float h)
        {
            DrawPanel(
                new Rect(x, y, w, h),
                new Color(
                    0.16f,
                    0.45f,
                    0.72f,
                    0.95f));

            if (_arsenal == null ||
                _arsenal.Weapon == null)
            {
                return;
            }

            string weapon =
                _arsenal.Current != null
                    ? _arsenal.Current.weaponName
                    : "WEAPON";

            GUI.Label(
                new Rect(
                    x + 16f,
                    y + 11f,
                    150f,
                    24f),
                weapon.ToUpperInvariant(),
                _small);

            GUI.Label(
                new Rect(
                    x + 112f,
                    y + 24f,
                    166f,
                    48f),
                $"{_arsenal.Weapon.Magazine:00} / {_arsenal.Weapon.Reserve:000}",
                _ammo);

            string grenades =
                _grenades == null
                    ? string.Empty
                    : _grenades.IsCooking
                        ? $"GRENADE  {_grenades.CookRemaining:0.0}s"
                        : $"GRENADES  {_grenades.GrenadeCount}";

            float grenadeTextX =
                x + 16f;

            if (grenadeIconSprite != null &&
                grenadeIconSprite.texture != null)
            {
                DrawSpriteTexture(
                    new Rect(
                        x + 14f,
                        y + 72f,
                        30f,
                        30f),
                    grenadeIconSprite);

                grenadeTextX =
                    x + 50f;
            }

            GUI.Label(
                new Rect(
                    grenadeTextX,
                    y + 78f,
                    220f,
                    22f),
                grenades,
                _small);
        }

        private void DrawObjective(
            float x,
            float y,
            float w,
            float h)
        {
            if (_mission == null ||
                string.IsNullOrEmpty(
                    _mission.CurrentObjective))
            {
                return;
            }

            DrawPanel(
                new Rect(
                    x,
                    y,
                    w,
                    h),
                new Color(
                    0.75f,
                    0.62f,
                    0.18f,
                    0.92f));

            float textX =
                x + 14f;

            if (objectiveIconSprite != null &&
                objectiveIconSprite.texture != null)
            {
                DrawSpriteTexture(
                    new Rect(
                        x + 16f,
                        y + 22f,
                        56f,
                        56f),
                    objectiveIconSprite);

                textX =
                    x + 84f;
            }

            GUI.Label(
                new Rect(
                    textX,
                    y + 12f,
                    x + w - 18f - textX,
                    h - 24f),
                _mission.CurrentObjective,
                _objective);
        }

        private void DrawSuppression(
            float w,
            float h)
        {
            if (_suppression == null ||
                !_suppression.IsSuppressed)
            {
                return;
            }

            float value =
                Mathf.Clamp01(
                    _suppression.Suppression);

            Color old =
                GUI.color;

            GUI.color =
                new Color(
                    0.70f,
                    0.02f,
                    0.05f,
                    0.08f +
                    value * 0.11f);

            GUI.DrawTexture(
                new Rect(
                    0f,
                    0f,
                    w,
                    h),
                Texture2D.whiteTexture);

            GUI.color = old;
        }

        private void DrawDamageOverlay(
            float w,
            float h)
        {
            if (_damageFlash <= 0.001f)
                return;

            Color old =
                GUI.color;

            GUI.color =
                new Color(
                    0.76f,
                    0.01f,
                    0.025f,
                    _damageFlash *
                    0.16f);

            GUI.DrawTexture(
                new Rect(
                    0f,
                    0f,
                    w,
                    h),
                Texture2D.whiteTexture);

            GUI.color = old;
        }

        private static void DrawSpriteTexture(
            Rect rect,
            Sprite sprite)
        {
            if (sprite == null ||
                sprite.texture == null)
            {
                return;
            }

            Color old =
                GUI.color;

            GUI.color = Color.white;

            GUI.DrawTexture(
                rect,
                sprite.texture,
                ScaleMode.ScaleToFit,
                true);

            GUI.color = old;
        }

        private static void DrawPanel(
            Rect rect,
            Color line)
        {
            Color old =
                GUI.color;

            GUI.color = Panel;

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture);

            GUI.color = line;

            GUI.DrawTexture(
                new Rect(
                    rect.x,
                    rect.y,
                    rect.width,
                    2f),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(
                    rect.x,
                    rect.yMax - 2f,
                    rect.width,
                    2f),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(
                    rect.x,
                    rect.y,
                    2f,
                    rect.height),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(
                    rect.xMax - 2f,
                    rect.y,
                    2f,
                    rect.height),
                Texture2D.whiteTexture);

            GUI.color = old;
        }

        private static void DrawBar(
            Rect rect,
            float value,
            Color fill)
        {
            Color old =
                GUI.color;

            GUI.color =
                new Color(
                    0.02f,
                    0.025f,
                    0.04f,
                    1f);

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture);

            GUI.color = fill;

            GUI.DrawTexture(
                new Rect(
                    rect.x + 2f,
                    rect.y + 2f,
                    Mathf.Max(
                        0f,
                        rect.width - 4f) *
                    Mathf.Clamp01(value),
                    Mathf.Max(
                        0f,
                        rect.height - 4f)),
                Texture2D.whiteTexture);

            GUI.color = old;
        }

        private static string Limb(
            bool disabled)
        {
            return disabled
                ? "X"
                : "OK";
        }

        private void OnDamaged(
            DamageInfo info)
        {
            _damageFlash = 1f;
        }
    }
}
