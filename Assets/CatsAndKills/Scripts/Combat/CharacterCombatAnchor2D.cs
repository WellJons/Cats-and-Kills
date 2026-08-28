using UnityEngine;

namespace CatsAndKills.Combat
{
    [DisallowMultipleComponent]
    public sealed class CharacterCombatAnchor2D : MonoBehaviour
    {
        public bool IsValid { get; private set; }
        public Vector2 AimPoint { get; private set; }
        public Vector2 MuzzlePoint { get; private set; }

        public void SetWorldPoints(
            Vector2 aimPoint,
            Vector2 muzzlePoint)
        {
            AimPoint = aimPoint;
            MuzzlePoint = muzzlePoint;
            IsValid = true;
        }

        public void Clear()
        {
            IsValid = false;
        }
    }
}
