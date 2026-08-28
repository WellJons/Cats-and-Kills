#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CatsAndKills.EditorTools
{
    public static class GeneratedArtFactory
    {
        private const string Root =
            "Assets/CatsAndKills/Generated/Art";

        private static readonly string[] Ids =
        {
            "ui_square",
            "ui_circle",
            "soft_shadow",
            "hazard",
            "floor_panel",
            "cat_head",
            "enemy_head",
            "torso",
            "arm",
            "leg",
            "rifle",
            "pistol",
            "shotgun",
            "machinegun",
            "grenade",
            "floor",
            "wall",
            "crate",
            "barrel",
            "door",
            "muzzle",
            "blood",
            "spark",
            "casing",
            "bullet_hole",
            "smoke",
            "explosion"
        };

        private static readonly Color32 Transparent =
            new Color32(0, 0, 0, 0);

        private static readonly Color32 Outline =
            new Color32(14, 17, 28, 255);

        private static readonly Color32 Navy =
            new Color32(39, 47, 68, 255);

        private static readonly Color32 Steel =
            new Color32(70, 82, 104, 255);

        private static readonly Color32 White =
            new Color32(232, 234, 241, 255);

        private static readonly Color32 Grey =
            new Color32(157, 164, 179, 255);

        private static readonly Color32 Red =
            new Color32(194, 30, 48, 255);

        private static readonly Color32 Magenta =
            new Color32(211, 43, 108, 255);

        private static readonly Color32 Amber =
            new Color32(240, 174, 62, 255);

        private static readonly Color32 Cyan =
            new Color32(71, 190, 201, 255);

        public static void RegenerateAll()
        {
            Directory.CreateDirectory(Root);

            foreach (string id in Ids)
                Write(id);

            AssetDatabase.Refresh();

            foreach (string id in Ids)
                ConfigureImporter(PathFor(id));

            AssetDatabase.Refresh();
        }

        public static Sprite Get(string id)
        {
            string path = PathFor(id);

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Root);
                Write(id);
                AssetDatabase.ImportAsset(
                    path,
                    ImportAssetOptions.ForceSynchronousImport);

                ConfigureImporter(path);
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static string PathFor(string id)
        {
            return $"{Root}/{id}.png";
        }

        private static void Write(string id)
        {
            PixelCanvas canvas = id switch
            {
                "ui_circle" => UiCircle(),
                "soft_shadow" => SoftShadow(),
                "hazard" => Hazard(),
                "floor_panel" => FloorPanel(),
                "cat_head" => CatHead(false),
                "enemy_head" => CatHead(true),
                "torso" => Torso(),
                "arm" => Arm(),
                "leg" => Leg(),
                "rifle" => Rifle(),
                "pistol" => Pistol(),
                "shotgun" => Shotgun(),
                "machinegun" => MachineGun(),
                "grenade" => Grenade(),
                "floor" => Floor(),
                "wall" => Wall(),
                "crate" => Crate(),
                "barrel" => Barrel(),
                "door" => Door(),
                "muzzle" => Muzzle(),
                "blood" => Blood(),
                "spark" => Spark(),
                "casing" => Casing(),
                "bullet_hole" => BulletHole(),
                "smoke" => Smoke(),
                "explosion" => Explosion(),
                _ => SolidSquare()
            };

            Texture2D texture = new Texture2D(
                canvas.Width,
                canvas.Height,
                TextureFormat.RGBA32,
                false);

            texture.SetPixels32(canvas.Pixels);
            texture.Apply();

            byte[] png = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            File.WriteAllBytes(PathFor(id), png);
        }

        private static void ConfigureImporter(string path)
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);

            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;

            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Repeat;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        private static PixelCanvas SolidSquare()
        {
            PixelCanvas c = new PixelCanvas(32, 32);
            c.Rect(0, 0, 32, 32, White);
            return c;
        }

        private static PixelCanvas UiCircle()
        {
            PixelCanvas c = new PixelCanvas(32, 32);
            c.Circle(16, 16, 15, White);
            return c;
        }

        private static PixelCanvas SoftShadow()
        {
            PixelCanvas c = new PixelCanvas(64, 40);

            for (int y = 0; y < c.Height; y++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    float nx = (x - 31.5f) / 31.5f;
                    float ny = (y - 19.5f) / 19.5f;
                    float d = nx * nx + ny * ny;

                    if (d > 1f) continue;

                    byte a = (byte)Mathf.RoundToInt(
                        Mathf.Pow(1f - d, 1.7f) * 130f);

                    c.Set(x, y, new Color32(3, 5, 10, a));
                }
            }

            return c;
        }

        private static PixelCanvas Hazard()
        {
            PixelCanvas c = new PixelCanvas(64, 64);
            Color32 baseColor = new Color32(38, 41, 48, 255);
            Color32 stripe = new Color32(226, 164, 39, 255);

            c.Rect(0, 0, 64, 64, baseColor);

            for (int x = -64; x < 128; x += 20)
            {
                c.Polygon(
                    new[]
                    {
                        new Vector2Int(x, 0),
                        new Vector2Int(x + 10, 0),
                        new Vector2Int(x + 64, 64),
                        new Vector2Int(x + 54, 64)
                    },
                    stripe);
            }

            c.Rect(0, 0, 64, 3, Outline);
            c.Rect(0, 61, 64, 3, Outline);

            return c;
        }

        private static PixelCanvas FloorPanel()
        {
            PixelCanvas c = new PixelCanvas(64, 64);
            Color32 baseColor = new Color32(34, 40, 54, 255);
            Color32 seam = new Color32(19, 23, 34, 255);
            Color32 edge = new Color32(59, 67, 84, 255);

            c.Rect(0, 0, 64, 64, baseColor);
            c.Rect(0, 0, 64, 2, seam);
            c.Rect(0, 62, 64, 2, seam);
            c.Rect(0, 0, 2, 64, seam);
            c.Rect(62, 0, 2, 64, seam);

            c.Rect(6, 6, 52, 2, edge);
            c.Rect(6, 56, 52, 2, new Color32(28, 33, 45, 255));
            c.Circle(8, 8, 2, Grey);
            c.Circle(56, 8, 2, Grey);
            c.Circle(8, 56, 2, Grey);
            c.Circle(56, 56, 2, Grey);

            return c;
        }

        private static PixelCanvas CatHead(bool enemy)
        {
            PixelCanvas c = new PixelCanvas(64, 64);

            Color32 fur = enemy
                ? new Color32(168, 174, 188, 255)
                : White;

            c.Triangle(10, 27, 17, 5, 31, 22, Outline);
            c.Triangle(54, 27, 47, 5, 33, 22, Outline);

            c.Triangle(
                15, 24,
                19, 11,
                28, 22,
                enemy
                    ? new Color32(122, 112, 126, 255)
                    : new Color32(191, 146, 161, 255));

            c.Triangle(
                49, 24,
                45, 11,
                36, 22,
                enemy
                    ? new Color32(122, 112, 126, 255)
                    : new Color32(191, 146, 161, 255));

            c.Circle(32, 35, 24, Outline);
            c.Circle(32, 35, 20, fur);

            c.Rect(20, 30, 6, 7, Outline);
            c.Rect(39, 30, 6, 7, Outline);

            Color32 eye = enemy
                ? new Color32(216, 83, 67, 255)
                : new Color32(54, 57, 72, 255);

            c.Rect(22, 32, 2, 3, eye);
            c.Rect(41, 32, 2, 3, eye);

            c.Triangle(
                29, 40,
                35, 40,
                32, 45,
                new Color32(181, 94, 113, 255));

            c.Rect(15, 51, 34, 10, Outline);
            c.Rect(
                18, 53, 28, 5,
                enemy
                    ? new Color32(145, 39, 54, 255)
                    : Magenta);

            return c;
        }

        private static PixelCanvas Torso()
        {
            PixelCanvas c = new PixelCanvas(64, 64);

            c.RoundRect(10, 5, 44, 54, 9, Outline);
            c.RoundRect(14, 9, 36, 46, 7, Navy);

            c.RoundRect(
                18, 17, 28, 33, 4,
                new Color32(29, 34, 49, 255));

            c.Rect(20, 22, 24, 5, Steel);
            c.Rect(21, 32, 10, 13, new Color32(50, 57, 74, 255));
            c.Rect(34, 32, 9, 13, new Color32(50, 57, 74, 255));
            c.Rect(29, 17, 6, 33, Outline);

            c.Rect(14, 11, 4, 26, Magenta);
            c.Rect(46, 11, 4, 26, Magenta);

            return c;
        }

        private static PixelCanvas Arm()
        {
            PixelCanvas c = new PixelCanvas(64, 24);

            c.RoundRect(2, 3, 55, 18, 7, Outline);
            c.RoundRect(5, 6, 49, 12, 5, Navy);
            c.Circle(55, 12, 7, Outline);
            c.Circle(55, 12, 4, White);

            return c;
        }

        private static PixelCanvas Leg()
        {
            PixelCanvas c = new PixelCanvas(60, 24);

            c.RoundRect(2, 3, 52, 18, 7, Outline);
            c.RoundRect(
                5, 6, 43, 12, 5,
                new Color32(27, 32, 47, 255));

            c.RoundRect(
                44, 3, 14, 18, 5,
                new Color32(13, 16, 25, 255));

            return c;
        }

        private static PixelCanvas Rifle()
        {
            PixelCanvas c = new PixelCanvas(128, 32);

            c.RoundRect(8, 9, 84, 13, 3, Outline);
            c.RoundRect(
                13, 12, 77, 7, 2,
                new Color32(52, 56, 62, 255));

            c.Rect(88, 13, 34, 5, Outline);
            c.Rect(92, 14, 30, 2, Steel);

            c.Polygon(
                new[]
                {
                    new Vector2Int(12, 20),
                    new Vector2Int(38, 20),
                    new Vector2Int(29, 31),
                    new Vector2Int(10, 29)
                },
                new Color32(80, 49, 35, 255));

            c.Polygon(
                new[]
                {
                    new Vector2Int(48, 19),
                    new Vector2Int(67, 19),
                    new Vector2Int(61, 31),
                    new Vector2Int(51, 30)
                },
                new Color32(47, 35, 32, 255));

            c.Rect(21, 5, 25, 5, Outline);
            c.Rect(24, 6, 19, 3, new Color32(35, 39, 44, 255));

            return c;
        }

        private static PixelCanvas Pistol()
        {
            PixelCanvas c = new PixelCanvas(80, 40);

            c.RoundRect(7, 8, 53, 14, 3, Outline);
            c.RoundRect(11, 11, 47, 7, 2, Steel);

            c.Polygon(
                new[]
                {
                    new Vector2Int(31, 19),
                    new Vector2Int(49, 19),
                    new Vector2Int(45, 37),
                    new Vector2Int(33, 37)
                },
                Outline);

            c.Polygon(
                new[]
                {
                    new Vector2Int(34, 22),
                    new Vector2Int(46, 22),
                    new Vector2Int(43, 34),
                    new Vector2Int(36, 34)
                },
                new Color32(48, 51, 58, 255));

            return c;
        }

        private static PixelCanvas Shotgun()
        {
            PixelCanvas c = new PixelCanvas(128, 32);

            c.RoundRect(7, 10, 101, 11, 3, Outline);
            c.Rect(31, 13, 90, 4, Steel);
            c.RoundRect(
                10, 12, 30, 8, 3,
                new Color32(91, 57, 36, 255));

            c.Polygon(
                new[]
                {
                    new Vector2Int(14, 19),
                    new Vector2Int(42, 19),
                    new Vector2Int(28, 31),
                    new Vector2Int(9, 29)
                },
                new Color32(87, 53, 34, 255));

            return c;
        }

        private static PixelCanvas MachineGun()
        {
            PixelCanvas c = new PixelCanvas(144, 40);

            c.RoundRect(10, 8, 101, 15, 4, Outline);
            c.RoundRect(
                15, 12, 94, 7, 2,
                new Color32(48, 54, 61, 255));

            c.Rect(106, 13, 35, 5, Outline);
            c.Rect(110, 14, 31, 2, Steel);

            c.Rect(50, 21, 21, 17, Outline);
            c.Rect(
                54, 24, 13, 11,
                new Color32(69, 59, 44, 255));

            c.Line(82, 21, 74, 39, Outline, 3);
            c.Line(93, 21, 102, 39, Outline, 3);

            return c;
        }

        private static PixelCanvas Grenade()
        {
            PixelCanvas c = new PixelCanvas(48, 48);

            c.RoundRect(10, 13, 28, 29, 7, Outline);
            c.RoundRect(
                13, 16, 22, 23, 5,
                new Color32(61, 80, 60, 255));

            c.Rect(19, 6, 13, 10, Outline);
            c.Rect(22, 8, 7, 7, Steel);
            c.Line(29, 7, 42, 15, Grey, 2);

            return c;
        }

        private static PixelCanvas Floor()
        {
            PixelCanvas c = new PixelCanvas(64, 64);
            var random = new System.Random(7);

            c.Rect(
                0, 0, 64, 64,
                new Color32(29, 33, 47, 255));

            for (int i = 0; i < 100; i++)
            {
                int x = random.Next(0, 64);
                int y = random.Next(0, 64);
                byte v = (byte)random.Next(31, 50);
                c.Set(x, y, new Color32(v, (byte)(v + 3), (byte)(v + 12), 255));
            }

            c.Line(0, 63, 63, 63, new Color32(45, 50, 66, 255), 1);
            c.Line(63, 0, 63, 63, new Color32(20, 23, 35, 255), 1);

            return c;
        }

        private static PixelCanvas Wall()
        {
            PixelCanvas c = new PixelCanvas(64, 64);

            c.Rect(0, 0, 64, 64, new Color32(48, 56, 75, 255));
            c.Rect(0, 0, 64, 4, Outline);
            c.Rect(0, 60, 64, 4, Outline);

            c.Line(0, 20, 63, 20, new Color32(78, 88, 108, 255), 2);
            c.Line(0, 44, 63, 44, new Color32(27, 31, 45, 255), 2);

            c.Circle(14, 12, 3, Grey);
            c.Circle(50, 12, 3, Grey);
            c.Circle(14, 52, 3, Grey);
            c.Circle(50, 52, 3, Grey);

            return c;
        }

        private static PixelCanvas Crate()
        {
            PixelCanvas c = new PixelCanvas(64, 64);

            c.RoundRect(3, 3, 58, 58, 4, Outline);
            c.Rect(8, 8, 48, 48, new Color32(98, 67, 48, 255));

            Color32 dark = new Color32(59, 40, 32, 255);
            c.Line(10, 10, 54, 54, dark, 5);
            c.Line(54, 10, 10, 54, dark, 5);

            return c;
        }

        private static PixelCanvas Barrel()
        {
            PixelCanvas c = new PixelCanvas(48, 64);

            c.RoundRect(7, 3, 34, 58, 8, Outline);
            c.RoundRect(11, 6, 26, 52, 5, new Color32(139, 32, 34, 255));
            c.Rect(10, 16, 28, 5, new Color32(82, 25, 30, 255));
            c.Rect(10, 43, 28, 5, new Color32(82, 25, 30, 255));
            c.Triangle(24, 25, 16, 39, 32, 39, Amber);

            return c;
        }

        private static PixelCanvas Door()
        {
            PixelCanvas c = new PixelCanvas(128, 32);

            c.RoundRect(2, 2, 124, 28, 4, Outline);
            c.RoundRect(7, 6, 114, 20, 3, new Color32(49, 58, 75, 255));

            for (int x = 14; x < 110; x += 22)
                c.Rect(x, 9, 13, 14, new Color32(67, 77, 95, 255));

            c.Circle(108, 16, 4, Amber);
            c.Rect(61, 6, 5, 20, Outline);

            return c;
        }

        private static PixelCanvas Muzzle()
        {
            PixelCanvas c = new PixelCanvas(64, 64);

            c.Polygon(
                new[]
                {
                    new Vector2Int(32, 2),
                    new Vector2Int(38, 22),
                    new Vector2Int(60, 12),
                    new Vector2Int(46, 30),
                    new Vector2Int(63, 38),
                    new Vector2Int(42, 41),
                    new Vector2Int(51, 61),
                    new Vector2Int(33, 48),
                    new Vector2Int(20, 63),
                    new Vector2Int(21, 43),
                    new Vector2Int(2, 49),
                    new Vector2Int(17, 32),
                    new Vector2Int(4, 19),
                    new Vector2Int(25, 22)
                },
                new Color32(255, 204, 80, 255));

            c.Circle(32, 33, 12, new Color32(255, 247, 199, 255));
            return c;
        }

        private static PixelCanvas Blood()
        {
            PixelCanvas c = new PixelCanvas(24, 24);
            c.Circle(12, 12, 8, Red);
            c.Circle(10, 10, 3, new Color32(230, 48, 65, 255));
            return c;
        }

        private static PixelCanvas Spark()
        {
            PixelCanvas c = new PixelCanvas(24, 24);
            c.Triangle(12, 1, 18, 12, 12, 23, Amber);
            c.Triangle(12, 1, 6, 12, 12, 23, new Color32(255, 231, 133, 255));
            return c;
        }

        private static PixelCanvas Casing()
        {
            PixelCanvas c = new PixelCanvas(20, 32);
            c.RoundRect(5, 2, 10, 28, 3, Outline);
            c.RoundRect(7, 4, 6, 24, 2, new Color32(215, 164, 67, 255));
            return c;
        }

        private static PixelCanvas BulletHole()
        {
            PixelCanvas c = new PixelCanvas(32, 32);
            c.Circle(16, 16, 7, new Color32(6, 7, 10, 230));
            c.Circle(16, 16, 3, new Color32(1, 1, 2, 255));

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f;
                int x = 16 + Mathf.RoundToInt(Mathf.Cos(angle) * 13f);
                int y = 16 + Mathf.RoundToInt(Mathf.Sin(angle) * 13f);
                c.Line(16, 16, x, y, new Color32(33, 35, 42, 170), 1);
            }

            return c;
        }

        private static PixelCanvas Smoke()
        {
            PixelCanvas c = new PixelCanvas(64, 64);
            Color32 smokeA = new Color32(96, 101, 113, 100);
            Color32 smokeB = new Color32(72, 77, 90, 80);

            c.Circle(23, 35, 18, smokeA);
            c.Circle(38, 28, 19, smokeA);
            c.Circle(31, 18, 15, smokeB);
            c.Circle(45, 42, 13, smokeB);

            return c;
        }

        private static PixelCanvas Explosion()
        {
            PixelCanvas c = new PixelCanvas(96, 96);

            for (int i = 0; i < 18; i++)
            {
                float angle = i * Mathf.PI * 2f / 18f;
                int x = 48 + Mathf.RoundToInt(Mathf.Cos(angle) * 43f);
                int y = 48 + Mathf.RoundToInt(Mathf.Sin(angle) * 43f);
                c.Line(48, 48, x, y, new Color32(255, 125, 36, 210), 5);
            }

            c.Circle(48, 48, 31, new Color32(255, 164, 57, 235));
            c.Circle(48, 48, 20, new Color32(255, 223, 118, 250));
            c.Circle(48, 48, 9, new Color32(255, 250, 220, 255));

            return c;
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

            public void Rect(
                int x,
                int y,
                int width,
                int height,
                Color32 color)
            {
                for (int yy = y; yy < y + height; yy++)
                {
                    for (int xx = x; xx < x + width; xx++)
                        Set(xx, yy, color);
                }
            }

            public void Circle(
                int cx,
                int cy,
                int radius,
                Color32 color)
            {
                int r2 = radius * radius;

                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    for (int x = cx - radius; x <= cx + radius; x++)
                    {
                        int dx = x - cx;
                        int dy = y - cy;

                        if (dx * dx + dy * dy <= r2)
                            Set(x, y, color);
                    }
                }
            }

            public void RoundRect(
                int x,
                int y,
                int width,
                int height,
                int radius,
                Color32 color)
            {
                Rect(x + radius, y, width - radius * 2, height, color);
                Rect(x, y + radius, width, height - radius * 2, color);

                Circle(x + radius, y + radius, radius, color);
                Circle(x + width - radius - 1, y + radius, radius, color);
                Circle(x + radius, y + height - radius - 1, radius, color);
                Circle(x + width - radius - 1, y + height - radius - 1, radius, color);
            }

            public void Triangle(
                int x1,
                int y1,
                int x2,
                int y2,
                int x3,
                int y3,
                Color32 color)
            {
                Polygon(
                    new[]
                    {
                        new Vector2Int(x1, y1),
                        new Vector2Int(x2, y2),
                        new Vector2Int(x3, y3)
                    },
                    color);
            }

            public void Polygon(
                Vector2Int[] points,
                Color32 color)
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
                            if (denominator == 0)
                            {
                                j = i;
                                continue;
                            }

                            nodes[count++] =
                                pi.x +
                                (y - pi.y) *
                                (pj.x - pi.x) /
                                denominator;
                        }

                        j = i;
                    }

                    Array.Sort(nodes, 0, count);

                    for (int i = 0; i + 1 < count; i += 2)
                    {
                        for (int x = nodes[i]; x <= nodes[i + 1]; x++)
                            Set(x, y, color);
                    }
                }
            }

            public void Line(
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
                    for (int oy = -thickness / 2; oy <= thickness / 2; oy++)
                    {
                        for (int ox = -thickness / 2; ox <= thickness / 2; ox++)
                            Set(x0 + ox, y0 + oy, color);
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
        }
    }
}
#endif
