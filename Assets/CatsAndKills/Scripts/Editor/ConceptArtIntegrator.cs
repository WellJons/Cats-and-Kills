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

        private static readonly Dictionary<Texture2D, List<AlphaComponentInfo>>
            AlphaComponentCache =
                new Dictionary<Texture2D, List<AlphaComponentInfo>>();

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

            AlphaComponentCache.Clear();

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

            Sprite reinforcedDoor = CropConnectedAsset(
                props, "Environment/reinforced_door",
                18, 7, 447, 511, 96f);

            Sprite crateHeavy = CropConnectedAsset(
                props, "Environment/crate_heavy",
                514, 91, 301, 317, 96f);

            Sprite crateLight = CropConnectedAsset(
                props, "Environment/crate_light",
                869, 182, 200, 226, 96f);

            Sprite crateStack = CropConnectedAsset(
                props, "Environment/crate_stack",
                1127, 95, 298, 308, 96f);

            Sprite fuelDrum = CropConnectedAsset(
                props, "Environment/fuel_drum",
                45, 527, 113, 190, 96f);

            Sprite barrelStack = CropConnectedAsset(
                props, "Environment/barrel_stack",
                207, 449, 241, 272, 96f);

            Sprite terminal = CropConnectedAsset(
                props, "Environment/terminal",
                485, 420, 193, 292, 96f);

            Sprite lamp = CropConnectedAsset(
                props, "Environment/lamp",
                724, 435, 108, 259, 96f);

            Sprite pipeCluster = CropConnectedAsset(
                props, "Environment/pipe_cluster",
                869, 443, 223, 277, 96f);

            Sprite fence = CropConnectedAsset(
                props, "Environment/fence",
                1122, 420, 314, 334, 96f);

            Sprite barricade = CropConnectedAsset(
                props, "Environment/barricade",
                248, 736, 314, 210, 96f);

            Sprite cableBundle = CropConnectedAsset(
                props, "Environment/cable_bundle",
                991, 741, 277, 143, 96f);

            Sprite ammoBox = CropConnectedAsset(
                props, "Environment/ammo_box",
                768, 925, 171, 139, 96f);

            Sprite medkitBox = CropConnectedAsset(
                props, "Environment/medkit_box",
                1004, 916, 172, 143, 96f);

            Sprite burningBarrel = CropConnectedAsset(
                props, "Environment/burning_barrel",
                1274, 822, 149, 244, 96f);

            Sprite propagandaPoster = CropConnectedAsset(
                props, "Environment/propaganda_poster",
                10, 712, 183, 351, 96f);

            Sprite debris = CropConnectedAsset(
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

            Sprite wallStraight = CropConnectedAsset(
                tileset, "Environment/wall_straight",
                754, 2, 378, 403, 96f);

            Sprite wallCorner = CropConnectedAsset(
                tileset, "Environment/wall_corner",
                1143, 15, 288, 350, 96f);

            Sprite wallDamaged = CropConnectedAsset(
                tileset, "Environment/wall_damaged",
                1142, 316, 289, 331, 96f);

            Sprite rifle = CropConnectedAsset(
                weapons, "Weapons/rifle",
                0, 0, 455, 230, 110f);

            Sprite pistol = CropConnectedAsset(
                weapons, "Weapons/pistol",
                0, 215, 250, 260, 110f);

            Sprite shotgun = CropConnectedAsset(
                weapons, "Weapons/shotgun",
                0, 455, 490, 235, 110f);

            Sprite machineGun = CropConnectedAsset(
                weapons, "Weapons/machinegun",
                900, 210, 548, 300, 110f);

            Sprite grenade = CropConnectedAsset(
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

            Sprite muzzleFlash = CropConnectedAsset(
                fx, "FX/muzzle_flash",
                0, 0, 175, 150, 96f);

            Sprite bloodDrop = CropConnectedAsset(
                fx, "FX/blood",
                0, 485, 260, 180, 96f);

            Sprite bulletHole = CropConnectedAsset(
                fx, "FX/bullet_hole",
                360, 340, 170, 150, 96f);

            Sprite spark = CropConnectedAsset(
                fx, "FX/spark",
                0, 300, 240, 175, 96f);

            Sprite casing = CropConnectedAsset(
                fx, "FX/casing",
                1070, 150, 360, 260, 96f);

            Sprite smoke = CropConnectedAsset(
                fx, "FX/smoke",
                0, 675, 330, 270, 96f);

            Sprite explosion = CropConnectedAsset(
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

        private sealed class AlphaComponentInfo
        {
            public readonly List<int> pixels =
                new List<int>(1024);

            public int minX = int.MaxValue;
            public int minY = int.MaxValue;
            public int maxX = int.MinValue;
            public int maxY = int.MinValue;
            public long sumX;
            public long sumY;

            public int Count => pixels.Count;

            public float CenterX =>
                Count > 0
                    ? sumX / (float)Count
                    : 0f;

            public float CenterY =>
                Count > 0
                    ? sumY / (float)Count
                    : 0f;

            public void Add(
                int index,
                int x,
                int y)
            {
                pixels.Add(index);
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
                sumX += x;
                sumY += y;
            }
        }

        private static DirectionalSpriteSet BuildCharacterSet(
            string id)
        {
            Texture2D atlas =
                LoadTexture(id + ".png");

            if (atlas == null)
                return null;

            const int sourceColumns = 7;
            const int rowsCount = 5;

            List<AlphaComponentInfo> components =
                FindAlphaComponents(
                    atlas,
                    18);

            var cells =
                new List<AlphaComponentInfo>[
                    sourceColumns *
                    rowsCount];

            for (int i = 0; i < cells.Length; i++)
                cells[i] =
                    new List<AlphaComponentInfo>();

            float cellW =
                atlas.width /
                (float)sourceColumns;

            float cellH =
                atlas.height /
                (float)rowsCount;

            foreach (AlphaComponentInfo component
                     in components)
            {
                if (component.Count < 6)
                    continue;

                float topCenter =
                    atlas.height -
                    component.CenterY;

                int col =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            component.CenterX /
                            cellW -
                            0.5f),
                        0,
                        sourceColumns - 1);

                int row =
                    Mathf.Clamp(
                        Mathf.RoundToInt(
                            topCenter /
                            cellH -
                            0.5f),
                        0,
                        rowsCount - 1);

                cells[
                    row *
                    sourceColumns +
                    col].Add(component);
            }

            var selected =
                new List<AlphaComponentInfo>[
                    sourceColumns *
                    rowsCount];

            float maxLeft = 0f;
            float maxRight = 0f;
            float maxDown = 0f;
            float maxUp = 0f;

            for (int row = 0;
                 row < rowsCount;
                 row++)
            {
                for (int col = 0;
                     col < sourceColumns;
                     col++)
                {
                    int index =
                        row *
                        sourceColumns +
                        col;

                    selected[index] =
                        SelectPoseComponents(
                            cells[index],
                            cellW,
                            cellH);

                    float anchorX =
                        (col + 0.5f) *
                        cellW;

                    float cellBottom =
                        atlas.height -
                        (row + 1f) *
                        cellH;

                    float anchorY =
                        cellBottom +
                        cellH *
                        0.075f;

                    if (selected[index].Count == 0)
                    {
                        int x0 =
                            Mathf.RoundToInt(
                                col * cellW);

                        int x1 =
                            Mathf.RoundToInt(
                                (col + 1) * cellW) -
                            1;

                        int y0 =
                            Mathf.RoundToInt(
                                cellBottom);

                        int y1 =
                            Mathf.RoundToInt(
                                cellBottom +
                                cellH) -
                            1;

                        maxLeft =
                            Mathf.Max(
                                maxLeft,
                                anchorX - x0);

                        maxRight =
                            Mathf.Max(
                                maxRight,
                                x1 - anchorX);

                        maxDown =
                            Mathf.Max(
                                maxDown,
                                anchorY - y0);

                        maxUp =
                            Mathf.Max(
                                maxUp,
                                y1 - anchorY);

                        continue;
                    }

                    foreach (AlphaComponentInfo component
                             in selected[index])
                    {
                        maxLeft =
                            Mathf.Max(
                                maxLeft,
                                anchorX -
                                component.minX);

                        maxRight =
                            Mathf.Max(
                                maxRight,
                                component.maxX -
                                anchorX);

                        maxDown =
                            Mathf.Max(
                                maxDown,
                                anchorY -
                                component.minY);

                        maxUp =
                            Mathf.Max(
                                maxUp,
                                component.maxY -
                                anchorY);
                    }
                }
            }

            int paddingX =
                Mathf.Max(
                    10,
                    Mathf.RoundToInt(
                        cellW *
                        0.05f));

            int paddingY =
                Mathf.Max(
                    10,
                    Mathf.RoundToInt(
                        cellH *
                        0.05f));

            int canvasW =
                Mathf.CeilToInt(
                    maxLeft +
                    maxRight) +
                paddingX * 2 +
                2;

            int canvasH =
                Mathf.CeilToInt(
                    maxDown +
                    maxUp) +
                paddingY * 2 +
                2;

            int pivotPixelX =
                Mathf.CeilToInt(
                    maxLeft) +
                paddingX;

            int pivotPixelY =
                Mathf.CeilToInt(
                    maxDown) +
                paddingY;

            Vector2 commonPivot =
                new Vector2(
                    pivotPixelX /
                    (float)Mathf.Max(
                        1,
                        canvasW),
                    pivotPixelY /
                    (float)Mathf.Max(
                        1,
                        canvasH));

            Color32[] sourcePixels =
                atlas.GetPixels32(0);

            Sprite[][] sourceRows =
                new Sprite[rowsCount][];

            for (int row = 0;
                 row < rowsCount;
                 row++)
            {
                sourceRows[row] =
                    new Sprite[sourceColumns];

                for (int col = 0;
                     col < sourceColumns;
                     col++)
                {
                    int index =
                        row *
                        sourceColumns +
                        col;

                    float anchorX =
                        (col + 0.5f) *
                        cellW;

                    float cellBottom =
                        atlas.height -
                        (row + 1f) *
                        cellH;

                    float anchorY =
                        cellBottom +
                        cellH *
                        0.075f;

                    Color32[] output =
                        new Color32[
                            canvasW *
                            canvasH];

                    List<AlphaComponentInfo> pose =
                        selected[index];

                    if (pose.Count > 0)
                    {
                        foreach (AlphaComponentInfo component
                                 in pose)
                        {
                            foreach (int sourceIndex
                                     in component.pixels)
                            {
                                int sx =
                                    sourceIndex %
                                    atlas.width;

                                int sy =
                                    sourceIndex /
                                    atlas.width;

                                int dx =
                                    Mathf.RoundToInt(
                                        sx -
                                        anchorX) +
                                    pivotPixelX;

                                int dy =
                                    Mathf.RoundToInt(
                                        sy -
                                        anchorY) +
                                    pivotPixelY;

                                if (dx < 0 ||
                                    dy < 0 ||
                                    dx >= canvasW ||
                                    dy >= canvasH)
                                {
                                    continue;
                                }

                                output[
                                    dy *
                                    canvasW +
                                    dx] =
                                    sourcePixels[
                                        sourceIndex];
                            }
                        }
                    }
                    else
                    {
                        CopyNominalCharacterCell(
                            atlas,
                            sourcePixels,
                            output,
                            canvasW,
                            canvasH,
                            row,
                            col,
                            sourceColumns,
                            rowsCount,
                            pivotPixelX,
                            pivotPixelY);
                    }

                    StripBakedWeaponExtension(
                        output,
                        canvasW,
                        canvasH,
                        pivotPixelX,
                        pivotPixelY,
                        col);

                    sourceRows[row][col] =
                        SaveGeneratedSprite(
                            output,
                            canvasW,
                            canvasH,
                            "Characters/" +
                            id +
                            "/source_" +
                            RowName(row) +
                            "_" +
                            col,
                            128f,
                            commonPivot);
                }
            }

            Sprite[][] mapped =
                new Sprite[rowsCount][];

            for (int row = 0;
                 row < rowsCount;
                 row++)
            {
                Sprite[] src =
                    sourceRows[row];

                Sprite[] dst =
                    new Sprite[8];

                dst[
                    (int)CharacterDirection8.East] =
                    src[0];

                dst[
                    (int)CharacterDirection8.NorthEast] =
                    src[3];

                dst[
                    (int)CharacterDirection8.North] =
                    src[2];

                dst[
                    (int)CharacterDirection8.NorthWest] =
                    src[3];

                dst[
                    (int)CharacterDirection8.West] =
                    src[4];

                dst[
                    (int)CharacterDirection8.SouthWest] =
                    src[6];

                dst[
                    (int)CharacterDirection8.South] =
                    src[5];

                dst[
                    (int)CharacterDirection8.SouthEast] =
                    src[1];

                mapped[row] = dst;
            }

            Sprite[][] walkCycle =
                BuildWalkCycle(
                    id,
                    mapped[0],
                    mapped[1],
                    mapped[2]);

            string dataPath =
                GeneratedRoot +
                "/Data/" +
                id +
                "_DirectionalSet.asset";

            DirectionalSpriteSet set =
                AssetDatabase.LoadAssetAtPath<
                    DirectionalSpriteSet>(
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

            set.ConfigureWalkCycle(
                walkCycle[0],
                walkCycle[1],
                walkCycle[2],
                walkCycle[3],
                walkCycle[4],
                walkCycle[5],
                walkCycle[6],
                walkCycle[7]);

            EditorUtility.SetDirty(set);

            return set;
        }

        private static void StripBakedWeaponExtension(
            Color32[] pixels,
            int width,
            int height,
            int pivotX,
            int pivotY,
            int sourceColumn)
        {
            if (pixels == null ||
                pixels.Length != width * height)
            {
                return;
            }

            Vector2 forward =
                SourceColumnForward(
                    sourceColumn);

            if (forward.sqrMagnitude <
                0.5f)
            {
                return;
            }

            forward.Normalize();

            Vector2 perpendicular =
                new Vector2(
                    -forward.y,
                    forward.x);

            float bodyHeight =
                Mathf.Max(
                    1f,
                    height - pivotY);

            Vector2 aim =
                new Vector2(
                    pivotX,
                    pivotY +
                    bodyHeight * 0.48f);

            float bodyHalfWidth =
                width * 0.18f;

            float bodyMinY =
                pivotY -
                height * 0.02f;

            float bodyMaxY =
                pivotY +
                bodyHeight * 0.82f;

            float corridorHalfWidth =
                Mathf.Max(
                    5f,
                    height * 0.075f);

            float minimumForward =
                Mathf.Max(
                    4f,
                    width * 0.055f);

            float protectLowY =
                pivotY +
                bodyHeight * 0.18f;

            for (int y = 0;
                 y < height;
                 y++)
            {
                if (y < protectLowY)
                    continue;

                for (int x = 0;
                     x < width;
                     x++)
                {
                    int index =
                        y *
                        width +
                        x;

                    if (pixels[index].a <= 18)
                        continue;

                    bool bodyCore =
                        Mathf.Abs(
                            x - pivotX) <=
                        bodyHalfWidth &&
                        y >= bodyMinY &&
                        y <= bodyMaxY;

                    // Never erase the protected torso/head core. This keeps
                    // character bodies intact even when the baked rifle crosses
                    // the chest. Dedicated weaponless source art will replace
                    // this conservative fallback later.
                    if (bodyCore)
                        continue;

                    Vector2 relative =
                        new Vector2(
                            x,
                            y) -
                        aim;

                    float along =
                        Vector2.Dot(
                            relative,
                            forward);

                    if (along <=
                        minimumForward)
                    {
                        continue;
                    }

                    float side =
                        Mathf.Abs(
                            Vector2.Dot(
                                relative,
                                perpendicular));

                    if (side >
                        corridorHalfWidth)
                    {
                        continue;
                    }

                    pixels[index] =
                        new Color32(
                            0,
                            0,
                            0,
                            0);
                }
            }
        }

        private static Vector2 SourceColumnForward(
            int sourceColumn)
        {
            const float diagonal =
                0.70710678f;

            switch (sourceColumn)
            {
                case 0:
                    return Vector2.right;

                case 1:
                    return new Vector2(
                        diagonal,
                        -diagonal);

                case 2:
                    return Vector2.up;

                case 3:
                    return new Vector2(
                        diagonal,
                        diagonal);

                case 4:
                    return Vector2.left;

                case 5:
                    return Vector2.down;

                case 6:
                    return new Vector2(
                        -diagonal,
                        -diagonal);

                default:
                    return Vector2.zero;
            }
        }

        private static Sprite[][] BuildWalkCycle(
            string id,
            Sprite[] idle,
            Sprite[] moveA,
            Sprite[] moveB)
        {
            var frames =
                new Sprite[8][];

            frames[0] =
                idle;

            frames[1] =
                CreateWalkCompositeSet(
                    id,
                    "walk_01",
                    idle,
                    moveA,
                    0.56f,
                    -3,
                    0);

            frames[2] =
                moveA;

            frames[3] =
                CreateWalkCompositeSet(
                    id,
                    "walk_03",
                    moveA,
                    idle,
                    0.48f,
                    2,
                    1);

            frames[4] =
                idle;

            frames[5] =
                CreateWalkCompositeSet(
                    id,
                    "walk_05",
                    idle,
                    moveB,
                    0.56f,
                    3,
                    0);

            frames[6] =
                moveB;

            frames[7] =
                CreateWalkCompositeSet(
                    id,
                    "walk_07",
                    moveB,
                    idle,
                    0.48f,
                    -2,
                    1);

            return frames;
        }

        private static Sprite[] CreateWalkCompositeSet(
            string id,
            string frameId,
            Sprite[] upperSet,
            Sprite[] lowerSet,
            float lowerCutoff01,
            int lowerShiftX,
            int lowerShiftY)
        {
            var result =
                new Sprite[8];

            for (int i = 0;
                 i < result.Length;
                 i++)
            {
                Sprite upper =
                    upperSet != null &&
                    i < upperSet.Length
                        ? upperSet[i]
                        : null;

                Sprite lower =
                    lowerSet != null &&
                    i < lowerSet.Length
                        ? lowerSet[i]
                        : null;

                result[i] =
                    CreateLowerBodyComposite(
                        upper,
                        lower,
                        "Characters/" +
                        id +
                        "/" +
                        frameId +
                        "_" +
                        i,
                        lowerCutoff01,
                        lowerShiftX,
                        lowerShiftY);
            }

            return result;
        }

        private static Sprite CreateLowerBodyComposite(
            Sprite upper,
            Sprite lower,
            string relativePath,
            float lowerCutoff01,
            int lowerShiftX,
            int lowerShiftY)
        {
            if (upper == null)
                return lower;

            if (lower == null)
                return upper;

            Texture2D upperTexture =
                upper.texture;

            Texture2D lowerTexture =
                lower.texture;

            if (upperTexture == null ||
                lowerTexture == null ||
                upperTexture.width != lowerTexture.width ||
                upperTexture.height != lowerTexture.height)
            {
                return lower;
            }

            Color32[] upperPixels =
                upperTexture.GetPixels32(0);

            Color32[] lowerPixels =
                lowerTexture.GetPixels32(0);

            if (upperPixels.Length !=
                lowerPixels.Length)
            {
                return lower;
            }

            int width =
                upperTexture.width;

            int height =
                upperTexture.height;

            Color32[] output =
                new Color32[
                    upperPixels.Length];

            float cutoff =
                Mathf.Clamp01(
                    lowerCutoff01) *
                Mathf.Max(
                    1,
                    height - 1);

            float blendBand =
                Mathf.Max(
                    2f,
                    height *
                    0.035f);

            for (int y = 0;
                 y < height;
                 y++)
            {
                float lowerWeight =
                    1f -
                    Mathf.InverseLerp(
                        cutoff -
                        blendBand,
                        cutoff +
                        blendBand,
                        y);

                for (int x = 0;
                     x < width;
                     x++)
                {
                    int index =
                        y *
                        width +
                        x;

                    Color32 a =
                        upperPixels[index];

                    int lowerX =
                        Mathf.Clamp(
                            x -
                            lowerShiftX,
                            0,
                            width - 1);

                    int lowerY =
                        Mathf.Clamp(
                            y -
                            lowerShiftY,
                            0,
                            height - 1);

                    Color32 b =
                        lowerPixels[
                            lowerY *
                            width +
                            lowerX];

                    output[index] =
                        new Color32(
                            (byte)Mathf.RoundToInt(
                                Mathf.Lerp(
                                    a.r,
                                    b.r,
                                    lowerWeight)),
                            (byte)Mathf.RoundToInt(
                                Mathf.Lerp(
                                    a.g,
                                    b.g,
                                    lowerWeight)),
                            (byte)Mathf.RoundToInt(
                                Mathf.Lerp(
                                    a.b,
                                    b.b,
                                    lowerWeight)),
                            (byte)Mathf.RoundToInt(
                                Mathf.Lerp(
                                    a.a,
                                    b.a,
                                    lowerWeight)));
                }
            }

            Vector2 pivot =
                new Vector2(
                    upper.pivot.x /
                    Mathf.Max(
                        1f,
                        upper.rect.width),
                    upper.pivot.y /
                    Mathf.Max(
                        1f,
                        upper.rect.height));

            return SaveGeneratedSprite(
                output,
                width,
                height,
                relativePath,
                upper.pixelsPerUnit,
                pivot);
        }

        private static List<AlphaComponentInfo>
            SelectPoseComponents(
                List<AlphaComponentInfo> candidates,
                float cellW,
                float cellH)
        {
            var result =
                new List<AlphaComponentInfo>();

            if (candidates == null ||
                candidates.Count == 0)
            {
                return result;
            }

            AlphaComponentInfo main = null;

            foreach (AlphaComponentInfo candidate
                     in candidates)
            {
                if (main == null ||
                    candidate.Count >
                    main.Count)
                {
                    main = candidate;
                }
            }

            if (main == null)
                return result;

            result.Add(main);

            float margin =
                Mathf.Max(
                    8f,
                    Mathf.Min(
                        cellW,
                        cellH) *
                    0.18f);

            foreach (AlphaComponentInfo candidate
                     in candidates)
            {
                if (candidate == main ||
                    candidate.Count < 6)
                {
                    continue;
                }

                float gapX =
                    Mathf.Max(
                        0f,
                        Mathf.Max(
                            main.minX -
                            candidate.maxX,
                            candidate.minX -
                            main.maxX));

                float gapY =
                    Mathf.Max(
                        0f,
                        Mathf.Max(
                            main.minY -
                            candidate.maxY,
                            candidate.minY -
                            main.maxY));

                float distance =
                    Mathf.Sqrt(
                        gapX * gapX +
                        gapY * gapY);

                bool reasonablySized =
                    candidate.Count <=
                    main.Count * 0.55f ||
                    distance <
                    margin * 0.65f;

                if (distance <= margin &&
                    reasonablySized)
                {
                    result.Add(candidate);
                }
            }

            return result;
        }

        private static List<AlphaComponentInfo>
            FindAlphaComponents(
                Texture2D source,
                byte alphaThreshold)
        {
            var result =
                new List<AlphaComponentInfo>();

            if (source == null)
                return result;

            Texture2D cacheKey =
                source;

            if (alphaThreshold == 18 &&
                AlphaComponentCache.TryGetValue(
                    cacheKey,
                    out List<AlphaComponentInfo> cached))
            {
                return cached;
            }

            int width =
                source.width;

            int height =
                source.height;

            Color32[] pixels =
                source.GetPixels32(0);

            bool[] visited =
                new bool[
                    width *
                    height];

            var queue =
                new Queue<int>();

            for (int index = 0;
                 index < pixels.Length;
                 index++)
            {
                if (visited[index] ||
                    pixels[index].a <=
                    alphaThreshold)
                {
                    continue;
                }

                AlphaComponentInfo component =
                    new AlphaComponentInfo();

                queue.Clear();
                queue.Enqueue(index);
                visited[index] = true;

                while (queue.Count > 0)
                {
                    int current =
                        queue.Dequeue();

                    int x =
                        current %
                        width;

                    int y =
                        current /
                        width;

                    component.Add(
                        current,
                        x,
                        y);

                    for (int oy = -1;
                         oy <= 1;
                         oy++)
                    {
                        for (int ox = -1;
                             ox <= 1;
                             ox++)
                        {
                            if (ox == 0 &&
                                oy == 0)
                            {
                                continue;
                            }

                            int nx =
                                x +
                                ox;

                            int ny =
                                y +
                                oy;

                            if (nx < 0 ||
                                ny < 0 ||
                                nx >= width ||
                                ny >= height)
                            {
                                continue;
                            }

                            int ni =
                                ny *
                                width +
                                nx;

                            if (visited[ni] ||
                                pixels[ni].a <=
                                alphaThreshold)
                            {
                                continue;
                            }

                            visited[ni] = true;
                            queue.Enqueue(ni);
                        }
                    }
                }

                if (component.Count >= 3)
                    result.Add(component);
            }

            if (alphaThreshold == 18)
                AlphaComponentCache[cacheKey] = result;

            return result;
        }

        private static void CopyNominalCharacterCell(
            Texture2D atlas,
            Color32[] sourcePixels,
            Color32[] output,
            int canvasW,
            int canvasH,
            int row,
            int col,
            int columns,
            int rows,
            int pivotPixelX,
            int pivotPixelY)
        {
            float cellW =
                atlas.width /
                (float)columns;

            float cellH =
                atlas.height /
                (float)rows;

            int x0 =
                Mathf.RoundToInt(
                    col *
                    cellW);

            int x1 =
                Mathf.RoundToInt(
                    (col + 1) *
                    cellW);

            int top0 =
                Mathf.RoundToInt(
                    row *
                    cellH);

            int top1 =
                Mathf.RoundToInt(
                    (row + 1) *
                    cellH);

            int bottom =
                atlas.height -
                top1;

            float anchorX =
                (col + 0.5f) *
                cellW;

            float anchorY =
                bottom +
                cellH *
                0.075f;

            for (int sy = bottom;
                 sy < atlas.height - top0;
                 sy++)
            {
                for (int sx = x0;
                     sx < x1;
                     sx++)
                {
                    int sourceIndex =
                        sy *
                        atlas.width +
                        sx;

                    if (sourcePixels[
                            sourceIndex].a <=
                        18)
                    {
                        continue;
                    }

                    int dx =
                        Mathf.RoundToInt(
                            sx -
                            anchorX) +
                        pivotPixelX;

                    int dy =
                        Mathf.RoundToInt(
                            sy -
                            anchorY) +
                        pivotPixelY;

                    if (dx < 0 ||
                        dy < 0 ||
                        dx >= canvasW ||
                        dy >= canvasH)
                    {
                        continue;
                    }

                    output[
                        dy *
                        canvasW +
                        dx] =
                        sourcePixels[
                            sourceIndex];
                }
            }
        }

        private static Sprite CropConnectedAsset(
            Texture2D source,
            string relativePath,
            int roughX,
            int roughTopY,
            int roughWidth,
            int roughHeight,
            float ppu)
        {
            if (source == null)
                return null;

            List<AlphaComponentInfo> components =
                FindAlphaComponents(
                    source,
                    18);

            int roughBottom =
                source.height -
                roughTopY -
                roughHeight;

            float centerX =
                roughX +
                roughWidth * 0.5f;

            float centerY =
                roughBottom +
                roughHeight * 0.5f;

            AlphaComponentInfo main = null;
            float bestScore = float.MaxValue;

            foreach (AlphaComponentInfo component
                     in components)
            {
                bool intersects =
                    component.maxX >= roughX &&
                    component.minX <=
                    roughX + roughWidth &&
                    component.maxY >= roughBottom &&
                    component.minY <=
                    roughBottom + roughHeight;

                if (!intersects)
                    continue;

                bool centerInside =
                    component.CenterX >= roughX &&
                    component.CenterX <=
                    roughX + roughWidth &&
                    component.CenterY >= roughBottom &&
                    component.CenterY <=
                    roughBottom + roughHeight;

                float dx =
                    (component.CenterX - centerX) /
                    Mathf.Max(
                        1f,
                        roughWidth);

                float dy =
                    (component.CenterY - centerY) /
                    Mathf.Max(
                        1f,
                        roughHeight);

                float score =
                    dx * dx +
                    dy * dy -
                    Mathf.Log10(
                        Mathf.Max(
                            1,
                            component.Count)) *
                    0.10f;

                if (centerInside)
                    score -= 0.35f;

                if (score < bestScore)
                {
                    bestScore = score;
                    main = component;
                }
            }

            if (main == null)
            {
                Debug.LogWarning(
                    "Could not find opaque component for " +
                    relativePath +
                    "; using rough crop.");

                return Crop(
                    source,
                    relativePath,
                    roughX,
                    roughTopY,
                    roughWidth,
                    roughHeight,
                    ppu);
            }

            var selected =
                new List<AlphaComponentInfo>
                {
                    main
                };

            float margin =
                Mathf.Max(
                    10f,
                    Mathf.Min(
                        roughWidth,
                        roughHeight) *
                    0.12f);

            foreach (AlphaComponentInfo component
                     in components)
            {
                if (component == main ||
                    component.Count < 4)
                {
                    continue;
                }

                bool centerInside =
                    component.CenterX >= roughX &&
                    component.CenterX <=
                    roughX + roughWidth &&
                    component.CenterY >= roughBottom &&
                    component.CenterY <=
                    roughBottom + roughHeight;

                float gapX =
                    Mathf.Max(
                        0f,
                        Mathf.Max(
                            main.minX -
                            component.maxX,
                            component.minX -
                            main.maxX));

                float gapY =
                    Mathf.Max(
                        0f,
                        Mathf.Max(
                            main.minY -
                            component.maxY,
                            component.minY -
                            main.maxY));

                float gap =
                    Mathf.Sqrt(
                        gapX * gapX +
                        gapY * gapY);

                bool closeDetachedDetail =
                    gap <= margin &&
                    component.Count <=
                    main.Count * 0.45f;

                if (centerInside ||
                    closeDetachedDetail)
                {
                    selected.Add(component);
                }
            }

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (AlphaComponentInfo component
                     in selected)
            {
                minX =
                    Mathf.Min(
                        minX,
                        component.minX);

                minY =
                    Mathf.Min(
                        minY,
                        component.minY);

                maxX =
                    Mathf.Max(
                        maxX,
                        component.maxX);

                maxY =
                    Mathf.Max(
                        maxY,
                        component.maxY);
            }

            int pad =
                Mathf.Max(
                    10,
                    Mathf.RoundToInt(
                        Mathf.Min(
                            roughWidth,
                            roughHeight) *
                        0.045f));

            int contentMinX = minX;
            int contentMinY = minY;
            int contentMaxX = maxX;
            int contentMaxY = maxY;

            int width =
                contentMaxX -
                contentMinX +
                1 +
                pad * 2;

            int height =
                contentMaxY -
                contentMinY +
                1 +
                pad * 2;

            Color32[] sourcePixels =
                source.GetPixels32(0);

            Color32[] output =
                new Color32[
                    width *
                    height];

            foreach (AlphaComponentInfo component
                     in selected)
            {
                foreach (int sourceIndex
                         in component.pixels)
                {
                    int sx =
                        sourceIndex %
                        source.width;

                    int sy =
                        sourceIndex /
                        source.width;

                    int dx =
                        sx -
                        contentMinX +
                        pad;

                    int dy =
                        sy -
                        contentMinY +
                        pad;

                    if (dx < 0 ||
                        dy < 0 ||
                        dx >= width ||
                        dy >= height)
                    {
                        continue;
                    }

                    output[
                        dy *
                        width +
                        dx] =
                        sourcePixels[
                            sourceIndex];
                }
            }

            bool groundAnchored =
                relativePath.StartsWith(
                    "Environment/",
                    StringComparison.Ordinal);

            float pivotXWorld =
                groundAnchored
                    ? main.CenterX
                    : centerX;

            float pivotYWorld =
                groundAnchored
                    ? main.minY
                    : centerY;

            Vector2 pivot =
                new Vector2(
                    Mathf.Clamp01(
                        (pivotXWorld -
                         contentMinX +
                         pad) /
                        Mathf.Max(
                            1f,
                            width)),
                    Mathf.Clamp01(
                        (pivotYWorld -
                         contentMinY +
                         pad) /
                        Mathf.Max(
                            1f,
                            height)));

            return SaveGeneratedSprite(
                output,
                width,
                height,
                relativePath,
                ppu,
                pivot);
        }

        private static Sprite SaveGeneratedSprite(
            Color32[] pixels,
            int width,
            int height,
            string relativePath,
            float ppu,
            Vector2 pivot)
        {
            Texture2D output =
                new Texture2D(
                    width,
                    height,
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

            UnityEngine.Object.DestroyImmediate(
                output);

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions
                    .ForceSynchronousImport);

            ConfigureGeneratedSprite(
                path,
                ppu,
                pivot);

            return
                AssetDatabase.LoadAssetAtPath<
                    Sprite>(
                    path);
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
                    nominalWidth * 0.14f);

            int padY =
                Mathf.RoundToInt(
                    nominalHeight * 0.10f);

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
                Mathf.Clamp(
                    Mathf.RoundToInt(
                        centerX -
                        cropWidth * 0.5f),
                    0,
                    source.width -
                    cropWidth);

            int cropTop =
                Mathf.Clamp(
                    Mathf.RoundToInt(
                        centerTop -
                        cropHeight * 0.5f),
                    0,
                    source.height -
                    cropHeight);

            int cropBottom =
                source.height -
                cropTop -
                cropHeight;

            int nominalBottom =
                source.height -
                nominalTop -
                nominalHeight;

            Color32[] sourcePixels =
                source.GetPixels32(0);

            Color32[] output =
                new Color32[
                    cropWidth *
                    cropHeight];

            bool[] visited =
                new bool[
                    cropWidth *
                    cropHeight];

            bool[] keep =
                new bool[
                    cropWidth *
                    cropHeight];

            var queue =
                new Queue<int>();

            var component =
                new List<int>(2048);

            float acceptLeft =
                nominalX -
                nominalWidth * 0.18f;

            float acceptRight =
                nominalX +
                nominalWidth * 1.18f;

            float acceptBottom =
                nominalBottom -
                nominalHeight * 0.18f;

            float acceptTop =
                nominalBottom +
                nominalHeight * 1.18f;

            for (int localY = 0;
                 localY < cropHeight;
                 localY++)
            {
                for (int localX = 0;
                     localX < cropWidth;
                     localX++)
                {
                    int localIndex =
                        localY *
                        cropWidth +
                        localX;

                    if (visited[localIndex])
                        continue;

                    int globalX =
                        cropX +
                        localX;

                    int globalY =
                        cropBottom +
                        localY;

                    Color32 seed =
                        sourcePixels[
                            globalY *
                            source.width +
                            globalX];

                    if (seed.a <= 18)
                    {
                        visited[localIndex] = true;
                        continue;
                    }

                    queue.Clear();
                    component.Clear();

                    queue.Enqueue(localIndex);
                    visited[localIndex] = true;

                    long sumX = 0;
                    long sumY = 0;

                    while (queue.Count > 0)
                    {
                        int index =
                            queue.Dequeue();

                        component.Add(index);

                        int x =
                            index %
                            cropWidth;

                        int y =
                            index /
                            cropWidth;

                        sumX += cropX + x;
                        sumY += cropBottom + y;

                        for (int oy = -1;
                             oy <= 1;
                             oy++)
                        {
                            for (int ox = -1;
                                 ox <= 1;
                                 ox++)
                            {
                                if (ox == 0 &&
                                    oy == 0)
                                {
                                    continue;
                                }

                                int nx = x + ox;
                                int ny = y + oy;

                                if (nx < 0 ||
                                    ny < 0 ||
                                    nx >= cropWidth ||
                                    ny >= cropHeight)
                                {
                                    continue;
                                }

                                int ni =
                                    ny *
                                    cropWidth +
                                    nx;

                                if (visited[ni])
                                    continue;

                                int gx =
                                    cropX +
                                    nx;

                                int gy =
                                    cropBottom +
                                    ny;

                                Color32 px =
                                    sourcePixels[
                                        gy *
                                        source.width +
                                        gx];

                                if (px.a <= 18)
                                    continue;

                                visited[ni] = true;
                                queue.Enqueue(ni);
                            }
                        }
                    }

                    if (component.Count < 4)
                        continue;

                    float centroidX =
                        sumX /
                        (float)component.Count;

                    float centroidY =
                        sumY /
                        (float)component.Count;

                    bool belongsToFrame =
                        centroidX >= acceptLeft &&
                        centroidX <= acceptRight &&
                        centroidY >= acceptBottom &&
                        centroidY <= acceptTop;

                    if (!belongsToFrame)
                        continue;

                    foreach (int index in component)
                        keep[index] = true;
                }
            }

            for (int localY = 0;
                 localY < cropHeight;
                 localY++)
            {
                for (int localX = 0;
                     localX < cropWidth;
                     localX++)
                {
                    int index =
                        localY *
                        cropWidth +
                        localX;

                    if (!keep[index])
                    {
                        output[index] =
                            new Color32(
                                0,
                                0,
                                0,
                                0);

                        continue;
                    }

                    int globalX =
                        cropX +
                        localX;

                    int globalY =
                        cropBottom +
                        localY;

                    output[index] =
                        sourcePixels[
                            globalY *
                            source.width +
                            globalX];
                }
            }

            float nominalPivotX =
                nominalX +
                nominalWidth * 0.5f;

            float nominalPivotY =
                nominalBottom +
                nominalHeight * 0.075f;

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

            Texture2D texture =
                new Texture2D(
                    cropWidth,
                    cropHeight,
                    TextureFormat.RGBA32,
                    false);

            texture.SetPixels32(output);
            texture.Apply();

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
                texture.EncodeToPNG());

            UnityEngine.Object.DestroyImmediate(
                texture);

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);

            ConfigureGeneratedSprite(
                path,
                ppu,
                pivot);

            return
                AssetDatabase.LoadAssetAtPath<Sprite>(
                    path);
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
                    source.height -
                    sampleHeight);

            Color32[] sourcePixels =
                source.GetPixels32(0);

            Color32[] sample =
                new Color32[
                    sampleWidth *
                    sampleHeight];

            long sumR = 0;
            long sumG = 0;
            long sumB = 0;
            int opaqueCount = 0;

            for (int y = 0;
                 y < sampleHeight;
                 y++)
            {
                for (int x = 0;
                     x < sampleWidth;
                     x++)
                {
                    int sx =
                        Mathf.Clamp(
                            sampleX + x,
                            0,
                            source.width - 1);

                    int sy =
                        Mathf.Clamp(
                            sampleY + y,
                            0,
                            source.height - 1);

                    Color32 px =
                        sourcePixels[
                            sy *
                            source.width +
                            sx];

                    sample[
                        y *
                        sampleWidth +
                        x] =
                        px;

                    if (px.a <= 24)
                        continue;

                    sumR += px.r;
                    sumG += px.g;
                    sumB += px.b;
                    opaqueCount++;
                }
            }

            Color32 average =
                opaqueCount > 0
                    ? new Color32(
                        (byte)(sumR / opaqueCount),
                        (byte)(sumG / opaqueCount),
                        (byte)(sumB / opaqueCount),
                        255)
                    : new Color32(
                        44,
                        48,
                        72,
                        255);

            // Generated floor tiles are often diamond-shaped with transparent
            // corners. Fill those corners from nearby real texels instead of
            // throwing the entire tile away and replacing it with flat purple.
            Color32[] filledSample =
                new Color32[
                    sample.Length];

            for (int y = 0;
                 y < sampleHeight;
                 y++)
            {
                for (int x = 0;
                     x < sampleWidth;
                     x++)
                {
                    int index =
                        y *
                        sampleWidth +
                        x;

                    Color32 px =
                        sample[index];

                    if (px.a > 24)
                    {
                        px.a = 255;
                        filledSample[index] = px;
                        continue;
                    }

                    Color32 nearest =
                        average;

                    int bestDistance =
                        int.MaxValue;

                    const int searchRadius = 14;

                    for (int oy = -searchRadius;
                         oy <= searchRadius;
                         oy++)
                    {
                        for (int ox = -searchRadius;
                             ox <= searchRadius;
                             ox++)
                        {
                            int nx = x + ox;
                            int ny = y + oy;

                            if (nx < 0 ||
                                ny < 0 ||
                                nx >= sampleWidth ||
                                ny >= sampleHeight)
                            {
                                continue;
                            }

                            Color32 candidate =
                                sample[
                                    ny *
                                    sampleWidth +
                                    nx];

                            if (candidate.a <= 24)
                                continue;

                            int distance =
                                ox * ox +
                                oy * oy;

                            if (distance >=
                                bestDistance)
                            {
                                continue;
                            }

                            bestDistance =
                                distance;

                            nearest =
                                new Color32(
                                    candidate.r,
                                    candidate.g,
                                    candidate.b,
                                    255);
                        }
                    }

                    filledSample[index] =
                        nearest;
                }
            }

            float tintR =
                tint.r /
                100f;

            float tintG =
                tint.g /
                100f;

            float tintB =
                tint.b /
                100f;

            Color32[] pixels =
                new Color32[
                    outW *
                    outH];

            // Build one continuous floor surface. The old implementation
            // changed mirroring/tone per large macro block, which produced the
            // obvious rectangular patches visible in Game view.
            const float sampleScaleX = 2.55f;
            const float sampleScaleY = 2.35f;

            for (int y = 0;
                 y < outH;
                 y++)
            {
                for (int x = 0;
                     x < outW;
                     x++)
                {
                    float warpX =
                        Mathf.Sin(
                            y * 0.0085f +
                            seed * 0.37f) *
                        5.0f +
                        Mathf.Sin(
                            y * 0.021f -
                            seed * 0.19f) *
                        1.8f;

                    float warpY =
                        Mathf.Sin(
                            x * 0.0072f -
                            seed * 0.23f) *
                        4.2f +
                        Mathf.Sin(
                            x * 0.018f +
                            seed * 0.31f) *
                        1.5f;

                    float sourceX =
                        x / sampleScaleX +
                        warpX;

                    float sourceY =
                        y / sampleScaleY +
                        warpY;

                    int tileX =
                        Mathf.FloorToInt(
                            sourceX /
                            Mathf.Max(
                                1,
                                sampleWidth));

                    int tileY =
                        Mathf.FloorToInt(
                            sourceY /
                            Mathf.Max(
                                1,
                                sampleHeight));

                    float localX =
                        Mathf.Repeat(
                            sourceX,
                            sampleWidth);

                    float localY =
                        Mathf.Repeat(
                            sourceY,
                            sampleHeight);

                    if (Mathf.Abs(tileX) % 2 == 1)
                    {
                        localX =
                            sampleWidth -
                            1 -
                            localX;
                    }

                    if (Mathf.Abs(tileY) % 2 == 1)
                    {
                        localY =
                            sampleHeight -
                            1 -
                            localY;
                    }

                    int sx =
                        Mathf.Clamp(
                            Mathf.RoundToInt(
                                localX),
                            0,
                            sampleWidth - 1);

                    int sy =
                        Mathf.Clamp(
                            Mathf.RoundToInt(
                                localY),
                            0,
                            sampleHeight - 1);

                    Color32 primary =
                        filledSample[
                            sy *
                            sampleWidth +
                            sx];

                    int detailX =
                        Mathf.Abs(
                            x / 5 +
                            seed * 17 +
                            y / 19) %
                        sampleWidth;

                    int detailY =
                        Mathf.Abs(
                            y / 5 +
                            seed * 29 -
                            x / 23) %
                        sampleHeight;

                    Color32 secondary =
                        filledSample[
                            detailY *
                            sampleWidth +
                            detailX];

                    float broad =
                        Mathf.Sin(
                            x * 0.0032f +
                            y * 0.0021f +
                            seed * 0.41f) *
                        0.032f;

                    float grime =
                        Mathf.Sin(
                            x * 0.010f -
                            y * 0.007f +
                            seed) *
                        0.014f;

                    float cross =
                        Mathf.Sin(
                            (x + y) *
                            0.016f +
                            seed * 0.7f) *
                        0.009f;

                    float r =
                        Mathf.Lerp(
                            primary.r,
                            secondary.r,
                            0.12f) *
                        tintR;

                    float g =
                        Mathf.Lerp(
                            primary.g,
                            secondary.g,
                            0.12f) *
                        tintG;

                    float b =
                        Mathf.Lerp(
                            primary.b,
                            secondary.b,
                            0.12f) *
                        tintB;

                    float multiplier =
                        1f +
                        broad +
                        grime +
                        cross;

                    pixels[
                        y *
                        outW +
                        x] =
                        new Color32(
                            (byte)Mathf.Clamp(
                                Mathf.RoundToInt(
                                    r *
                                    multiplier),
                                0,
                                255),
                            (byte)Mathf.Clamp(
                                Mathf.RoundToInt(
                                    g *
                                    multiplier),
                                0,
                                255),
                            (byte)Mathf.Clamp(
                                Mathf.RoundToInt(
                                    b *
                                    multiplier),
                                0,
                                255),
                            255);
                }
            }

            var random =
                new System.Random(
                    seed);

            // Large irregular plate seams. They are deliberately sparse and
            // offset, unlike the old checkerboard/grid.
            for (int i = 0;
                 i < 11;
                 i++)
            {
                int x0 =
                    random.Next(
                        40,
                        outW - 260);

                int y0 =
                    random.Next(
                        30,
                        outH - 100);

                int length =
                    random.Next(
                        150,
                        420);

                int rise =
                    random.Next(
                        -30,
                        31);

                DrawFloorLine(
                    pixels,
                    outW,
                    outH,
                    x0,
                    y0,
                    Mathf.Min(
                        outW - 30,
                        x0 + length),
                    Mathf.Clamp(
                        y0 + rise,
                        20,
                        outH - 20),
                    new Color32(
                        21,
                        24,
                        38,
                        92),
                    1);

                DrawFloorLine(
                    pixels,
                    outW,
                    outH,
                    x0,
                    y0 + 2,
                    Mathf.Min(
                        outW - 30,
                        x0 + length),
                    Mathf.Clamp(
                        y0 + rise + 2,
                        20,
                        outH - 20),
                    new Color32(
                        104,
                        89,
                        122,
                        36),
                    0);
            }

            for (int i = 0;
                 i < 34;
                 i++)
            {
                int cx =
                    random.Next(
                        32,
                        outW - 32);

                int cy =
                    random.Next(
                        32,
                        outH - 32);

                int rx =
                    random.Next(
                        18,
                        92);

                int ry =
                    random.Next(
                        8,
                        34);

                DarkenFloorEllipse(
                    pixels,
                    outW,
                    outH,
                    cx,
                    cy,
                    rx,
                    ry,
                    random.Next(
                        3,
                        12));
            }

            for (int i = 0;
                 i < 48;
                 i++)
            {
                int x0 =
                    random.Next(
                        20,
                        outW - 90);

                int y0 =
                    random.Next(
                        20,
                        outH - 40);

                int length =
                    random.Next(
                        8,
                        48);

                int rise =
                    random.Next(
                        -6,
                        7);

                DrawFloorLine(
                    pixels,
                    outW,
                    outH,
                    x0,
                    y0,
                    x0 + length,
                    y0 + rise,
                    new Color32(
                        18,
                        21,
                        34,
                        (byte)random.Next(
                            28,
                            70)),
                    0);
            }

            Texture2D output =
                new Texture2D(
                    outW,
                    outH,
                    TextureFormat.RGBA32,
                    false);

            output.SetPixels32(
                pixels);

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

            UnityEngine.Object.DestroyImmediate(
                output);

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions
                    .ForceSynchronousImport);

            ConfigureGeneratedSprite(
                path,
                ppu,
                new Vector2(
                    0.5f,
                    0.5f));

            return
                AssetDatabase.LoadAssetAtPath<
                    Sprite>(
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
            importer.isReadable = true;
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
