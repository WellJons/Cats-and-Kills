#if UNITY_EDITOR
using CatsAndKills.FX;
using CatsAndKills.Visual;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.EditorTools
{
    public static class ConceptVisualPolishBuilder
    {
        private const string RootName =
            "Concept Visual Polish";

        private const string LitMaterialPath =
            "Assets/CatsAndKills/Generated/IntegratedConcept/Data/ConceptSpriteLit.mat";

        public static Material GetOrCreateLitMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    LitMaterialPath);

            if (material != null)
                return material;

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Lit-Default");

            if (shader == null)
                return null;

            material =
                new Material(shader);

            AssetDatabase.CreateAsset(
                material,
                LitMaterialPath);

            return material;
        }

        public static void Apply(
            ProductionArtPack pack)
        {
            if (pack == null)
                return;

            GameObject old =
                GameObject.Find(RootName);

            if (old != null)
                Object.DestroyImmediate(old);

            GameObject root =
                new GameObject(RootName);

            DisableLegacyWallVisuals();

            Material lit =
                GetOrCreateLitMaterial();

            CreateConceptFloor(
                root.transform,
                pack);

            AddStructuralPass(
                root.transform,
                pack,
                lit);

            AddPropPass(
                root.transform,
                pack,
                lit);

            AddFloorDetailPass(
                root.transform,
                pack);

            AddLightingPass(
                root.transform,
                lit);

            AddFogPass(
                root.transform,
                pack);

            if (root.GetComponent<AlarmLighting2D>() == null)
                root.AddComponent<AlarmLighting2D>();

            ApplyMaterialToConceptSprites(
                lit);
        }

        private static void DisableLegacyWallVisuals()
        {
            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null)
                    continue;

                string n =
                    sr.gameObject.name;

                string parent =
                    sr.transform.parent != null
                        ? sr.transform.parent.name
                        : string.Empty;

                if (n == "Wall Top" ||
                    n == "Wall Side" ||
                    n == "Wall Shadow" ||
                    n == "Prop Shadow" ||
                    n == "Floor" ||
                    n.Contains("Floor Zone") ||
                    parent.Contains("Floor Zone") ||
                    n.Contains("Hazard //") ||
                    parent.Contains("Hazard //"))
                {
                    sr.enabled = false;
                }
            }
        }

        private static void CreateConceptFloor(
            Transform parent,
            ProductionArtPack pack)
        {
            if (pack.floorIndustrial == null)
                return;

            GameObject floor =
                new GameObject("Concept Floor");

            floor.transform.SetParent(
                parent,
                false);

            floor.transform.position =
                Vector3.zero;

            SpriteRenderer sr =
                floor.AddComponent<SpriteRenderer>();

            sr.sprite =
                pack.floorIndustrial;

            sr.drawMode =
                SpriteDrawMode.Simple;

            sr.color = Color.white;
            sr.sortingOrder = -1500;

            Vector2 spriteSize =
                sr.sprite.bounds.size;

            float scaleX =
                spriteSize.x > 0.001f
                    ? 46.5f / spriteSize.x
                    : 1f;

            float scaleY =
                spriteSize.y > 0.001f
                    ? 28.5f / spriteSize.y
                    : 1f;

            floor.transform.localScale =
                new Vector3(
                    scaleX,
                    scaleY,
                    1f);

            CreateFloorTintZone(
                parent,
                "Warehouse Tint",
                new Vector2(0.5f, 1.0f),
                new Vector2(13.5f, 22.0f),
                new Color(0.05f, 0.18f, 0.24f, 0.10f));

            CreateFloorTintZone(
                parent,
                "Administration Tint",
                new Vector2(16.0f, 1.5f),
                new Vector2(11.5f, 21.0f),
                new Color(0.20f, 0.05f, 0.18f, 0.08f));
        }

        private static void CreateFloorTintZone(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            Sprite square =
                GeneratedArtFactory.Get("ui_square");

            if (square == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = square;
            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size = size;
            sr.color = color;
            sr.sortingOrder = -1490;
        }

        private static void AddStructuralPass(
            Transform parent,
            ProductionArtPack pack,
            Material lit)
        {
            // Starting sector back wall.
            for (int i = 0; i < 5; i++)
            {
                CreateStructure(
                    parent,
                    "Start Back Wall " + i,
                    pack.wallStraight,
                    new Vector2(
                        -21f + i * 3.45f,
                        -2.8f),
                    new Vector2(0.98f, 0.98f),
                    0f,
                    false,
                    3480,
                    lit);
            }

            CreateStructure(
                parent,
                "Start Left Corner",
                pack.wallCorner != null
                    ? pack.wallCorner
                    : pack.wallStraight,
                new Vector2(-21.1f, -6.1f),
                new Vector2(0.95f, 0.95f),
                0f,
                false,
                3480,
                lit);

            CreateStructure(
                parent,
                "Start Right Corner",
                pack.wallCorner != null
                    ? pack.wallCorner
                    : pack.wallStraight,
                new Vector2(-7.3f, -6.1f),
                new Vector2(0.95f, 0.95f),
                0f,
                true,
                3480,
                lit);

            // Warehouse visual wall line.
            for (int i = 0; i < 4; i++)
            {
                CreateStructure(
                    parent,
                    "Warehouse Wall " + i,
                    pack.wallStraight,
                    new Vector2(
                        -6.4f + i * 3.55f,
                        4.8f),
                    new Vector2(0.96f, 0.96f),
                    0f,
                    false,
                    3460,
                    lit);
            }

            // Administration visual wall line.
            for (int i = 0; i < 4; i++)
            {
                CreateStructure(
                    parent,
                    "Admin Wall " + i,
                    pack.wallStraight,
                    new Vector2(
                        9.4f + i * 3.3f,
                        7.7f),
                    new Vector2(0.92f, 0.92f),
                    0f,
                    i % 2 == 1,
                    3440,
                    lit);
            }

            if (pack.wallDamaged != null)
            {
                CreateStructure(
                    parent,
                    "Damaged Wall",
                    pack.wallDamaged,
                    new Vector2(17.2f, 3.1f),
                    new Vector2(0.86f, 0.86f),
                    0f,
                    false,
                    3480,
                    lit);
            }
        }

        private static void AddPropPass(
            Transform parent,
            ProductionArtPack pack,
            Material lit)
        {
            CreateProp(
                parent,
                "Heavy Cover A",
                pack.crateHeavy != null
                    ? pack.crateHeavy
                    : pack.crateLight,
                new Vector2(-15.2f, -5.1f),
                0.92f,
                lit);

            CreateProp(
                parent,
                "Heavy Cover B",
                pack.crateHeavy != null
                    ? pack.crateHeavy
                    : pack.crateLight,
                new Vector2(-10.6f, -4.2f),
                0.82f,
                lit);

            CreateProp(
                parent,
                "Crate Stack A",
                pack.crateLight,
                new Vector2(-11.0f, -0.5f),
                0.72f,
                lit);

            CreateProp(
                parent,
                "Fuel Drum A",
                pack.fuelDrum,
                new Vector2(-18.4f, -3.8f),
                0.72f,
                lit);

            CreateProp(
                parent,
                "Terminal A",
                pack.terminal,
                new Vector2(-9.0f, -2.7f),
                0.78f,
                lit);

            CreateProp(
                parent,
                "Pipe Cluster A",
                pack.pipeCluster,
                new Vector2(-20.4f, -2.9f),
                0.70f,
                lit);

            CreateProp(
                parent,
                "Poster A",
                pack.propagandaPoster,
                new Vector2(-17.8f, -2.55f),
                0.52f,
                lit);

            CreateProp(
                parent,
                "Fence A",
                pack.fence,
                new Vector2(-12.6f, -7.3f),
                0.76f,
                lit);

            CreateHazardStrip(
                parent,
                "Start Door Marking",
                new Vector2(-8.25f, -3.75f),
                new Vector2(2.8f, 0.32f),
                lit);

            CreateHazardStrip(
                parent,
                "Admin Door Marking",
                new Vector2(9.95f, -0.9f),
                new Vector2(2.4f, 0.28f),
                lit);

            CreateProp(
                parent,
                "Start Lamp Cyan",
                pack.lamp,
                new Vector2(-16.2f, -2.65f),
                0.55f,
                lit);

            CreateProp(
                parent,
                "Start Lamp Red",
                pack.lamp,
                new Vector2(-9.2f, -2.65f),
                0.55f,
                lit);

            CreateProp(
                parent,
                "Warehouse Lamp",
                pack.lamp,
                new Vector2(0.8f, 4.45f),
                0.55f,
                lit);

            CreateProp(
                parent,
                "Admin Lamp",
                pack.lamp,
                new Vector2(12.8f, 7.25f),
                0.55f,
                lit);

            CreateProp(
                parent,
                "Terminal Warehouse",
                pack.terminal,
                new Vector2(5.4f, 3.7f),
                0.72f,
                lit);

            CreateProp(
                parent,
                "Pipe Warehouse",
                pack.pipeCluster,
                new Vector2(-5.6f, 4.35f),
                0.68f,
                lit);

            CreateProp(
                parent,
                "Poster Admin",
                pack.propagandaPoster,
                new Vector2(14.6f, 7.2f),
                0.48f,
                lit);

            for (int i = 0; i < 6; i++)
            {
                CreateProp(
                    parent,
                    "Debris Detail " + i,
                    pack.debris,
                    new Vector2(
                        -18f + i * 5.8f,
                        -8.3f + Mathf.Sin(i * 1.4f) * 1.3f),
                    0.32f + i % 2 * 0.08f,
                    lit);
            }
        }

        private static void AddFogPass(
            Transform parent,
            ProductionArtPack pack)
        {
            Sprite smoke =
                pack.smoke != null
                    ? pack.smoke
                    : GeneratedArtFactory.Get("smoke");

            if (smoke == null)
                return;

            CreateFogPatch(
                parent,
                "Start Low Fog",
                smoke,
                new Vector2(-15.5f, -6.5f),
                new Vector3(2.4f, 1.2f, 1f),
                new Color(0.34f, 0.28f, 0.58f, 0.16f),
                780,
                new Vector2(0.010f, 0.002f));

            CreateFogPatch(
                parent,
                "Start Neon Fog",
                smoke,
                new Vector2(-10.0f, -3.8f),
                new Vector3(1.8f, 1.0f, 1f),
                new Color(0.68f, 0.18f, 0.60f, 0.12f),
                790,
                new Vector2(-0.008f, 0.003f));

            CreateFogPatch(
                parent,
                "Warehouse Fog",
                smoke,
                new Vector2(0.0f, 1.5f),
                new Vector3(2.8f, 1.35f, 1f),
                new Color(0.28f, 0.46f, 0.70f, 0.13f),
                800,
                new Vector2(0.006f, 0.002f));

            CreateFogPatch(
                parent,
                "Admin Fog",
                smoke,
                new Vector2(14.0f, 2.8f),
                new Vector3(2.5f, 1.3f, 1f),
                new Color(0.62f, 0.18f, 0.48f, 0.12f),
                810,
                new Vector2(-0.006f, 0.003f));

            CreateFogPatch(
                parent,
                "Foreground Rolling Fog",
                smoke,
                new Vector2(-2.0f, -7.0f),
                new Vector3(3.8f, 1.8f, 1f),
                new Color(0.34f, 0.28f, 0.54f, 0.10f),
                7600,
                new Vector2(0.004f, 0.001f));
        }

        private static void CreateFogPatch(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector3 scale,
            Color color,
            int order,
            Vector2 drift)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;

            FogDrift2D fog =
                go.AddComponent<FogDrift2D>();

            fog.Configure(
                drift,
                0.12f,
                0.025f);
        }

        private static void AddFloorDetailPass(
            Transform parent,
            ProductionArtPack pack)
        {
            if (pack.bloodDrop != null)
            {
                CreateFloorDecal(
                    parent,
                    "Blood A",
                    pack.bloodDrop,
                    new Vector2(-12.8f, -5.9f),
                    new Vector2(0.85f, 0.46f),
                    -18f,
                    new Color(0.46f, 0.02f, 0.05f, 0.84f));

                CreateFloorDecal(
                    parent,
                    "Blood B",
                    pack.bloodDrop,
                    new Vector2(-4.4f, -1.5f),
                    new Vector2(0.72f, 0.38f),
                    31f,
                    new Color(0.42f, 0.015f, 0.04f, 0.78f));

                CreateFloorDecal(
                    parent,
                    "Blood C",
                    pack.bloodDrop,
                    new Vector2(6.4f, 0.4f),
                    new Vector2(0.95f, 0.48f),
                    -42f,
                    new Color(0.50f, 0.02f, 0.06f, 0.78f));

                CreateFloorDecal(
                    parent,
                    "Blood D",
                    pack.bloodDrop,
                    new Vector2(15.2f, 3.5f),
                    new Vector2(0.70f, 0.36f),
                    12f,
                    new Color(0.44f, 0.018f, 0.05f, 0.78f));
            }

            if (pack.bulletHole != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    CreateFloorDecal(
                        parent,
                        "Impact Mark " + i,
                        pack.bulletHole,
                        new Vector2(
                            -16.5f + i * 5.2f,
                            -7.6f + Mathf.Sin(i * 2.1f) * 2.0f),
                        new Vector2(0.18f, 0.18f),
                        i * 29f,
                        new Color(0.20f, 0.18f, 0.22f, 0.66f));
                }
            }
        }

        private static void CreateFloorDecal(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 scale,
            float rotation,
            Color color)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation);

            go.transform.localScale =
                new Vector3(
                    scale.x,
                    scale.y,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 1160;
        }

        private static void AddLightingPass(
            Transform parent,
            Material lit)
        {
            int defaultLayer =
                SortingLayer.NameToID("Default");

            GameObject globalGo =
                new GameObject("Global 2D Light");

            globalGo.transform.SetParent(
                parent,
                false);

            Light2D global =
                globalGo.AddComponent<Light2D>();

            global.lightType =
                Light2D.LightType.Global;

            global.color =
                new Color(
                    0.43f,
                    0.48f,
                    0.72f,
                    1f);

            global.intensity = 0.58f;
            global.targetSortingLayers =
                new[] { defaultLayer };

            CreatePointLight(
                parent,
                "Cyan Start Light",
                new Vector2(-16.2f, -4.0f),
                new Color(0.10f, 0.70f, 1.0f),
                1.05f,
                5.8f,
                false,
                defaultLayer);

            CreatePointLight(
                parent,
                "Red Start Alarm",
                new Vector2(-9.2f, -2.9f),
                new Color(1.0f, 0.08f, 0.15f),
                1.25f,
                4.2f,
                true,
                defaultLayer);

            CreatePointLight(
                parent,
                "Warehouse Cyan",
                new Vector2(0.8f, 4.5f),
                new Color(0.15f, 0.82f, 0.95f),
                1.10f,
                6.6f,
                false,
                defaultLayer);

            CreatePointLight(
                parent,
                "Warehouse Magenta",
                new Vector2(5.0f, 2.1f),
                new Color(0.95f, 0.13f, 0.72f),
                0.95f,
                5.2f,
                true,
                defaultLayer);

            CreatePointLight(
                parent,
                "Admin Red",
                new Vector2(12.8f, 5.2f),
                new Color(1.0f, 0.08f, 0.12f),
                1.22f,
                5.3f,
                true,
                defaultLayer);

            CreatePointLight(
                parent,
                "Admin Cold",
                new Vector2(18.1f, 0.8f),
                new Color(0.22f, 0.56f, 1.0f),
                0.92f,
                5.8f,
                false,
                defaultLayer);
        }

        private static void CreatePointLight(
            Transform parent,
            string name,
            Vector2 position,
            Color color,
            float intensity,
            float radius,
            bool dropout,
            int sortingLayer)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            Light2D light =
                go.AddComponent<Light2D>();

            light.lightType =
                Light2D.LightType.Point;

            light.color = color;
            light.intensity = intensity;
            light.pointLightInnerRadius =
                radius * 0.16f;

            light.pointLightOuterRadius =
                radius;

            light.falloffIntensity = 0.62f;
            light.overlapOperation =
                Light2D.OverlapOperation.Additive;

            light.targetSortingLayers =
                new[] { sortingLayer };

            GameObject glow =
                new GameObject("Glow");

            glow.transform.SetParent(
                go.transform,
                false);

            glow.transform.localScale =
                Vector3.one *
                radius *
                0.45f;

            SpriteRenderer sr =
                glow.AddComponent<SpriteRenderer>();

            sr.sprite =
                GeneratedArtFactory.Get(
                    "soft_glow");

            sr.color =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.10f);

            sr.sortingOrder = 620;

            NeonLightFlicker2D flicker =
                go.AddComponent<
                    NeonLightFlicker2D>();

            flicker.Configure(
                light,
                sr,
                intensity,
                dropout ? 0.16f : 0.06f,
                dropout ? 16f : 7f,
                dropout);
        }

        private static void CreateHazardStrip(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 scale,
            Material material)
        {
            Sprite sprite =
                GeneratedArtFactory.Get("hazard");

            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                new Vector3(
                    scale.x,
                    scale.y,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color =
                new Color(
                    0.86f,
                    0.78f,
                    0.62f,
                    0.88f);

            sr.sortingOrder = 1230;

            if (material != null)
                sr.sharedMaterial = material;
        }

        private static void CreateStructure(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 scale,
            float rotation,
            bool flipX,
            int baseOrder,
            Material material)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                new Vector3(
                    scale.x,
                    scale.y,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.flipX = flipX;
            sr.sortingOrder = 0;

            if (material != null)
                sr.sharedMaterial = material;

            DepthSortedSprite2D depth =
                go.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                baseOrder,
                -0.55f);

            ThreeQuarterOccluder2D occ =
                go.AddComponent<
                    ThreeQuarterOccluder2D>();
        }

        private static void CreateProp(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            float scale,
            Material material)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                Vector3.one * scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = 0;

            if (material != null)
                sr.sharedMaterial = material;

            DepthSortedSprite2D depth =
                go.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                3380,
                -0.20f);
        }

        private static void ApplyMaterialToConceptSprites(
            Material material)
        {
            if (material == null)
                return;

            GameObject polishRoot =
                GameObject.Find(RootName);

            Transform polishTransform =
                polishRoot != null
                    ? polishRoot.transform
                    : null;

            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null ||
                    sr.sprite == null)
                {
                    continue;
                }

                string spritePath =
                    AssetDatabase.GetAssetPath(
                        sr.sprite);

                bool generatedConcept =
                    spritePath.Contains(
                        "IntegratedConcept");

                bool sourceConcept =
                    spritePath.Contains(
                        "ConceptAtlases");

                bool conceptRuntime =
                    sr.gameObject.name.Contains(
                        "Concept Atlas Visual");

                bool stableCharacter =
                    sr.GetComponent<ThreeQuarterCharacterVisual2D>() != null ||
                    sr.GetComponentInParent<ThreeQuarterCharacterVisual2D>() != null ||
                    sr.gameObject.name.Contains("3-4 Visual");

                bool stableFloor =
                    sr.gameObject.name == "Concept Floor";

                if (stableCharacter || stableFloor)
                    continue;

                bool conceptPolish =
                    polishTransform != null &&
                    sr.transform.IsChildOf(
                        polishTransform);

                if (generatedConcept ||
                    sourceConcept ||
                    conceptRuntime ||
                    conceptPolish)
                {
                    sr.sharedMaterial =
                        material;
                }
            }
        }
    }
}
#endif
