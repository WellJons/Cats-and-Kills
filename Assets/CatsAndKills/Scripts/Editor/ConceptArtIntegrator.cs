#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using CatsAndKills.Visual;
using UnityEditor;
using UnityEngine;

namespace CatsAndKills.EditorTools
{
    public static class ConceptArtIntegrator
    {
        public const string AtlasRoot =
            "Assets/CatsAndKills/Art/ConceptAtlases";

        public const string GeneratedRoot =
            "Assets/CatsAndKills/Generated/IntegratedConcept";

        public const string PackPath =
            GeneratedRoot + "/ConceptProductionArtPack.asset";

        private static readonly string[] RequiredAtlases =
        {
            "player.png",
            "pistolier.png",
            "rifleman.png",
            "machinegunner.png",
            "demolitionist.png",
            "props.png",
            "weapons.png",
            "tileset.png",
            "fx.png",
            "ui.png",
            "ambience.png"
        };

        [MenuItem("Tools/Cats and Kills/Concept Art/Integrate All Generated Atlases")]
        public static void IntegrateAll()
        {
            if (!ValidateAtlases())
                return;

            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedRoot + "/Characters");
            EnsureFolder(GeneratedRoot + "/Environment");
            EnsureFolder(GeneratedRoot + "/Weapons");
            EnsureFolder(GeneratedRoot + "/FX");
            EnsureFolder(GeneratedRoot + "/UI");
            EnsureFolder(GeneratedRoot + "/Ambience");
            EnsureFolder(GeneratedRoot + "/Data");

            foreach (string file in RequiredAtlases)
                ConfigureSourceAtlas(AtlasRoot + "/" + file);

            DirectionalSpriteSet player =
                BuildCharacterSet("player");

            DirectionalSpriteSet pistolier =
                BuildCharacterSet("pistolier");

            DirectionalSpriteSet rifleman =
                BuildCharacterSet("rifleman");

            DirectionalSpriteSet machineGunner =
                BuildCharacterSet("machinegunner");

            DirectionalSpriteSet demolitionist =
                BuildCharacterSet("demolitionist");

            Texture2D props = LoadTexture("props.png");
            Texture2D weapons = LoadTexture("weapons.png");
            Texture2D tileset = LoadTexture("tileset.png");
            Texture2D fx = LoadTexture("fx.png");

            Sprite reinforcedDoor = Crop(
                props, "Environment/reinforced_door",
                0, 0, 465, 470, 96f);

            Sprite crateHeavy = Crop(
                props, "Environment/crate_heavy",
                465, 20, 390, 365, 96f);

            Sprite crateLight = Crop(
                props, "Environment/crate_light",
                820, 55, 300, 320, 96f);

            Sprite fuelDrum = Crop(
                props, "Environment/fuel_drum",
                0, 400, 190, 315, 96f);

            Sprite terminal = Crop(
                props, "Environment/terminal",
                450, 385, 245, 330, 96f);

            Sprite lamp = Crop(
                props, "Environment/lamp",
                670, 385, 190, 330, 96f);

            Sprite pipeCluster = Crop(
                props, "Environment/pipe_cluster",
                840, 390, 250, 320, 96f);

            Sprite fence = Crop(
                props, "Environment/fence",
                1080, 380, 368, 350, 96f);

            Sprite propagandaPoster = Crop(
                props, "Environment/propaganda_poster",
                0, 690, 205, 396, 96f);

            Sprite debris = Crop(
                props, "Environment/debris",
                555, 675, 325, 270, 96f);

            Sprite floorIndustrial = Crop(
                tileset, "Environment/floor_industrial",
                0, 0, 245, 190, 96f);

            Sprite floorOffice = Crop(
                tileset, "Environment/floor_office",
                245, 0, 240, 190, 96f);

            Sprite wallStraight = Crop(
                tileset, "Environment/wall_straight",
                710, 0, 390, 395, 96f);

            Sprite wallCorner = Crop(
                tileset, "Environment/wall_corner",
                1090, 0, 358, 395, 96f);

            Sprite wallDamaged = Crop(
                tileset, "Environment/wall_damaged",
                1080, 330, 368, 350, 96f);

            Sprite rifle = Crop(
                weapons, "Weapons/rifle",
                0, 0, 455, 230, 110f);

            Sprite pistol = Crop(
                weapons, "Weapons/pistol",
                0, 215, 250, 260, 110f);

            Sprite shotgun = Crop(
                weapons, "Weapons/shotgun",
                0, 455, 490, 235, 110f);

            Sprite machineGun = Crop(
                weapons, "Weapons/machinegun",
                900, 210, 548, 300, 110f);

