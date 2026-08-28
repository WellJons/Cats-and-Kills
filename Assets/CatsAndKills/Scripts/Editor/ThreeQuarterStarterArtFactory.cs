#if UNITY_EDITOR
using System;
using System.IO;
using CatsAndKills.Visual;
using UnityEditor;
using UnityEngine;

namespace CatsAndKills.EditorTools
{
    public static class ThreeQuarterStarterArtFactory
    {
        private const string Root =
            "Assets/CatsAndKills/Art/Production/GeneratedStarter";

        private static readonly Color32 Transparent = new Color32(0, 0, 0, 0);
        private static readonly Color32 Outline = new Color32(11, 14, 23, 255);
        private static readonly Color32 Steel = new Color32(75, 87, 110, 255);
        private static readonly Color32 Dark = new Color32(29, 35, 50, 255);
        private static readonly Color32 FurWhite = new Color32(230, 233, 240, 255);
        private static readonly Color32 FurEnemy = new Color32(166, 172, 183, 255);
        private static readonly Color32 Magenta = new Color32(205, 45, 108, 255);
        private static readonly Color32 Amber = new Color32(239, 167, 48, 255);
        private static readonly Color32 Red = new Color32(181, 36, 45, 255);

        [MenuItem("Tools/Cats and Kills/3-4 Art/Generate Starter 3-4 Art Pack")]
        public static void GenerateStarterPack()
        {
            ThreeQuarterArtPipeline.CreateProductionFolders();
            EnsureFolder(Root);
            EnsureFolder(Root + "/Characters");
            EnsureFolder(Root + "/Environment");
            EnsureFolder(Root + "/Weapons");
            EnsureFolder(Root + "/FX");

            DirectionalSpriteSet player = GenerateCharacterSet(
                "player",
                FurWhite,
                new Color32(36, 43, 61, 255),
                Magenta,
                0);

            DirectionalSpriteSet pistolier = GenerateCharacterSet(
                "pistolier",
                new Color32(181, 166, 149, 255),
                new Color32(77, 67, 63, 255),
                new Color32(151, 62, 57, 255),
                1);

            DirectionalSpriteSet rifleman = GenerateCharacterSet(
                "rifleman",
                FurEnemy,
                new Color32(56, 74, 72, 255),
                new Color32(111, 42, 54, 255),
                2);

            DirectionalSpriteSet machineGunner = GenerateCharacterSet(
                "machinegunner",
                new Color32(146, 154, 161, 255),
                new Color32(49, 65, 58, 255),
                new Color32(92, 38, 46, 255),
                3);

            DirectionalSpriteSet demolitionist = GenerateCharacterSet(
                "demolitionist",
                new Color32(169, 160, 158, 255),
                new Color32(69, 47, 50, 255),
                new Color32(190, 43, 52, 255),
                4);

            Sprite rifle = GenerateSprite("Weapons/rifle", Rifle(), 64f);
            Sprite pistol = GenerateSprite("Weapons/pistol", Pistol(), 64f);
            Sprite shotgun = GenerateSprite("Weapons/shotgun", Shotgun(), 64f);
            Sprite machineGun = GenerateSprite("Weapons/machinegun", MachineGun(), 64f);
            Sprite grenade = GenerateSprite("Weapons/grenade", Grenade(), 64f);

            Sprite floorIndustrial = GenerateSprite(
                "Environment/floor_industrial",
                FloorIndustrial(),
                64f);

            Sprite floorOffice = GenerateSprite(
                "Environment/floor_office",
                FloorOffice(),
                64f);

            Sprite wallStraight = GenerateSprite(
                "Environment/wall_straight",
                WallStraight(),
                64f);

            Sprite wallCorner = GenerateSprite(
                "Environment/wall_corner",
                WallCorner(),
                64f);

            Sprite wallDamaged = GenerateSprite(
                "Environment/wall_damaged",
                WallDamaged(),
                64f);

            Sprite reinforcedDoor = GenerateSprite(
                "Environment/reinforced_door",
                ReinforcedDoor(),
                64f);

            Sprite crateLight = GenerateSprite(
                "Environment/crate_light",
                Crate(false),
                64f);

            Sprite crateHeavy = GenerateSprite(
                "Environment/crate_heavy",
                Crate(true),
                64f);

            Sprite fuelDrum = GenerateSprite(
                "Environment/fuel_drum",
                FuelDrum(),
                64f);

            Sprite terminal = GenerateSprite(
                "Environment/terminal",
                Terminal(),
                64f);

            Sprite fence = GenerateSprite(
                "Environment/fence",
                Fence(),
                64f);

            Sprite pipe = GenerateSprite(
                "Environment/pipe_cluster",
                PipeCluster(),
                64f);

            Sprite lamp = GenerateSprite(
                "Environment/lamp",
                Lamp(),
                64f);

            Sprite debris = GenerateSprite(
                "Environment/debris",
                Debris(),
                64f);

            Sprite poster = GenerateSprite(
                "Environment/propaganda_poster",
                Poster(),
                64f);

            Sprite muzzle = GenerateSprite("FX/muzzle", Muzzle(), 64f);
            Sprite blood = GenerateSprite("FX/blood", Blood(), 64f);
            Sprite hole = GenerateSprite("FX/bullet_hole", BulletHole(), 64f);
            Sprite spark = GenerateSprite("FX/spark", Spark(), 64f);
            Sprite smoke = GenerateSprite("FX/smoke", Smoke(), 64f);
            Sprite explosion = GenerateSprite("FX/explosion", Explosion(), 64f);
            Sprite shadow = GenerateSprite("FX/soft_shadow", SoftShadow(), 64f);

            ProductionArtPack pack =
                AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                    ThreeQuarterArtPipeline.ArtPackPath);

