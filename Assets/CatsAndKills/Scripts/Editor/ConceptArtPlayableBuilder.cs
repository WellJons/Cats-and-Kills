#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.UI;
using CatsAndKills.Visual;
using CatsAndKills.World;
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

            ConceptVisualPolishBuilder.Apply(pack);

            // The source-atlas renderer is the path that actually renders the
            // generated characters correctly. Install it AFTER the lighting
            // polish so no lit-material pass can make the cats disappear.
            InstallDirectAtlasCharacterVisuals();

            ConfigureConceptDoors();
            InstallConceptHUD(pack);

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
                // Keep the camera centered on the player at the map edges.
                // The scene backdrop covers the extra view outside the walls.
            }

            CreateWorldBackdrop();
        }

        private static void ConfigureConceptDoors()
        {
            foreach (Door2D door in
                     Object.FindObjectsByType<Door2D>(
                         FindObjectsSortMode.None))
            {
                if (door == null)
                    continue;

                door.ConfigureSlide(
                    new Vector2(
                        0f,
                        1.05f),
                    4.8f);
            }
        }

        private static void InstallConceptHUD(
            ProductionArtPack pack)
        {
            PrototypeHUD[] oldHud =
                Object.FindObjectsByType<PrototypeHUD>(
                    FindObjectsSortMode.None);

            foreach (PrototypeHUD hud in oldHud)
            {
                if (hud != null)
                    hud.enabled = false;
            }

            ConceptHUD existing =
                Object.FindAnyObjectByType<ConceptHUD>();

            if (existing == null)
            {
                existing =
                    new GameObject(
                        "Concept HUD")
                        .AddComponent<ConceptHUD>();
            }

            if (pack != null)
            {
                existing.ConfigureSkin(
                    pack.uiPortrait,
                    pack.uiObjectiveIcon,
                    pack.uiGrenadeIcon,
                    pack.uiMedkitIcon);
            }
        }

        private static void InstallDirectAtlasCharacterVisuals()
        {
            PlayerMotor2D player =
                Object.FindAnyObjectByType<PlayerMotor2D>();

            if (player == null)
                return;

            Texture2D playerAtlas =
                LoadConceptAtlas("player.png");

            ReplaceWithDirectVisual(
                player.gameObject,
                playerAtlas,
                player.transform,
                true,
                1.28f);

            EnemyBrain[] enemies =
                Object.FindObjectsByType<EnemyBrain>(
                    FindObjectsSortMode.None);

            foreach (EnemyBrain enemy in enemies)
            {
                string file =
                    enemy.Archetype switch
                    {
                        EnemyArchetype.Pistolier =>
                            "pistolier.png",

                        EnemyArchetype.MachineGunner =>
                            "machinegunner.png",

                        EnemyArchetype.Demolitionist =>
                            "demolitionist.png",

                        _ =>
                            "rifleman.png"
                    };

                float scale =
                    enemy.Archetype ==
                    EnemyArchetype.MachineGunner
                        ? 1.40f
                        : 1.22f;

                ReplaceWithDirectVisual(
                    enemy.gameObject,
                    LoadConceptAtlas(file),
                    player.transform,
                    false,
                    scale);
            }
        }

        private static Texture2D LoadConceptAtlas(
            string file)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(
                ConceptArtIntegrator.AtlasRoot +
                "/" +
                file);
        }

        private static void ReplaceWithDirectVisual(
            GameObject root,
            Texture2D atlas,
            Transform lookTarget,
            bool isPlayer,
            float scale)
        {
            if (atlas == null || root == null)
                return;

            Transform oldVisual =
                root.transform.Find(
                    isPlayer
                        ? "Player 3-4 Visual"
                        : "Enemy 3-4 Visual");

            if (oldVisual != null)
                Object.DestroyImmediate(
                    oldVisual.gameObject);

            Transform existing =
                root.transform.Find(
                    "Concept Atlas Visual");

            if (existing != null)
                Object.DestroyImmediate(
                    existing.gameObject);

            GameObject go =
                new GameObject(
                    "Concept Atlas Visual");

            go.transform.SetParent(
                root.transform,
                false);

            go.transform.localPosition =
                Vector3.zero;

            go.transform.localScale =
                Vector3.one * scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.color = Color.white;
            sr.enabled = true;
            sr.sortingOrder = 10;

            Shader spriteShader =
                Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                sr.sharedMaterial =
                    new Material(spriteShader)
                    {
                        hideFlags =
                            HideFlags.HideAndDontSave
                    };
            }

            CharacterVitals vitals =
                root.GetComponent<CharacterVitals>();

            Rigidbody2D body =
                root.GetComponent<Rigidbody2D>();

            PlayerAim2D aim =
                isPlayer
                    ? root.GetComponent<PlayerAim2D>()
                    : null;

            HitscanWeapon2D playerGun =
                isPlayer
                    ? root.GetComponentInChildren<
                        HitscanWeapon2D>()
                    : null;

            EnemyWeapon2D enemyGun =
                !isPlayer
                    ? root.GetComponent<EnemyWeapon2D>()
                    : null;

            ConceptAtlasCharacterVisual2D visual =
                go.AddComponent<
                    ConceptAtlasCharacterVisual2D>();

            visual.Configure(
                atlas,
                sr,
                vitals,
                body,
                aim,
                playerGun,
                enemyGun,
                isPlayer ? null : lookTarget,
                128f);

            DepthSortedSprite2D depth =
                go.AddComponent<DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                5000,
                0f);

            if (isPlayer && aim != null)
                aim.SetBodyRotationEnabled(false);
        }

        private static void CreateWorldBackdrop()
        {
            GameObject existing =
                GameObject.Find("World Backdrop");

            if (existing != null)
                Object.DestroyImmediate(existing);

            GameObject go =
                new GameObject("World Backdrop");

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite =
                GeneratedArtFactory.Get("ui_square");

            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size =
                new Vector2(72f, 52f);

            sr.color =
                new Color(
                    0.018f,
                    0.021f,
                    0.034f,
                    1f);

            sr.sortingOrder = -2000;
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
            fx.casingSprite = pack.casing;
            fx.bulletHoleSprite = pack.bulletHole;
            fx.explosionSprite = pack.explosion;
            fx.smokeSprite = pack.smoke;

            if (pack.muzzleFlash != null)
            {
                foreach (MuzzleFlash2D flash in
                         Object.FindObjectsByType<MuzzleFlash2D>(
                             FindObjectsSortMode.None))
                {
                    SpriteRenderer sr =
                        flash.GetComponent<SpriteRenderer>();

                    if (sr == null)
                        sr =
                            flash.GetComponentInChildren<
                                SpriteRenderer>();

                    if (sr != null)
                        sr.sprite = pack.muzzleFlash;
                }
            }

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