            Sprite grenade = Crop(
                weapons, "Weapons/grenade",
                0, 650, 220, 260, 110f);

            Sprite muzzleFlash = Crop(
                fx, "FX/muzzle_flash",
                0, 0, 175, 150, 96f);

            Sprite bloodDrop = Crop(
                fx, "FX/blood",
                0, 485, 260, 180, 96f);

            Sprite bulletHole = Crop(
                fx, "FX/bullet_hole",
                360, 340, 170, 150, 96f);

            Sprite spark = Crop(
                fx, "FX/spark",
                0, 300, 240, 175, 96f);

            Sprite smoke = Crop(
                fx, "FX/smoke",
                0, 675, 330, 270, 96f);

            Sprite explosion = Crop(
                fx, "FX/explosion",
                0, 790, 500, 296, 96f);

            Sprite softShadow =
                CropAmbience("soft_shadow", 0, 0, 260, 120, 96f);

            ProductionArtPack pack =
                AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                    PackPath);

            if (pack == null)
            {
                pack =
                    ScriptableObject.CreateInstance<ProductionArtPack>();

                AssetDatabase.CreateAsset(pack, PackPath);
            }

            pack.player = player;
            pack.pistolier = pistolier;
            pack.rifleman = rifleman;
            pack.machineGunner = machineGunner;
            pack.demolitionist = demolitionist;

            pack.rifle = rifle;
            pack.pistol = pistol;
            pack.shotgun = shotgun;
            pack.machineGun = machineGun;
            pack.grenade = grenade;

            pack.floorIndustrial = floorIndustrial;
            pack.floorOffice = floorOffice;
            pack.wallStraight = wallStraight;
            pack.wallCorner = wallCorner;
            pack.wallDamaged = wallDamaged;
            pack.reinforcedDoor = reinforcedDoor;
            pack.crateLight = crateLight;
            pack.crateHeavy = crateHeavy;
            pack.fuelDrum = fuelDrum;
            pack.terminal = terminal;
            pack.fence = fence;
            pack.pipeCluster = pipeCluster;
            pack.lamp = lamp;
            pack.debris = debris;
            pack.propagandaPoster = propagandaPoster;

