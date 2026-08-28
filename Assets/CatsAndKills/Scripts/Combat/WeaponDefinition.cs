using UnityEngine;

namespace CatsAndKills.Combat
{
    [CreateAssetMenu(menuName = "Cats and Kills/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Rifle";
        public Sprite weaponSprite;
        public bool automatic = true;

        [Header("Ballistics")]
        public float damage = 32f;
        public float range = 28f;
        public float fireRate = 9f;
        public int pellets = 1;
        public float impactForce = 3.8f;
        public float dismemberPower = 0.08f;
        public LayerMask hitMask = ~0;

        [Header("Magazine")]
        public int magazineSize = 30;
        public int startingReserve = 120;
        public float reloadTime = 1.8f;

        [Header("Accuracy")]
        public float baseSpread = 0.45f;
        public float movingSpread = 1.15f;

        [Header("Recoil")]
        public float recoilPerShot = 1.1f;
        public float recoilMax = 7f;
        public float recoilRecovery = 9f;
        public float recoilHorizontal = 0.4f;
        public float visualKickDistance = 0.10f;
        public float visualKickRotation = 3.2f;

        [Header("Feel")]
        public float cameraKick = 0.10f;
        public float cameraKickDecay = 22f;
        public float rumbleLow = 0.18f;
        public float rumbleHigh = 0.30f;
        public float rumbleDuration = 0.055f;

        [Header("Audio")]
        public AudioClip shotClip;
        public AudioClip reloadClip;
        [Range(0f, 1f)] public float shotVolume = 0.75f;
    }
}
