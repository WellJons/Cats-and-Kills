using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class WeaponVisualRecoil2D : MonoBehaviour
    {
        private Vector3 _basePos;
        private Quaternion _baseRot;
        private float _kickDistance;
        private float _kickRotation;
        private float _returnSharpness = 24f;
        private float _reloadBlend;
        private bool _reloading;

        private void Awake()
        {
            _basePos = transform.localPosition;
            _baseRot = transform.localRotation;
        }

        public void Kick(float distance, float rotation)
        {
            _kickDistance += distance;
            _kickRotation += Random.Range(-rotation, rotation);
        }

        public void SetReloading(bool value)
        {
            _reloading = value;
        }

        private void LateUpdate()
        {
            float t = 1f - Mathf.Exp(-_returnSharpness * Time.unscaledDeltaTime);
            _kickDistance = Mathf.Lerp(_kickDistance, 0f, t);
            _kickRotation = Mathf.Lerp(_kickRotation, 0f, t);

            _reloadBlend = Mathf.MoveTowards(
                _reloadBlend,
                _reloading ? 1f : 0f,
                Time.unscaledDeltaTime * 5.8f);

            Vector3 reloadOffset =
                new Vector3(-0.12f, -0.18f, 0f) * _reloadBlend;

            float reloadRotation = -38f * _reloadBlend;

            transform.localPosition =
                _basePos +
                Vector3.left * _kickDistance +
                reloadOffset;

            transform.localRotation =
                _baseRot *
                Quaternion.Euler(
                    0f,
                    0f,
                    _kickRotation + reloadRotation);
        }
    }
}