            pack.muzzleFlash = muzzleFlash;
            pack.bloodDrop = bloodDrop;
            pack.bulletHole = bulletHole;
            pack.spark = spark;
            pack.smoke = smoke;
            pack.explosion = explosion;
            pack.softShadow = softShadow;

            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Cats and Kills: generated concept atlases were " +
                "cropped, sliced and integrated into a ProductionArtPack.");
        }

        public static ProductionArtPack EnsureIntegratedPack()
        {
            ProductionArtPack pack =
                AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                    PackPath);

            if (pack == null || !pack.HasMinimumPlayableArt)
            {
                IntegrateAll();

                pack =
                    AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                        PackPath);
            }

            return pack;
        }

        public static Sprite GetAmbienceSprite(
            string id,
            int x,
            int y,
            int width,
            int height,
            float ppu = 96f)
        {
            string path =
                GeneratedRoot + "/Ambience/" + id + ".png";

            Sprite existing =
                AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (existing != null)
                return existing;

            return CropAmbience(
                id,
                x,
                y,
                width,
                height,
                ppu);
        }

        private static DirectionalSpriteSet BuildCharacterSet(
            string id)
        {
            Texture2D atlas =
                LoadTexture(id + ".png");

            if (atlas == null)
                return null;

            Sprite[][] rows = new Sprite[5][];

            for (int row = 0; row < 5; row++)
            {
                rows[row] = new Sprite[8];

                for (int col = 0; col < 8; col++)
                {
                    int x0 =
                        Mathf.RoundToInt(
                            col * atlas.width / 8f);

                    int x1 =
                        Mathf.RoundToInt(
                            (col + 1) * atlas.width / 8f);

                    int top0 =
                        Mathf.RoundToInt(
                            row * atlas.height / 5f);

                    int top1 =
                        Mathf.RoundToInt(
                            (row + 1) * atlas.height / 5f);

                    rows[row][col] =
                        Crop(
                            atlas,
                            "Characters/" +
                            id + "/" +
                            id + "_" +
                            RowName(row) + "_" +
                            col,
                            x0,
                            top0,
                            x1 - x0,
                            top1 - top0,
                            128f,
                            new Vector2(0.5f, 0.08f));
                }
            }

            string dataPath =
                GeneratedRoot +
                "/Data/" +
                id +
                "_DirectionalSet.asset";

            DirectionalSpriteSet set =
                AssetDatabase.LoadAssetAtPath<DirectionalSpriteSet>(
                    dataPath);

            if (set == null)
            {
                set =
                    ScriptableObject.CreateInstance<
                        DirectionalSpriteSet>();

                AssetDatabase.CreateAsset(
                    set,
                    dataPath);
            }

            set.ConfigureExtended(
                rows[0],
                rows[1],
                rows[2],
                rows[3],
                rows[0],
                rows[4],
                rows[1],
                rows[4]);

            EditorUtility.SetDirty(set);

            return set;
        }

        private static string RowName(int row)
        {
            switch (row)
            {
                case 0: return "idle";
                case 1: return "move_a";
                case 2: return "move_b";
                case 3: return "fire";
                default: return "hurt";
            }
        }

        private static Sprite CropAmbience(
            string id,
            int x,
            int y,
            int width,
            int height,
            float ppu)
        {
            Texture2D ambience =
                LoadTexture("ambience.png");

            return Crop(
                ambience,
                "Ambience/" + id,
                x,
                y,
                width,
                height,
                ppu);
        }

        private static Sprite Crop(
            Texture2D source,
            string relativePath,
            int x,
            int topY,
            int width,
            int height,
            float ppu,
            Vector2? pivot = null)
        {
            if (source == null)
                return null;

            int clampedX =
                Mathf.Clamp(x, 0, source.width - 1);

            int clampedTop =
                Mathf.Clamp(topY, 0, source.height - 1);

            int clampedW =
                Mathf.Clamp(
                    width,
                    1,
                    source.width - clampedX);

            int clampedH =
                Mathf.Clamp(
                    height,
                    1,
                    source.height - clampedTop);

            int sourceY =
                source.height -
                clampedTop -
                clampedH;

            sourceY =
                Mathf.Clamp(
                    sourceY,
                    0,
                    source.height - clampedH);

            Color32[] pixels =
                source.GetPixels32(
                    0);

            Color32[] crop =
                new Color32[
                    clampedW *
                    clampedH];

            for (int yy = 0; yy < clampedH; yy++)
            {
                int srcRow =
                    (sourceY + yy) *
                    source.width +
                    clampedX;

                int dstRow =
                    yy * clampedW;

                Array.Copy(
                    pixels,
                    srcRow,
                    crop,
                    dstRow,
                    clampedW);
            }

            Texture2D output =
                new Texture2D(
                    clampedW,
                    clampedH,
                    TextureFormat.RGBA32,
                    false);

            output.SetPixels32(crop);
            output.Apply();

            string path =
                GeneratedRoot +
                "/" +
                relativePath +
                ".png";

            string folder =
                Path.GetDirectoryName(path)?
                    .Replace("\\", "/");

            EnsureFolder(folder);

            File.WriteAllBytes(
                path,
                output.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(output);

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);

            ConfigureGeneratedSprite(
                path,
                ppu,
                pivot ?? new Vector2(0.5f, 0.5f));

            return AssetDatabase.LoadAssetAtPath<Sprite>(
                path);
        }

        private static void ConfigureSourceAtlas(
            string path)
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                return;

            importer.textureType =
                TextureImporterType.Default;

            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;

            importer.SaveAndReimport();
        }

        private static void ConfigureGeneratedSprite(
            string path,
            float ppu,
            Vector2 pivot)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
                return;

            importer.textureType =
                TextureImporterType.Sprite;

            importer.spriteImportMode =
                SpriteImportMode.Single;

            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;

            var settings =
                new TextureImporterSettings();

            importer.ReadTextureSettings(settings);
            settings.spriteMeshType =
                SpriteMeshType.FullRect;

            settings.spriteAlignment =
                (int)SpriteAlignment.Custom;

            settings.spritePivot = pivot;

            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Texture2D LoadTexture(
            string file)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                AtlasRoot + "/" + file);
        }

        private static bool ValidateAtlases()
        {
            var missing = new List<string>();

            foreach (string file in RequiredAtlases)
            {
                string path =
                    AtlasRoot + "/" + file;

                if (!File.Exists(path))
                    missing.Add(path);
            }

            if (missing.Count == 0)
                return true;

            Debug.LogError(
                "Concept atlases are missing. Extract the generated " +
                "art archive into the repository root. Missing:\n- " +
                string.Join("\n- ", missing));

            return false;
        }

        private static void EnsureFolder(
            string path)
        {
            if (string.IsNullOrEmpty(path) ||
                AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent =
                Path.GetDirectoryName(path)?
                    .Replace("\\", "/");

            string leaf =
                Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(
                parent,
                leaf);
        }
    }
}
#endif
