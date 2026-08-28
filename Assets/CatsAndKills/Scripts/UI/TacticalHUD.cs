using CatsAndKills.Player;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class TacticalHUD : MonoBehaviour
    {
        [SerializeField] private TacticalCombatDirector tactical;
        [SerializeField] private TacticalPlayerController playerController;
        [SerializeField] private CollarAbility collar;

        private GUIStyle _phase;
        private GUIStyle _hint;

        private void Awake()
        {
            tactical =
                tactical != null
                    ? tactical
                    : TacticalCombatDirector.Instance;

            playerController =
                playerController != null
                    ? playerController
                    : FindAnyObjectByType<TacticalPlayerController>();

            collar =
                collar != null
                    ? collar
                    : FindAnyObjectByType<CollarAbility>();
        }

        private void EnsureStyles()
        {
            if (_phase != null)
                return;

            _phase =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 21,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };

            _phase.normal.textColor =
                new Color(
                    0.95f,
                    0.88f,
                    0.58f,
                    1f);

            _hint =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft
                };

            _hint.normal.textColor =
                new Color(
                    0.82f,
                    0.86f,
                    0.94f,
                    1f);
        }

        private void OnGUI()
        {
            if (tactical == null)
                tactical = TacticalCombatDirector.Instance;

            if (tactical == null ||
                !tactical.IsTacticalCombat)
            {
                return;
            }

            EnsureStyles();

            float scale =
                Mathf.Clamp(
                    Screen.height / 1080f,
                    0.78f,
                    1.4f);

            Matrix4x4 old =
                GUI.matrix;

            GUI.matrix =
                Matrix4x4.Scale(
                    new Vector3(
                        scale,
                        scale,
                        1f));

            float width =
                Screen.width / scale;

            Rect panel =
                new Rect(
                    28f,
                    28f,
                    465f,
                    102f);

            GUI.Box(
                panel,
                GUIContent.none);

            string phase =
                tactical.IsPlayerTurn
                    ? "PLAYER PHASE"
                    : "ENEMY PHASE";

            GUI.Label(
                new Rect(
                    44f,
                    38f,
                    260f,
                    30f),
                phase +
                "  //  ROUND " +
                tactical.RoundIndex,
                _phase);

            if (tactical.IsPlayerTurn)
            {
                float x =
                    46f;

                for (int i = 0;
                     i < tactical.MaxPlayerAP + 8;
                     i++)
                {
                    if (i >= tactical.PlayerAP &&
                        i >= tactical.MaxPlayerAP)
                    {
                        break;
                    }

                    Rect pip =
                        new Rect(
                            x + i * 26f,
                            73f,
                            20f,
                            13f);

                    Color oldColor =
                        GUI.color;

                    GUI.color =
                        i < tactical.PlayerAP
                            ? new Color(
                                0.95f,
                                0.72f,
                                0.20f,
                                1f)
                            : new Color(
                                0.23f,
                                0.25f,
                                0.30f,
                                1f);

                    GUI.Box(
                        pip,
                        GUIContent.none);

                    GUI.color =
                        oldColor;
                }

                string mode =
                    playerController != null &&
                    playerController.GrenadeTargeting
                        ? "GRENADE TARGET // ЛКМ бросить"
                        : "ЛКМ движение  |  ПКМ огонь  |  G граната  |  R перезарядка  |  Enter конец хода";

                GUI.Label(
                    new Rect(
                        44f,
                        91f,
                        width - 80f,
                        24f),
                    mode,
                    _hint);

                if (collar != null)
                {
                    GUI.Label(
                        new Rect(
                            515f,
                            40f,
                            470f,
                            25f),
                        "Q // " +
                        collar.TacticalAbilityName +
                        "   INSTABILITY " +
                        collar.Instability.ToString("0") +
                        "%",
                        _hint);

                    float instability =
                        collar.Instability01;

                    Rect bg =
                        new Rect(
                            515f,
                            72f,
                            240f,
                            10f);

                    GUI.Box(
                        bg,
                        GUIContent.none);

                    Color oldColor =
                        GUI.color;

                    GUI.color =
                        Color.Lerp(
                            new Color(
                                0.18f,
                                0.72f,
                                0.92f),
                            new Color(
                                1f,
                                0.16f,
                                0.18f),
                            instability);

                    GUI.Box(
                        new Rect(
                            bg.x,
                            bg.y,
                            bg.width *
                            instability,
                            bg.height),
                        GUIContent.none);

                    GUI.color =
                        oldColor;
                }
            }

            GUI.matrix = old;
        }
    }
}
