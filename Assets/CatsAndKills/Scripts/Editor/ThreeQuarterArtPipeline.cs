#if UNITY_EDITOR
using System.Collections.Generic;
using CatsAndKills.Visual;
using UnityEditor;
using UnityEngine;

namespace CatsAndKills.EditorTools
{
    public static class ThreeQuarterArtPipeline
    {
        public const string ProductionRoot =
            "Assets/CatsAndKills/Art/Production";

        public const string ArtPackPath =
            "Assets/CatsAndKills/Art/Production/ProductionArtPack.asset";

        [MenuItem("Tools/Cats and Kills/3-4 Art/Create Production Folders")]
        public static void CreateProductionFolders()
        {
            EnsureFolder("Assets/CatsAndKills/Art");
            EnsureFolder(ProductionRoot);
            EnsureFolder(ProductionRoot + "/Characters");
            EnsureFolder(ProductionRoot + "/Characters/Player");
            EnsureFolder(ProductionRoot + "/Characters/Pistolier");
            EnsureFolder(ProductionRoot + "/Characters/Rifleman");
            EnsureFolder(ProductionRoot + "/Characters/MachineGunner");
            EnsureFolder(ProductionRoot + "/Characters/Demolitionist");
            EnsureFolder(ProductionRoot + "/Weapons");
            EnsureFolder(ProductionRoot + "/Environment");
            EnsureFolder(ProductionRoot + "/FX");
            EnsureFolder(ProductionRoot + "/UI");

            AssetDatabase.Refresh();

            if (AssetDatabase.LoadAssetAtPath<ProductionArtPack>(ArtPackPath) == null)
            {
                var pack = ScriptableObject.CreateInstance<ProductionArtPack>();
                AssetDatabase.CreateAsset(pack, ArtPackPath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = pack;
            }

            Debug.Log(
                "Cats and Kills 3/4 production art folders are ready: " +
                ProductionRoot);
        }

        [MenuItem("Tools/Cats and Kills/3-4 Art/Validate Production Art")]
        public static void ValidateProductionArt()
        {
            ProductionArtPack pack =
                AssetDatabase.LoadAssetAtPath<ProductionArtPack>(ArtPackPath);

            if (pack == null)
            {
                Debug.LogError(
                    "ProductionArtPack.asset is missing. " +
                    "Run Create Production Folders first.");

                return;
            }

            var missing = new List<string>();

            Check(pack.player, "player 8-direction set", missing);
            Check(pack.pistolier, "pistolier 8-direction set", missing);
            Check(pack.rifleman, "rifleman 8-direction set", missing);
            Check(pack.machineGunner, "machine gunner 8-direction set", missing);
            Check(pack.demolitionist, "demolitionist 8-direction set", missing);

            Check(pack.rifle, "rifle sprite", missing);
            Check(pack.pistol, "pistol sprite", missing);
            Check(pack.shotgun, "shotgun sprite", missing);
            Check(pack.machineGun, "machine gun sprite", missing);
            Check(pack.grenade, "grenade sprite", missing);

            Check(pack.floorIndustrial, "industrial floor", missing);
            Check(pack.wallStraight, "straight 3/4 wall", missing);
            Check(pack.wallCorner, "corner 3/4 wall", missing);
            Check(pack.reinforcedDoor, "reinforced door", missing);
            Check(pack.crateLight, "light cover crate", missing);
            Check(pack.fuelDrum, "fuel drum", missing);
            Check(pack.terminal, "archive terminal", missing);

            Check(pack.muzzleFlash, "muzzle flash", missing);
            Check(pack.bloodDrop, "blood sprite", missing);
            Check(pack.bulletHole, "bullet hole", missing);
            Check(pack.smoke, "smoke", missing);
            Check(pack.explosion, "explosion", missing);
            Check(pack.softShadow, "soft shadow", missing);

            if (missing.Count == 0)
            {
                Debug.Log(
                    "Cats and Kills 3/4 art validation passed. " +
                    "Minimum production art is ready for scene integration.");
                return;
            }

            Debug.LogWarning(
                "Cats and Kills 3/4 production art is not complete yet.\n" +
                "Missing:\n- " +
                string.Join("\n- ", missing));
        }

        private static void Check(
            Object value,
            string label,
            List<string> missing)
        {
            if (value == null)
                missing.Add(label);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent =
                System.IO.Path.GetDirectoryName(path)?
                    .Replace("\\", "/");

            string leaf =
                System.IO.Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
