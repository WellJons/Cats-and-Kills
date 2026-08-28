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

        private void LateUpdate()
        {
            float t = 1f - Mathf.Exp(-_returnSharpness * Time.unscaledDeltaTime);
            _kickDistance = Mathf.Lerp(_kickDistance, 0f, t);
            _kickRotation = Mathf.Lerp(_kickRotation, 0f, t);

            transform.localPosition = _basePos + Vector3.left * _kickDistance;
            transform.localRotation = _baseRot * Quaternion.Euler(0f, 0f, _kickRotation);
        }
    }
}
