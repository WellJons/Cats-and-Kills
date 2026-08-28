using System.Collections;
using CatsAndKills.AI;
using CatsAndKills.Combat;
using UnityEngine;

namespace CatsAndKills.World
{
    public sealed class Door2D : MonoBehaviour, IInteractable
    {
        private enum DoorAnimationMode
        {
            Swing,
            Slide
        }

        [SerializeField] private bool locked;
        [SerializeField] private Collider2D blocker;
        [SerializeField] private Transform visual;
        [SerializeField] private DoorAnimationMode animationMode =
            DoorAnimationMode.Swing;
        [SerializeField] private Vector2 slideOffset =
            new Vector2(0f, 1.15f);
        [SerializeField] private float animationSpeed = 5f;

        private bool _open;
        private Vector3 _closedLocalPosition;

        public string InteractionPrompt =>
            locked ? "ДВЕРЬ ЗАБЛОКИРОВАНА" : (_open ? "ЗАКРЫТЬ [E]" : "ОТКРЫТЬ [E]");

        public void Configure(Collider2D col, Transform visualTransform, bool isLocked = false)
        {
            blocker = col;
            visual = visualTransform;
            locked = isLocked;

            if (visual != null)
                _closedLocalPosition = visual.localPosition;
        }

        public void ConfigureSlide(
            Vector2 localOffset,
            float speed = 4.5f)
        {
            animationMode =
                DoorAnimationMode.Slide;

            slideOffset =
                localOffset;

            animationSpeed =
                Mathf.Max(
                    0.5f,
                    speed);

            if (visual != null)
                _closedLocalPosition =
                    visual.localPosition;
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
            float t = 0f;

            if (blocker != null)
                blocker.enabled = !_open;

            NavigationGrid2D nav =
                FindAnyObjectByType<NavigationGrid2D>();

            if (nav != null)
                nav.Invoke(
                    nameof(NavigationGrid2D.Build),
                    0.08f);

            if (visual == null)
                yield break;

            if (animationMode ==
                DoorAnimationMode.Slide)
            {
                Vector3 from =
                    visual.localPosition;

                Vector3 target =
                    _open
                        ? _closedLocalPosition +
                          (Vector3)slideOffset
                        : _closedLocalPosition;

                Quaternion rotation =
                    visual.localRotation;

                while (t < 1f)
                {
                    t +=
                        Time.deltaTime *
                        animationSpeed;

                    float eased =
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            Mathf.Clamp01(t));

                    visual.localPosition =
                        Vector3.Lerp(
                            from,
                            target,
                            eased);

                    visual.localRotation =
                        rotation;

                    yield return null;
                }

                visual.localPosition =
                    target;

                yield break;
            }

            float fromAngle =
                visual.localEulerAngles.z;

            float targetAngle =
                _open
                    ? 90f
                    : 0f;

            while (t < 1f)
            {
                t +=
                    Time.deltaTime *
                    animationSpeed;

                float angle =
                    Mathf.LerpAngle(
                        fromAngle,
                        targetAngle,
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            Mathf.Clamp01(t)));

                visual.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle);

                yield return null;
            }
        }
    }
}
