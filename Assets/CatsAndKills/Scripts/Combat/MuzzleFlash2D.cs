using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.Combat
{
    public sealed class MuzzleFlash2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Light2D muzzleLight;
        [SerializeField] private float lightRadius = 1.9f;
        [SerializeField] private float lightIntensity = 1.7f;
        private Coroutine _routine;

        public void Configure(SpriteRenderer sr)
        {
            spriteRenderer = sr;

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            EnsureLight();
        }

        private void EnsureLight()
        {
            if (muzzleLight == null)
                muzzleLight = GetComponent<Light2D>();

            if (muzzleLight == null)
                muzzleLight = gameObject.AddComponent<Light2D>();

            muzzleLight.lightType =
                Light2D.LightType.Point;

            muzzleLight.color =
                new Color(
                    1f,
                    0.53f,
                    0.20f,
                    1f);

            muzzleLight.pointLightInnerRadius =
                lightRadius * 0.10f;

            muzzleLight.pointLightOuterRadius =
                lightRadius;

            muzzleLight.falloffIntensity = 0.78f;
            muzzleLight.overlapOperation =
                Light2D.OverlapOperation.Additive;

            muzzleLight.intensity = 0f;
            muzzleLight.enabled = false;
        }

        public void Flash()
        {
            if (spriteRenderer == null) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            EnsureLight();

            spriteRenderer.enabled = true;

            transform.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    Random.Range(
                        -18f,
                        18f));

            transform.localScale =
                Vector3.one *
                Random.Range(
                    0.78f,
                    1.25f);

            if (muzzleLight != null)
            {
                muzzleLight.enabled = true;
                muzzleLight.intensity =
                    lightIntensity *
                    Random.Range(
                        0.85f,
                        1.25f);
            }

            yield return new WaitForSecondsRealtime(
                Random.Range(
                    0.026f,
                    0.052f));

            spriteRenderer.enabled = false;

            if (muzzleLight != null)
            {
                muzzleLight.intensity = 0f;
                muzzleLight.enabled = false;
            }
        }
    }
}
