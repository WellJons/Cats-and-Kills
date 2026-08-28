using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class CrosshairUI : MonoBehaviour
    {
        public static CrosshairUI Instance { get; private set; }

        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private float armLength = 8f;
        [SerializeField] private float gap = 4f;
        [SerializeField] private float thickness = 2f;

        private float _hitUntil;
        private bool _killHit;

        private void Awake()
        {
            Instance = this;
        }

        public void Configure(PlayerAim2D playerAim)
        {
            aim = playerAim;
        }

        public void FlashHit(bool killed)
        {
            _hitUntil = Time.unscaledTime + (killed ? 0.18f : 0.11f);
            _killHit = killed;
        }

        private void OnGUI()
        {
            if (aim == null || Camera.main == null || Time.timeScale <= 0f)
                return;

            Vector3 screen = Camera.main.WorldToScreenPoint(aim.AimWorldPoint);
            if (screen.z < 0f) return;

            float x = screen.x;
            float y = Screen.height - screen.y;

            Color old = GUI.color;

            bool hit = Time.unscaledTime < _hitUntil;
            GUI.color = hit
                ? (_killHit
                    ? new Color(1f, 0.18f, 0.22f, 1f)
                    : new Color(1f, 0.85f, 0.35f, 1f))
                : new Color(0.95f, 0.95f, 0.98f, 0.92f);

            GUI.DrawTexture(
                new Rect(x - gap - armLength, y - thickness * 0.5f, armLength, thickness),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(x + gap, y - thickness * 0.5f, armLength, thickness),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(x - thickness * 0.5f, y - gap - armLength, thickness, armLength),
                Texture2D.whiteTexture);

            GUI.DrawTexture(
                new Rect(x - thickness * 0.5f, y + gap, thickness, armLength),
                Texture2D.whiteTexture);

            if (hit)
            {
                float d = gap + 4f;
                float l = 6f;
                DrawDiagonal(x - d, y - d, -l, -l);
                DrawDiagonal(x + d, y - d, l, -l);
                DrawDiagonal(x - d, y + d, -l, l);
                DrawDiagonal(x + d, y + d, l, l);
            }

            GUI.color = old;
        }

        private static void DrawDiagonal(
            float startX,
            float startY,
            float deltaX,
            float deltaY)
        {
            float length = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (length <= 0.01f) return;

            Matrix4x4 oldMatrix = GUI.matrix;
            float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg;

            GUIUtility.RotateAroundPivot(
                angle,
                new Vector2(startX, startY));

            GUI.DrawTexture(
                new Rect(startX, startY - 1f, length, 2f),
                Texture2D.whiteTexture);

            GUI.matrix = oldMatrix;
        }
    }
}
