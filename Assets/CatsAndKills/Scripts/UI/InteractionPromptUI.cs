using CatsAndKills.World;
using UnityEngine;

namespace CatsAndKills.UI
{
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float radius = 1.35f;

        private GUIStyle _style;

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void OnGUI()
        {
            if (player == null || Time.timeScale <= 0f)
                return;

            string prompt = FindPrompt();
            if (string.IsNullOrEmpty(prompt))
                return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    alignment = TextAnchor.MiddleCenter
                };
                _style.normal.textColor = Color.white;
            }

            float width = Mathf.Min(420f, Screen.width - 40f);
            GUI.Box(
                new Rect(
                    (Screen.width - width) * 0.5f,
                    Screen.height - 205f,
                    width,
                    38f),
                prompt,
                _style);
        }

        private string FindPrompt()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, radius);
            float best = float.MaxValue;
            string result = null;

            foreach (Collider2D hit in hits)
            {
                if (hit == null) continue;

                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null)
                    interactable = hit.GetComponentInParent<IInteractable>();

                if (interactable == null) continue;

                float distance = Vector2.Distance(player.position, hit.bounds.center);
                if (distance >= best) continue;

                best = distance;
                result = interactable.InteractionPrompt;
            }

            return result;
        }
    }
}
