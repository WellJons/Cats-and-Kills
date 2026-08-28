#if UNITY_EDITOR
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
            ConfigurePostFX();
            ApplyConceptFX(pack);

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
                follow.ConfigureBounds(
                    new Vector2(-23f, -14f),
                    new Vector2(23f, 14f),
                    camera);
            }
        }

        private static void ConfigurePostFX()
        {
            Camera camera = Camera.main;

            if (camera != null)
            {
                camera.allowHDR = true;

                UniversalAdditionalCameraData cameraData =
                    camera.GetComponent<UniversalAdditionalCameraData>();

                if (cameraData == null)
                {
                    cameraData =
                        camera.gameObject.AddComponent<
                            UniversalAdditionalCameraData>();
                }

                cameraData.renderPostProcessing = true;
            }

            GameObject old =
                GameObject.Find("Concept Post FX");

            if (old != null)
                Object.DestroyImmediate(old);

            string profilePath =
                ConceptArtIntegrator.GeneratedRoot +
                "/Data/ConceptPostFX.asset";

            VolumeProfile profile =
                AssetDatabase.LoadAssetAtPath<VolumeProfile>(
                    profilePath);

            if (profile == null)
            {
                profile =
                    ScriptableObject.CreateInstance<VolumeProfile>();

                AssetDatabase.CreateAsset(
                    profile,
                    profilePath);
            }

            if (!profile.TryGet(out Bloom bloom))
                bloom = profile.Add<Bloom>(true);

            bloom.active = true;
            bloom.intensity.Override(0.72f);
            bloom.threshold.Override(0.82f);
            bloom.scatter.Override(0.72f);

            if (!profile.TryGet(out Vignette vignette))
                vignette = profile.Add<Vignette>(true);

            vignette.active = true;
            vignette.intensity.Override(0.34f);
            vignette.smoothness.Override(0.58f);
            vignette.rounded.Override(false);

            if (!profile.TryGet(
                    out ColorAdjustments color))
            {
                color =
                    profile.Add<ColorAdjustments>(true);
            }

            color.active = true;
            color.postExposure.Override(-0.08f);
            color.contrast.Override(20f);
            color.saturation.Override(-4f);
            color.colorFilter.Override(
                new Color(0.90f, 0.93f, 1f));

            if (!profile.TryGet(
                    out ChromaticAberration chromatic))
            {
                chromatic =
                    profile.Add<ChromaticAberration>(true);
            }

            chromatic.active = true;
            chromatic.intensity.Override(0.035f);

            EditorUtility.SetDirty(profile);

            GameObject go =
                new GameObject("Concept Post FX");

            Volume volume =
                go.AddComponent<Volume>();

            volume.isGlobal = true;
            volume.priority = 100f;
            volume.sharedProfile = profile;
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

        private static void ApplyConceptFX(
            ProductionArtPack pack)
        {
            FXService fx =
                Object.FindAnyObjectByType<FXService>();

            if (fx == null)
                return;

            fx.bloodSprite = pack.bloodDrop;
            fx.sparkSprite = pack.spark;
            fx.bulletHoleSprite = pack.bulletHole;
            fx.explosionSprite = pack.explosion;
            fx.smokeSprite = pack.smoke;

            EditorUtility.SetDirty(fx);
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
                new Color(0.40f, 0.34f, 0.66f, 0.24f),
                700,
                new Vector2(0.018f, 0.004f));

            CreateFog(
                root.transform,
                "Background Fog B",
                magentaFog,
                new Vector2(4f, 7f),
                new Vector3(4.0f, 2.0f, 1f),
                new Color(0.72f, 0.22f, 0.66f, 0.20f),
                750,
                new Vector2(-0.012f, 0.003f));

            CreateFog(
                root.transform,
                "Foreground Fog",
                softFog,
                new Vector2(0f, -6f),
                new Vector3(5.8f, 2.3f, 1f),
                new Color(0.36f, 0.28f, 0.54f, 0.16f),
                7600,
                new Vector2(0.008f, 0.002f));

            CreateGlow(
                root.transform,
                "Cyan Practical Spill",
                cyanGlow,
                new Vector2(-13f, 6f),
                new Vector3(2.2f, 1.7f, 1f),
                new Color(0.20f, 0.82f, 1f, 0.28f),
                650);

            CreateGlow(
                root.transform,
                "Red Alarm Spill",
                redGlow,
                new Vector2(7f, 5f),
                new Vector3(2.4f, 2.0f, 1f),
                new Color(1f, 0.10f, 0.18f, 0.26f),
                680);

            CreateGlow(
                root.transform,
                "Red Alarm Spill 2",
                redGlow,
                new Vector2(-18f, 1.5f),
                new Vector3(1.9f, 1.6f, 1f),
                new Color(1f, 0.09f, 0.16f, 0.22f),
                680);

            CreateGlow(
                root.transform,
                "Overhead Cone",
                cone,
                new Vector2(-6f, 5f),
                new Vector3(2.0f, 2.0f, 1f),
                new Color(0.46f, 0.58f, 1f, 0.20f),
                620);

            CreateGlow(
                root.transform,
                "Overhead Cone 2",
                cone,
                new Vector2(12f, 3f),
                new Vector3(1.7f, 1.7f, 1f),
                new Color(0.80f, 0.20f, 0.76f, 0.18f),
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
