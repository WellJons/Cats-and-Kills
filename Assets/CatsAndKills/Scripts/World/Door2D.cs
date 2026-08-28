using System.Collections;
using CatsAndKills.AI;
using CatsAndKills.Combat;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class Door2D : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool locked;
        [SerializeField] private Collider2D blocker;
        [SerializeField] private Transform visual;

        private bool _open;

        public string InteractionPrompt =>
            locked ? "ДВЕРЬ ЗАБЛОКИРОВАНА" : (_open ? "ЗАКРЫТЬ [E]" : "ОТКРЫТЬ [E]");

        public void Configure(Collider2D col, Transform visualTransform, bool isLocked = false)
        {
            blocker = col;
            visual = visualTransform;
            locked = isLocked;
        }

        public void SetLocked(bool value)
        {
            locked = value;
        }

        public void Interact()
        {
            if (locked) return;
            _open = !_open;
            NoiseSystem.Report(transform.position, 4.5f, gameObject);
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float from = visual != null ? visual.localEulerAngles.z : 0f;
            float target = _open ? 90f : 0f;
            float t = 0f;

            if (blocker != null)
                blocker.enabled = !_open;

            NavigationGrid2D nav = FindFirstObjectByType<NavigationGrid2D>();
            if (nav != null)
                nav.Invoke(nameof(NavigationGrid2D.Build), 0.08f);

            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                if (visual != null)
                {
                    float angle = Mathf.LerpAngle(from, target, Mathf.SmoothStep(0f, 1f, t));
                    visual.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
                yield return null;
            }
        }
    }
}
