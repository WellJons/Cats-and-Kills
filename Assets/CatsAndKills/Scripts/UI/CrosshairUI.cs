using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class CrosshairUI : MonoBehaviour
    {
        [SerializeField] private PlayerAim2D aim;
        [SerializeField] private float armLength = 8f;
        [SerializeField] private float gap = 4f;
        [SerializeField] private float thickness = 2f;

        public void Configure(PlayerAim2D playerAim)
        {
            aim = playerAim;
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
            GUI.color = new Color(0.95f, 0.95f, 0.98f, 0.92f);

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

            GUI.color = old;
        }
    }
}
