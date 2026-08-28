using CatsAndKills.Combat;
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
        private GUIStyle _action;
        private GUIStyle _actionCost;

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

            _action =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

            _action.normal.textColor =
                new Color(
                    0.94f,
                    0.95f,
                    0.99f,
                    1f);

            _actionCost =
                new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleCenter
                };

            _actionCost.normal.textColor =
                new Color(
                    0.96f,
                    0.72f,
                    0.22f,
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

            if (!tactical.IsPlayerTurn &&
                playerController != null &&
                playerController.Overwatch != null &&
                playerController.Overwatch.IsArmed)
            {
                GUI.Label(
                    new Rect(
                        44f,
                        82f,
                        width - 80f,
                        24f),
                    "OVERWATCH // контроль линии активен",
                    _hint);
            }

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

                string mode;

                if (playerController != null &&
                    playerController.GrenadeTargeting)
                {
                    mode =
                        "GRENADE TARGET // ЛКМ бросить";
                }
                else if (playerController != null &&
                         playerController.MolotovTargeting)
                {
                    mode =
                        "MOLOTOV // ЛКМ поджечь область";
                }
                else if (playerController != null &&
                         playerController.SmokeTargeting)
                {
                    mode =
                        "SMOKE // ЛКМ закрыть линию обзора";
                }
                else
                {
                    TacticalUtilityBelt belt =
                        playerController != null
                            ? playerController.UtilityBelt
                            : null;

                    string utility =
                        belt != null
                            ? "  |  M молотов " +
                              belt.MolotovCount +
                              "  |  X дым " +
                              belt.SmokeCount
                            : string.Empty;

                    mode =
                        "ЛКМ движение  |  ПКМ огонь  |  O наблюдение  |  G граната  |  R перезарядка" +
                        utility +
                        "  |  Enter конец хода";
                }

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
                    string collarText =
                        collar.IsUnlocked
                            ? "Q // " +
                              collar.TacticalAbilityName +
                              "   INSTABILITY " +
                              collar.Instability.ToString("0") +
                              "%"
                            : "COLLAR // DAMAGED SIGNAL // ПРОТОКОЛ НЕИЗВЕСТЕН";

                    GUI.Label(
                        new Rect(
                            515f,
                            40f,
                            470f,
                            25f),
                        collarText,
                        _hint);

                    if (collar.IsUnlocked)
                    {
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
            }

            if (tactical.IsPlayerTurn)
            {
                DrawActionBar(
                    width,
                    Screen.height / scale);
            }

            GUI.matrix = old;
        }

        private void DrawActionBar(
            float width,
            float height)
        {
            string[] labels =
            {
                "MOVE",
                "FIRE",
                "OVERWATCH",
                "GRENADE",
                "MOLOTOV",
                "SMOKE",
                "RELOAD",
                "END TURN"
            };

            string[] costs =
            {
                "1 AP / CELL",
                "3 AP",
                "3 AP",
                "4 AP",
                "4 AP",
                "3 AP",
                "2 AP",
                "ENTER"
            };

            string[] keys =
            {
                "ЛКМ / WASD",
                "ПКМ / F",
                "O",
                "G",
                "M",
                "X",
                "R",
                "ENTER"
            };

            float itemWidth = 112f;
            float gap = 6f;

            float total =
                labels.Length *
                itemWidth +
                (labels.Length - 1) *
                gap;

            float startX =
                (width - total) *
                0.5f;

            float y =
                height -
                92f;

            for (int i = 0;
                 i < labels.Length;
                 i++)
            {
                Rect box =
                    new Rect(
                        startX +
                        i *
                        (itemWidth + gap),
                        y,
                        itemWidth,
                        58f);

                Color oldColor =
                    GUI.color;

                bool affordable =
                    i == 0 ||
                    i == 7 ||
                    tactical.PlayerAP >=
                    ActionCost(i);

                GUI.color =
                    affordable
                        ? new Color(
                            0.10f,
                            0.12f,
                            0.18f,
                            0.94f)
                        : new Color(
                            0.07f,
                            0.07f,
                            0.09f,
                            0.82f);

                GUI.Box(
                    box,
                    GUIContent.none);

                GUI.color = oldColor;

                GUI.Label(
                    new Rect(
                        box.x + 4f,
                        box.y + 5f,
                        box.width - 8f,
                        18f),
                    labels[i],
                    _action);

                GUI.Label(
                    new Rect(
                        box.x + 4f,
                        box.y + 23f,
                        box.width - 8f,
                        15f),
                    costs[i],
                    _actionCost);

                GUI.Label(
                    new Rect(
                        box.x + 4f,
                        box.y + 38f,
                        box.width - 8f,
                        14f),
                    keys[i],
                    _hint);
            }
        }

        private static int ActionCost(
            int actionIndex)
        {
            switch (actionIndex)
            {
                case 1:
                    return 3;
                case 2:
                    return 3;
                case 3:
                    return 4;
                case 4:
                    return 4;
                case 5:
                    return 3;
                case 6:
                    return 2;
                default:
                    return 0;
            }
        }
    }
}
