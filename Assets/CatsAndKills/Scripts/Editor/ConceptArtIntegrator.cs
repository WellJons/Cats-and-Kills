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
            Texture2D ui = LoadTexture("ui.png");

            Sprite reinforcedDoor = Crop(
                props, "Environment/reinforced_door",
                18, 7, 447, 511, 96f);

            Sprite crateHeavy = Crop(
                props, "Environment/crate_heavy",
                514, 91, 301, 317, 96f);

            Sprite crateLight = Crop(
                props, "Environment/crate_light",
                869, 182, 200, 226, 96f);

            Sprite crateStack = Crop(
                props, "Environment/crate_stack",
                1127, 95, 298, 308, 96f);

            Sprite fuelDrum = Crop(
                props, "Environment/fuel_drum",
                45, 527, 113, 190, 96f);

            Sprite barrelStack = Crop(
                props, "Environment/barrel_stack",
                207, 449, 241, 272, 96f);

            Sprite terminal = Crop(
                props, "Environment/terminal",
                485, 420, 193, 292, 96f);

            Sprite lamp = Crop(
                props, "Environment/lamp",
                724, 435, 108, 259, 96f);

            Sprite pipeCluster = Crop(
                props, "Environment/pipe_cluster",
                869, 443, 223, 277, 96f);

            Sprite fence = Crop(
                props, "Environment/fence",
                1122, 420, 314, 334, 96f);

            Sprite barricade = Crop(
                props, "Environment/barricade",
                248, 736, 314, 210, 96f);

            Sprite cableBundle = Crop(
                props, "Environment/cable_bundle",
                991, 741, 277, 143, 96f);

            Sprite ammoBox = Crop(
                props, "Environment/ammo_box",
                768, 925, 171, 139, 96f);

            Sprite medkitBox = Crop(
                props, "Environment/medkit_box",
                1004, 916, 172, 143, 96f);

            Sprite burningBarrel = Crop(
                props, "Environment/burning_barrel",
                1274, 822, 149, 244, 96f);

            Sprite propagandaPoster = Crop(
                props, "Environment/propaganda_poster",
                10, 712, 183, 351, 96f);

            Sprite debris = Crop(
                props, "Environment/debris",
                607, 709, 335, 196, 96f);

            // Build one large facility floor texture instead of repeating
            // the isometric tile diamonds. This removes the visible grid and
            // preserves the concept palette without obvious repetition.
            Sprite floorIndustrial =
                CreateFacilityFloorTexture(
                    tileset,
                    "Environment/floor_industrial",
                    72,
                    81,
                    89,
                    63,
                    32f,
                    173,
                    new Color32(100, 104, 120, 255));

            Sprite floorOffice =
                CreateFacilityFloorTexture(
                    tileset,
                    "Environment/floor_office",
                    317,
                    81,
                    89,
                    63,
                    32f,
                    241,
                    new Color32(112, 96, 118, 255));

            Sprite wallStraight = Crop(
                tileset, "Environment/wall_straight",
                754, 2, 378, 403, 96f);

            Sprite wallCorner = Crop(
                tileset, "Environment/wall_corner",
                1143, 15, 288, 350, 96f);

            Sprite wallDamaged = Crop(
                tileset, "Environment/wall_damaged",
                1142, 316, 289, 331, 96f);

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

            Sprite uiPortrait = Crop(
                ui, "UI/player_portrait",
                18, 18, 238, 238, 128f);

            Sprite uiGrenadeIcon = Crop(
                ui, "UI/grenade_icon",
                20, 270, 205, 172, 128f);

            Sprite uiMedkitIcon = Crop(
                ui, "UI/medkit_icon",
                225, 270, 205, 172, 128f);

            Sprite uiObjectiveIcon = Crop(
                ui, "UI/objective_icon",
                430, 245, 205, 205, 128f);

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

            Sprite casing = Crop(
                fx, "FX/casing",
                1070, 150, 360, 260, 96f);

            Sprite smoke = Crop(
                fx, "FX/smoke",
                0, 675, 330, 270, 96f);

            Sprite explosion = Crop(
                fx, "FX/explosion",
                0, 790, 500, 296, 96f);

            // Character shadows are better served by the small
            // procedural ellipse than by a cropped ambience atlas cell.
            Sprite softShadow =
                GeneratedArtFactory.Get("soft_shadow");

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
            pack.crateStack = crateStack;
            pack.fuelDrum = fuelDrum;
            pack.barrelStack = barrelStack;
            pack.terminal = terminal;
            pack.fence = fence;
            pack.barricade = barricade;
            pack.pipeCluster = pipeCluster;
            pack.lamp = lamp;
            pack.cableBundle = cableBundle;
            pack.ammoBox = ammoBox;
            pack.medkitBox = medkitBox;
            pack.burningBarrel = burningBarrel;
            pack.debris = debris;
            pack.propagandaPoster = propagandaPoster;

            pack.uiPortrait = uiPortrait;
            pack.uiGrenadeIcon = uiGrenadeIcon;
            pack.uiMedkitIcon = uiMedkitIcon;
            pack.uiObjectiveIcon = uiObjectiveIcon;

            pack.muzzleFlash = muzzleFlash;
            pack.bloodDrop = bloodDrop;
            pack.bulletHole = bulletHole;
            pack.spark = spark;
            pack.casing = casing;
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
            // Rebuild the generated integration every time the concept build
            // is requested. The source atlases are small enough for editor-time
            // processing, and this guarantees fixes to slicing/cropping are
            // reflected immediately instead of reusing stale generated assets.
            IntegrateAll();

            return AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                PackPath);
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

            // The generated concept sheets visually contain seven columns,
            // even though the original generation request asked for eight.
            // Slicing them as 8 columns was cutting every character in half.
            const int sourceColumns = 7;
            const int rowsCount = 5;

            Sprite[][] sourceRows = new Sprite[rowsCount][];

            for (int row = 0; row < rowsCount; row++)
            {
                sourceRows[row] = new Sprite[sourceColumns];

                for (int col = 0; col < sourceColumns; col++)
                {
                    int x0 =
                        Mathf.RoundToInt(
                            col * atlas.width / (float)sourceColumns);

                    int x1 =
                        Mathf.RoundToInt(
                            (col + 1) * atlas.width /
                            (float)sourceColumns);

                    int top0 =
                        Mathf.RoundToInt(
                            row * atlas.height / (float)rowsCount);

                    int top1 =
                        Mathf.RoundToInt(
                            (row + 1) * atlas.height /
                            (float)rowsCount);

                    sourceRows[row][col] =
                        CropCharacterCell(
                            atlas,
                            "Characters/" +
                            id + "/source_" +
                            RowName(row) + "_" +
                            col,
                            x0,
                            top0,
                            x1 - x0,
                            top1 - top0,
                            128f);
                }
            }

            Sprite[][] mapped = new Sprite[rowsCount][];

            for (int row = 0; row < rowsCount; row++)
            {
                Sprite[] src = sourceRows[row];
                Sprite[] dst = new Sprite[8];

                // Actual generated layout:
                // 0 E, 1 SE, 2 N, 3 NW, 4 W, 5 S, 6 SW.
                // NE is the only missing view; runtime mirrors the NW frame.
                dst[(int)CharacterDirection8.East] = src[0];
                dst[(int)CharacterDirection8.NorthEast] = src[3];
                dst[(int)CharacterDirection8.North] = src[2];
                dst[(int)CharacterDirection8.NorthWest] = src[3];
                dst[(int)CharacterDirection8.West] = src[4];
                dst[(int)CharacterDirection8.SouthWest] = src[6];
                dst[(int)CharacterDirection8.South] = src[5];
                dst[(int)CharacterDirection8.SouthEast] = src[1];

                mapped[row] = dst;
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
                mapped[0],
                mapped[1],
                mapped[2],
                mapped[3],
                mapped[0],
                mapped[4],
                mapped[1],
                mapped[4],
                true);

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

        private static Sprite CropCharacterCell(
            Texture2D source,
            string relativePath,
            int nominalX,
            int nominalTop,
            int nominalWidth,
            int nominalHeight,
            float ppu)
        {
            if (source == null)
                return null;

            int padX =
                Mathf.RoundToInt(
                    nominalWidth * 0.10f);

            int padY =
                Mathf.RoundToInt(
                    nominalHeight * 0.08f);

            int cropWidth =
                Mathf.Min(
                    source.width,
                    nominalWidth +
                    padX * 2);

            int cropHeight =
                Mathf.Min(
                    source.height,
                    nominalHeight +
                    padY * 2);

            float centerX =
                nominalX +
                nominalWidth * 0.5f;

            float centerTop =
                nominalTop +
                nominalHeight * 0.5f;

            int cropX =
                Mathf.RoundToInt(
                    centerX -
                    cropWidth * 0.5f);

            int cropTop =
                Mathf.RoundToInt(
                    centerTop -
                    cropHeight * 0.5f);

            cropX =
                Mathf.Clamp(
                    cropX,
                    0,
                    source.width -
                    cropWidth);

            cropTop =
                Mathf.Clamp(
                    cropTop,
                    0,
                    source.height -
                    cropHeight);

            float nominalPivotX =
                nominalX +
                nominalWidth * 0.5f;

            float nominalBottom =
                source.height -
                nominalTop -
                nominalHeight;

            float nominalPivotY =
                nominalBottom +
                nominalHeight * 0.075f;

            float cropBottom =
                source.height -
                cropTop -
                cropHeight;

            Vector2 pivot =
                new Vector2(
                    Mathf.Clamp01(
                        (nominalPivotX -
                         cropX) /
                        Mathf.Max(
                            1f,
                            cropWidth)),
                    Mathf.Clamp01(
                        (nominalPivotY -
                         cropBottom) /
                        Mathf.Max(
                            1f,
                            cropHeight)));

            return Crop(
                source,
                relativePath,
                cropX,
                cropTop,
                cropWidth,
                cropHeight,
                ppu,
                pivot);
        }

        private static Sprite CreateFacilityFloorTexture(
            Texture2D source,
            string relativePath,
            int sampleX,
            int sampleTopY,
            int sampleWidth,
            int sampleHeight,
            float ppu,
            int seed,
            Color32 tint)
        {
            if (source == null)
                return null;

            const int outW = 1536;
            const int outH = 896;

            int sampleY =
                Mathf.Clamp(
                    source.height -
                    sampleTopY -
                    sampleHeight,
                    0,
                    source.height - sampleHeight);

            Color32[] sourcePixels =
                source.GetPixels32();

            long sumR = 0;
            long sumG = 0;
            long sumB = 0;
            int count = 0;

            for (int yy = 0; yy < sampleHeight; yy++)
            {
                for (int xx = 0; xx < sampleWidth; xx++)
                {
                    int sx =
                        Mathf.Clamp(
                            sampleX + xx,
                            0,
                            source.width - 1);

                    int sy =
                        Mathf.Clamp(
                            sampleY + yy,
                            0,
                            source.height - 1);

                    Color32 px =
                        sourcePixels[
                            sy * source.width +
                            sx];

                    if (px.a < 160)
                        continue;

                    sumR += px.r;
                    sumG += px.g;
                    sumB += px.b;
                    count++;
                }
            }

            Color32 average =
                count > 0
                    ? new Color32(
                        (byte)(sumR / count),
                        (byte)(sumG / count),
                        (byte)(sumB / count),
                        255)
                    : new Color32(48, 52, 76, 255);

            float tintR = tint.r / 100f;
            float tintG = tint.g / 100f;
            float tintB = tint.b / 100f;

            Color32[] pixels =
                new Color32[outW * outH];

            for (int y = 0; y < outH; y++)
            {
                float lowY =
                    Mathf.Sin(
                        y * 0.0105f +
                        seed * 0.17f) *
                    2.5f;

                for (int x = 0; x < outW; x++)
                {
                    unchecked
                    {
                        int hash =
                            x * 73856093 ^
                            y * 19349663 ^
                            seed * 83492791;

                        float grain =
                            ((hash & 255) - 127) /
                            127f;

                        float low =
                            Mathf.Sin(
                                x * 0.0065f +
                                y * 0.0042f +
                                seed * 0.7f) *
                            3.6f +
                            lowY;

                        float broad =
                            Mathf.Sin(
                                x * 0.0018f -
                                y * 0.0022f +
                                seed * 0.31f) *
                            4.4f;

                        int r =
                            Mathf.RoundToInt(
                                average.r * tintR +
                                grain * 2.6f +
                                low +
                                broad);

                        int g =
                            Mathf.RoundToInt(
                                average.g * tintG +
                                grain * 3.0f +
                                low +
                                broad);

                        int b =
                            Mathf.RoundToInt(
                                average.b * tintB +
                                grain * 3.8f +
                                low * 1.10f +
                                broad * 1.18f);

                        pixels[y * outW + x] =
                            new Color32(
                                (byte)Mathf.Clamp(r, 0, 255),
                                (byte)Mathf.Clamp(g, 0, 255),
                                (byte)Mathf.Clamp(b, 0, 255),
                                255);
                    }
                }
            }

            var random =
                new System.Random(seed);

            // Soft, irregular industrial wear. No repeating panel grid.
            for (int i = 0; i < 26; i++)
            {
                int cx = random.Next(40, outW - 40);
                int cy = random.Next(40, outH - 40);
                int rx = random.Next(24, 110);
                int ry = random.Next(10, 48);

                DarkenFloorEllipse(
                    pixels,
                    outW,
                    outH,
                    cx,
                    cy,
                    rx,
                    ry,
                    random.Next(3, 10));
            }

            // Sparse short scratches only.
            for (int i = 0; i < 34; i++)
            {
                int x0 = random.Next(20, outW - 80);
                int y0 = random.Next(20, outH - 40);
                int len = random.Next(12, 58);
                int rise = random.Next(-5, 6);

                DrawFloorLine(
                    pixels,
                    outW,
                    outH,
                    x0,
                    y0,
                    x0 + len,
                    y0 + rise,
                    new Color32(
                        26,
                        29,
                        42,
                        (byte)random.Next(45, 95)),
                    0);
            }

            // Rare tiny bolts/details, deliberately non-periodic.
            for (int i = 0; i < 22; i++)
            {
                int x = random.Next(14, outW - 14);
                int y = random.Next(14, outH - 14);

                DrawFloorDot(
                    pixels,
                    outW,
                    outH,
                    x,
                    y,
                    new Color32(
                        83,
                        70,
                        96,
                        110));
            }

            Texture2D output =
                new Texture2D(
                    outW,
                    outH,
                    TextureFormat.RGBA32,
                    false);

            output.SetPixels32(pixels);
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
                new Vector2(0.5f, 0.5f));

            return AssetDatabase.LoadAssetAtPath<Sprite>(
                path);
        }

        private static void DrawFloorLine(
            Color32[] pixels,
            int width,
            int height,
            int x0,
            int y0,
            int x1,
            int y1,
            Color32 color,
            int thickness)
        {
            int dx = Mathf.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                for (int oy = -thickness; oy <= thickness; oy++)
                {
                    for (int ox = -thickness; ox <= thickness; ox++)
                    {
                        int x = x0 + ox;
                        int y = y0 + oy;

                        if (x < 0 ||
                            y < 0 ||
                            x >= width ||
                            y >= height)
                        {
                            continue;
                        }

                        Color32 current =
                            pixels[y * width + x];

                        pixels[y * width + x] =
                            BlendFloor(
                                current,
                                color);
                    }
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * error;

                if (e2 >= dy)
                {
                    error += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private static void DrawFloorDot(
            Color32[] pixels,
            int width,
            int height,
            int cx,
            int cy,
            Color32 color)
        {
            for (int y = cy - 2; y <= cy + 2; y++)
            {
                for (int x = cx - 2; x <= cx + 2; x++)
                {
                    if (x < 0 ||
                        y < 0 ||
                        x >= width ||
                        y >= height)
                    {
                        continue;
                    }

                    pixels[y * width + x] =
                        BlendFloor(
                            pixels[y * width + x],
                            color);
                }
            }
        }

        private static void DarkenFloorEllipse(
            Color32[] pixels,
            int width,
            int height,
            int cx,
            int cy,
            int rx,
            int ry,
            int amount)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (x < 0 ||
                        y < 0 ||
                        x >= width ||
                        y >= height)
                    {
                        continue;
                    }

                    float nx =
                        (x - cx) /
                        (float)Mathf.Max(1, rx);

                    float ny =
                        (y - cy) /
                        (float)Mathf.Max(1, ry);

                    float d =
                        nx * nx +
                        ny * ny;

                    if (d > 1f)
                        continue;

                    float strength =
                        (1f - d) *
                        amount;

                    Color32 px =
                        pixels[y * width + x];

                    pixels[y * width + x] =
                        new Color32(
                            (byte)Mathf.Max(
                                0,
                                px.r - strength),
                            (byte)Mathf.Max(
                                0,
                                px.g - strength),
                            (byte)Mathf.Max(
                                0,
                                px.b - strength),
                            255);
                }
            }
        }

        private static Color32 BlendFloor(
            Color32 baseColor,
            Color32 overlay)
        {
            float a =
                overlay.a /
                255f;

            return new Color32(
                (byte)Mathf.RoundToInt(
                    Mathf.Lerp(
                        baseColor.r,
                        overlay.r,
                        a)),
                (byte)Mathf.RoundToInt(
                    Mathf.Lerp(
                        baseColor.g,
                        overlay.g,
                        a)),
                (byte)Mathf.RoundToInt(
                    Mathf.Lerp(
                        baseColor.b,
                        overlay.b,
                        a)),
                255);
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
            Vector2? pivot = null,
            Color32? transparentFill = null)
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

            if (transparentFill.HasValue)
            {
                Color32 fill = transparentFill.Value;

                for (int i = 0; i < crop.Length; i++)
                {
                    Color32 px = crop[i];
                    float alpha = px.a / 255f;

                    crop[i] = new Color32(
                        (byte)Mathf.RoundToInt(
                            px.r * alpha +
                            fill.r * (1f - alpha)),
                        (byte)Mathf.RoundToInt(
                            px.g * alpha +
                            fill.g * (1f - alpha)),
                        (byte)Mathf.RoundToInt(
                            px.b * alpha +
                            fill.b * (1f - alpha)),
                        255);
                }
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
            importer.filterMode = FilterMode.Bilinear;
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
