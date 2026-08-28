using UnityEngine;

namespace CatsAndKills.Visual
{
    [CreateAssetMenu(
        menuName = "Cats and Kills/Visual/Production Art Pack",
        fileName = "ProductionArtPack")]
    public sealed class ProductionArtPack : ScriptableObject
    {
        [Header("Characters")]
        public DirectionalSpriteSet player;
        public DirectionalSpriteSet pistolier;
        public DirectionalSpriteSet rifleman;
        public DirectionalSpriteSet machineGunner;
        public DirectionalSpriteSet demolitionist;

        [Header("Weapons")]
        public Sprite rifle;
        public Sprite pistol;
        public Sprite shotgun;
        public Sprite machineGun;
        public Sprite grenade;

        [Header("Environment")]
        public Sprite floorIndustrial;
        public Sprite floorOffice;
        public Sprite wallStraight;
        public Sprite wallCorner;
        public Sprite wallDamaged;
        public Sprite reinforcedDoor;
        public Sprite crateLight;
        public Sprite crateHeavy;
        public Sprite fuelDrum;
        public Sprite terminal;
        public Sprite fence;
        public Sprite pipeCluster;
        public Sprite lamp;
        public Sprite debris;
        public Sprite propagandaPoster;

        [Header("UI")]
        public Sprite uiPortrait;
        public Sprite uiObjectiveIcon;
        public Sprite uiGrenadeIcon;
        public Sprite uiMedkitIcon;

        [Header("FX")]
        public Sprite muzzleFlash;
        public Sprite bloodDrop;
        public Sprite bulletHole;
        public Sprite spark;
        public Sprite casing;
        public Sprite smoke;
        public Sprite explosion;
        public Sprite softShadow;

        public bool HasMinimumPlayableArt =>
            player != null &&
            rifleman != null &&
            rifle != null &&
            pistol != null &&
            shotgun != null &&
            machineGun != null &&
            floorIndustrial != null &&
            wallStraight != null &&
            reinforcedDoor != null &&
            crateLight != null &&
            fuelDrum != null &&
            terminal != null;
    }
}
