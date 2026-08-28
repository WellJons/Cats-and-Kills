using CatsAndKills.Core;
using CatsAndKills.UI;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class PlayerInteraction2D : MonoBehaviour
    {
        [SerializeField] private float radius = 1.25f;

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (!CKInput.InteractPressed) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            IInteractable closest = null;
            float best = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable == null) interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable == null) continue;

                float d = Vector2.Distance(transform.position, hit.bounds.center);
                if (d < best)
                {
                    best = d;
                    closest = interactable;
                }
            }

            if (closest != null)
            {
                closest.Interact();
                RadioDialogueSystem.Instance?.ShowTransient(closest.InteractionPrompt, 0.65f);
            }
        }
    }
}
