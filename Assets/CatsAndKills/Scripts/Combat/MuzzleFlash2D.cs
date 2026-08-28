using System.Collections;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class MuzzleFlash2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private Coroutine _routine;

        public void Configure(SpriteRenderer sr)
        {
            spriteRenderer = sr;
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }

        public void Flash()
        {
            if (spriteRenderer == null) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            spriteRenderer.enabled = true;
            transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-25f, 25f));
            transform.localScale = Vector3.one * Random.Range(0.65f, 1.15f);
            yield return new WaitForSecondsRealtime(Random.Range(0.025f, 0.045f));
            spriteRenderer.enabled = false;
        }
    }
}