            if (pack == null)
            {
                Debug.LogError("ProductionArtPack.asset could not be created.");
                return;
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
            pack.pipeCluster = pipe;
            pack.lamp = lamp;
            pack.debris = debris;
            pack.propagandaPoster = poster;

            pack.muzzleFlash = muzzle;
            pack.bloodDrop = blood;
            pack.bulletHole = hole;
            pack.spark = spark;
            pack.smoke = smoke;
            pack.explosion = explosion;
            pack.softShadow = shadow;

            EditorUtility.SetDirty(pack);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = pack;

            Debug.Log(
                "Cats and Kills starter 3/4 art pack generated and assigned.");
        }

        public static ProductionArtPack EnsureStarterPack()
        {
            ProductionArtPack pack =
                AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                    ThreeQuarterArtPipeline.ArtPackPath);

            if (pack == null || !pack.HasMinimumPlayableArt)
            {
                GenerateStarterPack();

                pack =
                    AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                        ThreeQuarterArtPipeline.ArtPackPath);
            }

            return pack;
        }

        private static DirectionalSpriteSet GenerateCharacterSet(
            string id,
            Color32 fur,
            Color32 uniform,
            Color32 accent,
            int archetype)
        {
            Sprite[] idle = new Sprite[8];
            Sprite[] hurt = new Sprite[8];
            Sprite[] dead = new Sprite[8];

            for (int i = 0; i < 8; i++)
            {
                CharacterDirection8 direction =
                    (CharacterDirection8)i;

                idle[i] = GenerateSprite(
                    $"Characters/{id}_idle_{direction}",
                    Character(direction, fur, uniform, accent, archetype, 0),
                    64f);

                hurt[i] = GenerateSprite(
                    $"Characters/{id}_hurt_{direction}",
                    Character(direction, fur, uniform, accent, archetype, 1),
                    64f);

                dead[i] = GenerateSprite(
                    $"Characters/{id}_dead_{direction}",
                    Character(direction, fur, uniform, accent, archetype, 2),
                    64f);
            }

            string assetPath = $"{Root}/Characters/{id}_set.asset";
            DirectionalSpriteSet set =
                AssetDatabase.LoadAssetAtPath<DirectionalSpriteSet>(assetPath);

            if (set == null)
            {
                set = ScriptableObject.CreateInstance<DirectionalSpriteSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }

            set.Configure(
                idle,
                idle,
                idle,
                idle,
                hurt,
                idle,
                dead);

            EditorUtility.SetDirty(set);
            return set;
        }

        private static Sprite GenerateSprite(
            string relative,
            PixelCanvas canvas,
            float pixelsPerUnit)
        {
            string path = $"{Root}/{relative}.png";
            string folder =
                Path.GetDirectoryName(path)?.Replace("\\", "/");

            if (!string.IsNullOrEmpty(folder))
                EnsureFolder(folder);

            Texture2D texture =
                new Texture2D(
                    canvas.Width,
                    canvas.Height,
                    TextureFormat.RGBA32,
                    false);

            texture.SetPixels32(canvas.Pixels);
            texture.Apply();

            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression =
                    TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static PixelCanvas Character(
            CharacterDirection8 direction,
            Color32 fur,
            Color32 uniform,
            Color32 accent,
            int archetype,
            int state)
        {
            PixelCanvas c = new PixelCanvas(128, 128);

            if (state == 2)
            {
                c.Ellipse(61, 32, 31, 10, new Color32(0, 0, 0, 70));
                c.RoundRect(31, 40, 64, 24, 9, Outline);
                c.RoundRect(35, 44, 56, 16, 7, uniform);
                c.Circle(96, 53, 16, Outline);
                c.Circle(96, 53, 13, fur);
                c.Triangle(88, 63, 92, 77, 99, 64, Outline);
                c.Triangle(101, 64, 108, 77, 111, 61, Outline);
                c.Line(45, 47, 18, 61, Outline, 6);
                c.Line(44, 51, 17, 64, Steel, 2);
                c.Circle(67, 56, 5, Red);
                return c;
            }

            Vector2 dir = DirectionVector(direction);
            Vector2 projected = new Vector2(dir.x, dir.y * 0.58f);
            if (projected.sqrMagnitude < 0.01f)
                projected = Vector2.right;
            projected.Normalize();

            Vector2 perp = new Vector2(-projected.y, projected.x);

            c.Ellipse(64, 22, archetype == 3 ? 23 : 19, 7, new Color32(0, 0, 0, 75));

            int hipX = 64;
            int hipY = 48;

            int spread = archetype == 3 ? 8 : 7;
            c.Line(hipX - spread, hipY, 54, 25, Outline, 8);
            c.Line(hipX + spread, hipY, 74, 25, Outline, 8);
            c.Line(hipX - spread, hipY, 54, 25, new Color32(24, 30, 43, 255), 5);
            c.Line(hipX + spread, hipY, 74, 25, new Color32(24, 30, 43, 255), 5);

            int torsoW = archetype == 3 ? 40 : 34;
            int torsoH = archetype == 3 ? 43 : 39;

            if (dir.y > 0.3f)
                DrawWeapon(c, projected, archetype, false);

            c.RoundRect(
                64 - torsoW / 2,
                46,
                torsoW,
                torsoH,
                10,
                Outline);

            c.RoundRect(
                64 - torsoW / 2 + 4,
                50,
                torsoW - 8,
                torsoH - 8,
                7,
                uniform);

            c.Rect(50, 63, 28, 5, new Color32(22, 27, 39, 255));

            if (archetype == 3)
            {
                c.Rect(44, 55, 8, 28, new Color32(45, 54, 55, 255));
                c.Rect(76, 55, 8, 28, new Color32(45, 54, 55, 255));
            }

            if (archetype == 4)
            {
                c.Line(49, 54, 77, 83, accent, 4);
                c.Line(78, 54, 51, 83, accent, 4);
            }

            int faceShiftX = Mathf.RoundToInt(dir.x * 4f);
            int faceShiftY = Mathf.RoundToInt(dir.y * 2f);

            c.Circle(64, 91, 22, Outline);
            c.Circle(64, 92, 18, fur);

            c.Triangle(48, 104, 53, 124, 62, 108, Outline);
            c.Triangle(80, 104, 75, 124, 66, 108, Outline);

            c.Triangle(52, 105, 55, 117, 61, 108, new Color32(177, 124, 137, 255));
            c.Triangle(76, 105, 73, 117, 67, 108, new Color32(177, 124, 137, 255));

            c.Rect(45, 75, 38, 7, Outline);
            c.Rect(48, 77, 32, 3, accent);

            bool facingBack = dir.y > 0.45f;

            if (!facingBack)
            {
                int eyeY = 94 + faceShiftY;
                c.Rect(55 + faceShiftX, eyeY, 4, 4, Outline);
                c.Rect(69 + faceShiftX, eyeY, 4, 4, Outline);

                c.Triangle(
                    61 + faceShiftX,
                    87 + faceShiftY,
                    67 + faceShiftX,
                    87 + faceShiftY,
                    64 + faceShiftX,
                    83 + faceShiftY,
                    new Color32(171, 91, 105, 255));
            }
            else
            {
                c.Rect(54, 95, 20, 4, new Color32(132, 136, 145, 255));
            }

            if (dir.y <= 0.3f)
                DrawWeapon(c, projected, archetype, true);

            if (state == 1)
            {
                c.Circle(75, 65, 6, Red);
                c.Line(75, 65, 86, 58, new Color32(220, 49, 60, 220), 3);
            }

            return c;
        }

        private static void DrawWeapon(
            PixelCanvas c,
            Vector2 projected,
            int archetype,
            bool foreground)
        {
            float length =
                archetype == 1 ? 26f :
                archetype == 3 ? 44f :
                38f;

            int cx = 64;
            int cy = foreground ? 70 : 67;

            int x0 = cx - Mathf.RoundToInt(projected.x * 13f);
            int y0 = cy - Mathf.RoundToInt(projected.y * 13f);
            int x1 = cx + Mathf.RoundToInt(projected.x * length);
            int y1 = cy + Mathf.RoundToInt(projected.y * length);

            int thick = archetype == 3 ? 7 : archetype == 1 ? 5 : 6;
            c.Line(x0, y0, x1, y1, Outline, thick);
            c.Line(x0, y0, x1, y1, new Color32(58, 63, 72, 255), Mathf.Max(2, thick - 3));

            Vector2 p = new Vector2(-projected.y, projected.x);

            int gripX = cx + Mathf.RoundToInt(projected.x * 6f);
            int gripY = cy + Mathf.RoundToInt(projected.y * 6f);

            c.Line(
                gripX,
                gripY,
                gripX - Mathf.RoundToInt(p.x * 9f) - Mathf.RoundToInt(projected.x * 3f),
                gripY - Mathf.RoundToInt(p.y * 9f) - Mathf.RoundToInt(projected.y * 3f),
                Outline,
                4);
        }

        private static Vector2 DirectionVector(CharacterDirection8 d)
        {
            switch (d)
            {
                case CharacterDirection8.NorthEast: return new Vector2(1f, 1f).normalized;
                case CharacterDirection8.North: return Vector2.up;
                case CharacterDirection8.NorthWest: return new Vector2(-1f, 1f).normalized;
                case CharacterDirection8.West: return Vector2.left;
                case CharacterDirection8.SouthWest: return new Vector2(-1f, -1f).normalized;
                case CharacterDirection8.South: return Vector2.down;
                case CharacterDirection8.SouthEast: return new Vector2(1f, -1f).normalized;
                default: return Vector2.right;
            }
        }

        private static PixelCanvas Rifle()
        {
            PixelCanvas c = new PixelCanvas(128, 36);
            c.Line(10, 18, 112, 18, Outline, 10);
            c.Line(15, 18, 108, 18, new Color32(55, 60, 67, 255), 5);
            c.Rect(34, 10, 28, 7, new Color32(37, 42, 49, 255));
            c.Polygon(new[]
            {
                new Vector2Int(47, 20), new Vector2Int(67, 20),
                new Vector2Int(62, 35), new Vector2Int(50, 34)
            }, new Color32(63, 43, 35, 255));
            return c;
        }

        private static PixelCanvas Pistol()
        {
            PixelCanvas c = new PixelCanvas(78, 48);
            c.RoundRect(7, 15, 52, 14, 3, Outline);
            c.Rect(11, 18, 45, 6, Steel);
            c.Polygon(new[]
            {
                new Vector2Int(29, 25), new Vector2Int(47, 25),
                new Vector2Int(44, 45), new Vector2Int(32, 45)
            }, Outline);
            return c;
        }

        private static PixelCanvas Shotgun()
        {
            PixelCanvas c = new PixelCanvas(138, 36);
            c.Line(8, 18, 126, 18, Outline, 9);
            c.Line(20, 18, 123, 18, Steel, 4);
            c.Rect(28, 10, 35, 15, new Color32(96, 61, 38, 255));
            return c;
        }

        private static PixelCanvas MachineGun()
        {
            PixelCanvas c = new PixelCanvas(150, 48);
            c.Line(9, 19, 140, 19, Outline, 12);
            c.Line(14, 19, 136, 19, new Color32(54, 60, 68, 255), 6);
            c.Rect(48, 24, 24, 21, Outline);
            c.Rect(53, 27, 14, 15, new Color32(91, 75, 51, 255));
            return c;
        }

        private static PixelCanvas Grenade()
        {
            PixelCanvas c = new PixelCanvas(48, 56);
            c.RoundRect(10, 15, 28, 33, 8, Outline);
            c.RoundRect(14, 18, 20, 27, 5, new Color32(57, 76, 61, 255));
            c.Rect(18, 7, 14, 12, Outline);
            c.Line(29, 8, 43, 15, Steel, 2);
            return c;
        }

        private static PixelCanvas FloorIndustrial()
        {
            PixelCanvas c = new PixelCanvas(96, 96);
            c.Rect(0, 0, 96, 96, new Color32(31, 38, 54, 255));
            c.Line(0, 48, 96, 48, new Color32(20, 25, 38, 255), 2);
            c.Line(48, 0, 48, 96, new Color32(20, 25, 38, 255), 2);
            c.Line(0, 0, 96, 96, new Color32(51, 59, 76, 90), 1);
            c.Circle(8, 8, 2, Steel);
            c.Circle(88, 8, 2, Steel);
            c.Circle(8, 88, 2, Steel);
            c.Circle(88, 88, 2, Steel);
            return c;
        }

        private static PixelCanvas FloorOffice()
        {
            PixelCanvas c = new PixelCanvas(96, 96);
            c.Rect(0, 0, 96, 96, new Color32(43, 40, 53, 255));
            for (int y = 0; y < 96; y += 24)
                c.Line(0, y, 96, y, new Color32(25, 24, 35, 255), 2);
            for (int x = 0; x < 96; x += 24)
                c.Line(x, 0, x, 96, new Color32(25, 24, 35, 255), 2);
            return c;
        }

        private static PixelCanvas WallStraight()
        {
            PixelCanvas c = new PixelCanvas(128, 112);
            c.Ellipse(64, 15, 58, 10, new Color32(0, 0, 0, 85));
            c.Polygon(new[]
            {
                new Vector2Int(8, 54), new Vector2Int(119, 54),
                new Vector2Int(108, 94), new Vector2Int(20, 94)
            }, new Color32(78, 89, 111, 255));
            c.Polygon(new[]
            {
                new Vector2Int(20, 94), new Vector2Int(108, 94),
                new Vector2Int(96, 108), new Vector2Int(32, 108)
            }, new Color32(111, 123, 143, 255));
            c.Polygon(new[]
            {
                new Vector2Int(8, 54), new Vector2Int(119, 54),
                new Vector2Int(108, 94), new Vector2Int(20, 94)
            }, new Color32(55, 65, 84, 255));
            c.Line(64, 57, 64, 92, new Color32(28, 34, 48, 255), 2);
            c.Line(15, 57, 113, 57, new Color32(127, 139, 157, 255), 2);
            return c;
        }

        private static PixelCanvas WallCorner()
        {
            PixelCanvas c = WallStraight();
            c.Polygon(new[]
            {
                new Vector2Int(8, 54), new Vector2Int(36, 69),
                new Vector2Int(36, 102), new Vector2Int(18, 94)
            }, new Color32(38, 47, 64, 255));
            return c;
        }

        private static PixelCanvas WallDamaged()
        {
            PixelCanvas c = WallStraight();
            c.Polygon(new[]
            {
                new Vector2Int(53, 94), new Vector2Int(63, 79),
                new Vector2Int(73, 94), new Vector2Int(81, 82),
                new Vector2Int(92, 96), new Vector2Int(86, 108),
                new Vector2Int(48, 108)
            }, Transparent);
            return c;
        }

        private static PixelCanvas ReinforcedDoor()
        {
            PixelCanvas c = new PixelCanvas(128, 112);
            c.Ellipse(64, 12, 55, 9, new Color32(0, 0, 0, 90));
            c.Polygon(new[]
            {
                new Vector2Int(15, 48), new Vector2Int(113, 48),
                new Vector2Int(103, 96), new Vector2Int(25, 96)
            }, Outline);
            c.Polygon(new[]
            {
                new Vector2Int(21, 52), new Vector2Int(107, 52),
                new Vector2Int(98, 91), new Vector2Int(30, 91)
            }, new Color32(54, 63, 82, 255));
            c.Line(64, 53, 64, 92, new Color32(13, 17, 27, 255), 4);
            c.Circle(91, 72, 6, Amber);
            return c;
        }

        private static PixelCanvas Crate(bool heavy)
        {
            PixelCanvas c = new PixelCanvas(96, 96);
            c.Ellipse(48, 11, 35, 9, new Color32(0, 0, 0, 80));
            Color32 top = heavy ? new Color32(73, 77, 83, 255) : new Color32(109, 71, 47, 255);
            Color32 front = heavy ? new Color32(48, 52, 61, 255) : new Color32(74, 48, 37, 255);
            c.Polygon(new[]
            {
                new Vector2Int(18, 53), new Vector2Int(49, 70),
                new Vector2Int(79, 53), new Vector2Int(48, 36)
            }, top);
            c.Polygon(new[]
            {
                new Vector2Int(18, 53), new Vector2Int(49, 70),
                new Vector2Int(49, 25), new Vector2Int(18, 12)
            }, front);
            c.Polygon(new[]
            {
                new Vector2Int(49, 70), new Vector2Int(79, 53),
                new Vector2Int(79, 14), new Vector2Int(49, 25)
            }, new Color32(
                (byte)Mathf.Max(0, front.r - 12),
                (byte)Mathf.Max(0, front.g - 10),
                (byte)Mathf.Max(0, front.b - 7),
                255));
            c.Line(22, 50, 75, 19, Outline, 3);
            c.Line(74, 50, 23, 20, Outline, 3);
            return c;
        }

        private static PixelCanvas FuelDrum()
        {
            PixelCanvas c = new PixelCanvas(64, 96);
            c.Ellipse(32, 12, 22, 7, new Color32(0, 0, 0, 80));
            c.RoundRect(12, 25, 40, 58, 10, Outline);
            c.RoundRect(16, 29, 32, 50, 8, new Color32(132, 34, 39, 255));
            c.Rect(15, 43, 34, 6, new Color32(78, 27, 32, 255));
            c.Rect(15, 63, 34, 6, new Color32(78, 27, 32, 255));
            c.Triangle(32, 51, 24, 64, 40, 64, Amber);
            return c;
        }

        private static PixelCanvas Terminal()
        {
            PixelCanvas c = new PixelCanvas(96, 128);
            c.Ellipse(49, 10, 28, 8, new Color32(0, 0, 0, 75));
            c.Polygon(new[]
            {
                new Vector2Int(21, 45), new Vector2Int(76, 45),
                new Vector2Int(69, 103), new Vector2Int(27, 103)
            }, Outline);
            c.Polygon(new[]
            {
                new Vector2Int(26, 49), new Vector2Int(71, 49),
                new Vector2Int(66, 96), new Vector2Int(31, 96)
            }, new Color32(47, 57, 75, 255));
            c.Polygon(new[]
            {
                new Vector2Int(34, 67), new Vector2Int(63, 67),
                new Vector2Int(60, 90), new Vector2Int(37, 90)
            }, new Color32(37, 169, 176, 255));
            c.Line(35, 78, 60, 78, new Color32(169, 244, 238, 255), 2);
            return c;
        }

        private static PixelCanvas Fence()
        {
            PixelCanvas c = new PixelCanvas(128, 96);
            for (int x = 8; x < 128; x += 14)
                c.Line(x, 12, x, 82, Steel, 3);
            for (int x = -40; x < 140; x += 16)
                c.Line(x, 14, x + 56, 82, new Color32(51, 62, 79, 255), 2);
            return c;
        }

        private static PixelCanvas PipeCluster()
        {
            PixelCanvas c = new PixelCanvas(96, 96);
            c.Line(19, 12, 19, 86, Outline, 12);
            c.Line(19, 12, 19, 86, new Color32(73, 83, 96, 255), 7);
            c.Line(45, 18, 45, 86, Outline, 10);
            c.Line(45, 18, 45, 86, new Color32(94, 67, 53, 255), 5);
            c.Line(70, 8, 70, 84, Outline, 9);
            c.Line(70, 8, 70, 84, new Color32(54, 76, 80, 255), 4);
            return c;
        }

        private static PixelCanvas Lamp()
        {
            PixelCanvas c = new PixelCanvas(64, 96);
            c.Line(32, 8, 32, 61, Outline, 6);
            c.RoundRect(14, 57, 36, 21, 7, Outline);
            c.RoundRect(19, 61, 26, 13, 5, new Color32(176, 208, 213, 255));
            return c;
        }

        private static PixelCanvas Debris()
        {
            PixelCanvas c = new PixelCanvas(96, 64);
            c.Polygon(new[] { new Vector2Int(13, 10), new Vector2Int(31, 18), new Vector2Int(23, 35) }, Steel);
            c.Polygon(new[] { new Vector2Int(46, 8), new Vector2Int(67, 13), new Vector2Int(61, 31), new Vector2Int(42, 25) }, new Color32(65, 55, 52, 255));
            c.Line(58, 18, 85, 48, new Color32(87, 73, 58, 255), 4);
            return c;
        }

        private static PixelCanvas Poster()
        {
            PixelCanvas c = new PixelCanvas(64, 96);
            c.Rect(4, 4, 56, 88, Outline);
            c.Rect(8, 8, 48, 80, new Color32(177, 164, 148, 255));
            c.Rect(13, 58, 38, 8, Red);
            c.Circle(32, 40, 15, new Color32(59, 64, 78, 255));
            c.Rect(18, 19, 28, 4, new Color32(63, 49, 50, 255));
            return c;
        }

        private static PixelCanvas Muzzle()
        {
            PixelCanvas c = new PixelCanvas(64, 64);
            c.Triangle(32, 4, 39, 29, 60, 15, Amber);
            c.Triangle(32, 4, 25, 29, 5, 16, new Color32(255, 216, 99, 255));
            c.Triangle(32, 60, 39, 35, 59, 48, Amber);
            c.Triangle(32, 60, 25, 35, 6, 48, new Color32(255, 216, 99, 255));
            c.Circle(32, 32, 11, new Color32(255, 248, 202, 255));
            return c;
        }

        private static PixelCanvas Blood()
        {
            PixelCanvas c = new PixelCanvas(32, 32);
            c.Circle(16, 16, 10, new Color32(132, 15, 26, 220));
            c.Circle(13, 13, 4, new Color32(207, 33, 48, 230));
            return c;
        }

        private static PixelCanvas BulletHole()
        {
            PixelCanvas c = new PixelCanvas(32, 32);
            c.Circle(16, 16, 7, new Color32(5, 6, 9, 240));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 0.25f;
                c.Line(16, 16, 16 + Mathf.RoundToInt(Mathf.Cos(a) * 13), 16 + Mathf.RoundToInt(Mathf.Sin(a) * 13), new Color32(47, 50, 57, 175), 1);
            }
            return c;
        }

        private static PixelCanvas Spark()
        {
            PixelCanvas c = new PixelCanvas(32, 32);
            c.Line(16, 2, 16, 30, new Color32(255, 221, 105, 255), 3);
            c.Line(2, 16, 30, 16, Amber, 3);
            c.Line(6, 6, 26, 26, new Color32(255, 238, 151, 255), 2);
            c.Line(26, 6, 6, 26, new Color32(255, 183, 59, 255), 2);
            return c;
        }

        private static PixelCanvas Smoke()
        {
            PixelCanvas c = new PixelCanvas(96, 96);
            c.Circle(34, 44, 25, new Color32(79, 84, 96, 95));
            c.Circle(58, 36, 28, new Color32(65, 70, 83, 90));
            c.Circle(51, 61, 22, new Color32(88, 93, 103, 78));
            return c;
        }

        private static PixelCanvas Explosion()
        {
            PixelCanvas c = new PixelCanvas(96, 96);
            for (int i = 0; i < 16; i++)
            {
                float a = i * Mathf.PI * 2f / 16f;
                c.Line(48, 48, 48 + Mathf.RoundToInt(Mathf.Cos(a) * 43), 48 + Mathf.RoundToInt(Mathf.Sin(a) * 43), new Color32(255, 112, 31, 220), 5);
            }
            c.Circle(48, 48, 28, new Color32(255, 164, 55, 240));
            c.Circle(48, 48, 15, new Color32(255, 239, 171, 255));
            return c;
        }

        private static PixelCanvas SoftShadow()
        {
            PixelCanvas c = new PixelCanvas(96, 64);
            for (int y = 0; y < c.Height; y++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    float nx = (x - 47.5f) / 47.5f;
                    float ny = (y - 31.5f) / 31.5f;
                    float d = nx * nx + ny * ny;
                    if (d > 1f) continue;
                    byte a = (byte)Mathf.RoundToInt(Mathf.Pow(1f - d, 1.7f) * 130f);
                    c.Set(x, y, new Color32(2, 4, 8, a));
                }
            }
            return c;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent =
                Path.GetDirectoryName(path)?.Replace("\\", "/");

            string leaf = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }

        private sealed class PixelCanvas
        {
            public int Width { get; }
            public int Height { get; }
            public Color32[] Pixels { get; }

            public PixelCanvas(int width, int height)
            {
                Width = width;
                Height = height;
                Pixels = new Color32[width * height];

                for (int i = 0; i < Pixels.Length; i++)
                    Pixels[i] = Transparent;
            }

            public void Set(int x, int y, Color32 color)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height)
                    return;

                Pixels[y * Width + x] = color;
            }

            public void Rect(int x, int y, int w, int h, Color32 color)
            {
                for (int yy = y; yy < y + h; yy++)
                    for (int xx = x; xx < x + w; xx++)
                        Set(xx, yy, color);
            }

            public void Circle(int cx, int cy, int r, Color32 color)
            {
                int r2 = r * r;
                for (int y = cy - r; y <= cy + r; y++)
                    for (int x = cx - r; x <= cx + r; x++)
                    {
                        int dx = x - cx;
                        int dy = y - cy;
                        if (dx * dx + dy * dy <= r2)
                            Set(x, y, color);
                    }
            }

            public void Ellipse(int cx, int cy, int rx, int ry, Color32 color)
            {
                float irx = 1f / Mathf.Max(1, rx);
                float iry = 1f / Mathf.Max(1, ry);

                for (int y = cy - ry; y <= cy + ry; y++)
                    for (int x = cx - rx; x <= cx + rx; x++)
                    {
                        float dx = (x - cx) * irx;
                        float dy = (y - cy) * iry;

                        if (dx * dx + dy * dy <= 1f)
                            Set(x, y, color);
                    }
            }

            public void RoundRect(int x, int y, int w, int h, int r, Color32 color)
            {
                Rect(x + r, y, w - r * 2, h, color);
                Rect(x, y + r, w, h - r * 2, color);
                Circle(x + r, y + r, r, color);
                Circle(x + w - r - 1, y + r, r, color);
                Circle(x + r, y + h - r - 1, r, color);
                Circle(x + w - r - 1, y + h - r - 1, r, color);
            }

            public void Triangle(int x1, int y1, int x2, int y2, int x3, int y3, Color32 color)
            {
                Polygon(new[]
                {
                    new Vector2Int(x1, y1),
                    new Vector2Int(x2, y2),
                    new Vector2Int(x3, y3)
                }, color);
            }

            public void Line(int x0, int y0, int x1, int y1, Color32 color, int thickness)
            {
                int dx = Mathf.Abs(x1 - x0);
                int sx = x0 < x1 ? 1 : -1;
                int dy = -Mathf.Abs(y1 - y0);
                int sy = y0 < y1 ? 1 : -1;
                int error = dx + dy;

                while (true)
                {
                    for (int oy = -thickness / 2; oy <= thickness / 2; oy++)
                        for (int ox = -thickness / 2; ox <= thickness / 2; ox++)
                            Set(x0 + ox, y0 + oy, color);

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

            public void Polygon(Vector2Int[] points, Color32 color)
            {
                if (points == null || points.Length < 3)
                    return;

                int minY = Height - 1;
                int maxY = 0;

                foreach (Vector2Int p in points)
                {
                    minY = Mathf.Min(minY, p.y);
                    maxY = Mathf.Max(maxY, p.y);
                }

                for (int y = minY; y <= maxY; y++)
                {
                    int[] nodes = new int[points.Length];
                    int count = 0;
                    int j = points.Length - 1;

                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector2Int pi = points[i];
                        Vector2Int pj = points[j];

                        if ((pi.y < y && pj.y >= y) ||
                            (pj.y < y && pi.y >= y))
                        {
                            int denominator = pj.y - pi.y;
                            if (denominator != 0)
                            {
                                nodes[count++] =
                                    pi.x +
                                    (y - pi.y) *
                                    (pj.x - pi.x) /
                                    denominator;
                            }
                        }

                        j = i;
                    }

                    Array.Sort(nodes, 0, count);

                    for (int i = 0; i + 1 < count; i += 2)
                        for (int x = nodes[i]; x <= nodes[i + 1]; x++)
                            Set(x, y, color);
                }
            }
        }
    }
}
#endif
