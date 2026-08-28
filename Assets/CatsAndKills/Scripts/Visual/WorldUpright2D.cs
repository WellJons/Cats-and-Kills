using UnityEngine;

namespace CatsAndKills.Visual
{
    [DisallowMultipleComponent]
    public sealed class WorldUpright2D : MonoBehaviour
    {
        [SerializeField] private float zRotation;

        public void Configure(float rotation = 0f)
        {
            zRotation = rotation;
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, zRotation);
        }
    }
}
