#if UNITY_EDITOR
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CatsAndKills.EditorTools
{
    public static class ConceptArtPlayableBuilder
    {
        [MenuItem("Tools/Cats and Kills/Build Playable Concept-Art Version")]
        public static void Build()
        {
            ProductionArtPack pack =
                ConceptArtIntegrator.EnsureIntegratedPack();

            if (pack == null || !pack.HasMinimumPlayableArt)
            {
                Debug.LogError(
                    "Concept art pack is not ready.");
                return;
            }

            ThreeQuarterPlayableBuilder.BuildWithPack(
                pack,
                "generated concept");

            AddAtmosphere();
            ImproveCamera();
            ApplyLitSpriteMaterial();

            EditorSceneManager.MarkSceneDirty(
                EditorSceneManager.GetActiveScene());

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Cats and Kills concept-art version built. " +
                "This scene now uses the generated character, environment, " +
                "weapon and atmosphere assets.");
        }

        private static void ImproveCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            camera.orthographicSize = 4.7f;
            camera.backgroundColor =
                new Color(0.008f, 0.010f, 0.020f);

            CameraFollow2D follow =
                camera.GetComponent<CameraFollow2D>();

            if (follow != null)
            {
                // Existing follow component keeps gameplay logic.
                // The tighter framing is intentional for the 3/4 art.
            }
        }

        private static void ApplyLitSpriteMaterial()
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Lit-Default");

            if (shader == null)
                return;

            string materialPath =
                ConceptArtIntegrator.GeneratedRoot +
                "/Data/ConceptSpriteLit.mat";

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    materialPath);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(
                    material,
                    materialPath);
            }

            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null || sr.sprite == null)
                    continue;

                string path =
                    AssetDatabase.GetAssetPath(sr.sprite);

                if (path.Contains(
                        "IntegratedConcept") ||
                    sr.gameObject.name.Contains(
                        "3-4"))
                {
                    sr.sharedMaterial = material;
                }
            }
        }

        private static void AddAtmosphere()
        {
            GameObject existing =
                GameObject.Find("Concept Atmosphere");

            if (existing != null)
                Object.DestroyImmediate(existing);

            GameObject root =
                new GameObject("Concept Atmosphere");

            Sprite softFog =
                ConceptArtIntegrator.GetAmbienceSprite(
                    "fog_soft",
                    0,
                    430,
                    520,
                    270,
                    90f);

            Sprite magentaFog =
                ConceptArtIntegrator.GetAmbienceSprite(
                    "fog_magenta",
                    480,
                    430,
                    470,
                    270,
                    90f);

            Sprite cyanGlow =
                ConceptArtIntegrator.GetAmbienceSprite(
                    "cyan_glow",
                    830,
                    90,
                    235,
                    255,
                    90f);

            Sprite redGlow =
                ConceptArtIntegrator.GetAmbienceSprite(
                    "red_alarm_glow",
                    1080,
                    85,
                    300,
                    290,
                    90f);

            Sprite cone =
                ConceptArtIntegrator.GetAmbienceSprite(
                    "light_cone",
                    0,
                    115,
                    255,
                    305,
                    90f);

            CreateFog(
                root.transform,
                "Background Fog A",
                softFog,
                new Vector2(-12f, 4f),
                new Vector3(4.5f, 2.2f, 1f),
                new Color(0.36f, 0.30f, 0.58f, 0.15f),
                700,
                new Vector2(0.018f, 0.004f));

            CreateFog(
                root.transform,
                "Background Fog B",
                magentaFog,
                new Vector2(4f, 7f),
                new Vector3(4.0f, 2.0f, 1f),
                new Color(0.64f, 0.20f, 0.58f, 0.12f),
                750,
                new Vector2(-0.012f, 0.003f));

            CreateFog(
                root.transform,
                "Foreground Fog",
                softFog,
                new Vector2(0f, -6f),
                new Vector3(5.8f, 2.3f, 1f),
                new Color(0.30f, 0.24f, 0.48f, 0.10f),
                7600,
                new Vector2(0.008f, 0.002f));

            CreateGlow(
                root.transform,
                "Cyan Practical Spill",
                cyanGlow,
                new Vector2(-13f, 6f),
                new Vector3(2.2f, 1.7f, 1f),
                new Color(0.18f, 0.78f, 1f, 0.18f),
                650);

            CreateGlow(
                root.transform,
                "Red Alarm Spill",
                redGlow,
                new Vector2(7f, 5f),
                new Vector3(2.4f, 2.0f, 1f),
                new Color(1f, 0.12f, 0.18f, 0.16f),
                680);

            CreateGlow(
                root.transform,
                "Red Alarm Spill 2",
                redGlow,
                new Vector2(-18f, 1.5f),
                new Vector3(1.9f, 1.6f, 1f),
                new Color(1f, 0.10f, 0.16f, 0.14f),
                680);

            CreateGlow(
                root.transform,
                "Overhead Cone",
                cone,
                new Vector2(-6f, 5f),
                new Vector3(2.0f, 2.0f, 1f),
                new Color(0.42f, 0.53f, 1f, 0.13f),
                620);

            CreateGlow(
                root.transform,
                "Overhead Cone 2",
                cone,
                new Vector2(12f, 3f),
                new Vector3(1.7f, 1.7f, 1f),
                new Color(0.75f, 0.20f, 0.72f, 0.11f),
                620);
        }

        private static void CreateFog(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector3 scale,
            Color color,
            int order,
            Vector2 drift)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;

            FogDrift2D fog =
                go.AddComponent<FogDrift2D>();

            fog.Configure(
                drift,
                0.08f,
                0.025f);
        }

        private static void CreateGlow(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector3 scale,
            Color color,
            int order)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;

            NeonPulse2D pulse =
                go.AddComponent<NeonPulse2D>();

            pulse.Configure(
                Random.Range(0.8f, 1.6f),
                0.10f);
        }
    }
}
#endif
