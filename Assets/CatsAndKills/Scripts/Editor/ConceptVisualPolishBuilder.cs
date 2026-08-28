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

            AddStructuralPass(
                root.transform,
                pack,
                lit);

            AddPropPass(
                root.transform,
                pack,
                lit);

            AddLightingPass(
                root.transform,
                lit);

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

                if (n == "Wall Top" ||
                    n == "Wall Side" ||
                    n == "Wall Shadow")
                {
                    sr.enabled = false;
                }
            }
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

            CreateStructure(
                parent,
                "Security Door Visual",
                pack.reinforcedDoor,
                new Vector2(9.95f, 0.35f),
                new Vector2(0.88f, 0.88f),
                0f,
                false,
                3520,
                lit);

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

                bool conceptPolish =
                    sr.transform.IsChildOf(
                        GameObject.Find(RootName)?
                            .transform);

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
