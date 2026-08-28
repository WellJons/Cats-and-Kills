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

            if (!ValidateCharacterArt(pack))
            {
                Debug.LogError(
                    "Character art integration is incomplete. " +
                    "Playable scene build was stopped instead of saving a broken scene.");
                return;
            }

            ThreeQuarterPlayableBuilder.BuildWithPack(
                pack,
                "generated concept");

            ApplyConceptWeaponSprites(pack);
            ConceptVisualPolishBuilder.Apply(pack);
            RebuildConceptCoverPoints();
            RebuildConceptNavigation();

            // Use one character presentation pipeline only.
            // BuildWithPack already installs the stable DirectionalSpriteSet
            // visuals. The runtime bootstrap only verifies/recreates that path.
            // Installing the direct-atlas visual here caused a second visual
            // implementation to be destroyed and replaced on the first Play frame.
            InstallRuntimeCharacterBootstrap();

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

        private static bool ValidateCharacterArt(
            ProductionArtPack pack)
        {
            return
                ValidateSet("player", pack.player) &&
                ValidateSet("pistolier", pack.pistolier) &&
                ValidateSet("rifleman", pack.rifleman) &&
                ValidateSet("machine gunner", pack.machineGunner) &&
                ValidateSet("demolitionist", pack.demolitionist);
        }

        private static bool ValidateSet(
            string label,
            DirectionalSpriteSet set)
        {
            if (set == null)
            {
                Debug.LogError(
                    "Missing DirectionalSpriteSet: " + label);
                return false;
            }

            CharacterDirection8[] directions =
            {
                CharacterDirection8.East,
                CharacterDirection8.North,
                CharacterDirection8.West,
                CharacterDirection8.South
            };

            foreach (CharacterDirection8 direction in directions)
            {
                if (set.GetIdle(direction) == null ||
                    set.GetMove(direction) == null ||
                    set.GetFire(direction) == null)
                {
                    Debug.LogError(
                        "Incomplete character sprite set: " +
                        label +
                        " / " +
                        direction);
                    return false;
                }
            }

            return true;
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

        private static void ApplyConceptWeaponSprites(
            ProductionArtPack pack)
        {
            if (pack == null)
                return;

            SetWeaponSprite(
                "Assets/CatsAndKills/Data/CK74.asset",
                pack.rifle);

            SetWeaponSprite(
                "Assets/CatsAndKills/Data/Service_Pistol.asset",
                pack.pistol);

            SetWeaponSprite(
                "Assets/CatsAndKills/Data/KS-12.asset",
                pack.shotgun);

            PlayerMotor2D player =
                Object.FindAnyObjectByType<PlayerMotor2D>();

            HitscanWeapon2D weapon =
                player != null
                    ? player.GetComponentInChildren<HitscanWeapon2D>(true)
                    : null;

            if (weapon != null &&
                weapon.Definition != null)
            {
                weapon.SetDefinition(
                    weapon.Definition,
                    false);

                EditorUtility.SetDirty(
                    weapon);
            }
        }

        private static void SetWeaponSprite(
            string assetPath,
            Sprite sprite)
        {
            if (sprite == null)
                return;

            WeaponDefinition definition =
                AssetDatabase.LoadAssetAtPath<WeaponDefinition>(
                    assetPath);

            if (definition == null)
                return;

            definition.weaponSprite =
                sprite;

            EditorUtility.SetDirty(
                definition);
        }

        private static void RebuildConceptCoverPoints()
        {
            foreach (CoverPoint point in
                     Object.FindObjectsByType<CoverPoint>(
                         FindObjectsSortMode.None))
            {
                if (point != null)
                    Object.DestroyImmediate(
                        point.gameObject);
            }

            GameObject polishRoot =
                GameObject.Find(
                    "Concept Visual Polish");

            if (polishRoot == null)
                return;

            Physics2D.SyncTransforms();

            int obstacleLayer =
                LayerMask.NameToLayer(
                    "Obstacles");

            int obstacleMask =
                obstacleLayer >= 0
                    ? 1 << obstacleLayer
                    : 0;

            foreach (BoxCollider2D obstacle in
                     polishRoot.GetComponentsInChildren<BoxCollider2D>(
                         true))
            {
                if (obstacle == null ||
                    !obstacle.enabled ||
                    obstacle.isTrigger)
                {
                    continue;
                }

                Bounds bounds =
                    obstacle.bounds;

                float clearance =
                    0.44f;

                Vector2 center =
                    bounds.center;

                Vector2[] candidates =
                {
                    new Vector2(
                        bounds.min.x - clearance,
                        center.y),
                    new Vector2(
                        bounds.max.x + clearance,
                        center.y),
                    new Vector2(
                        center.x,
                        bounds.min.y - clearance),
                    new Vector2(
                        center.x,
                        bounds.max.y + clearance)
                };

                foreach (Vector2 raw in candidates)
                {
                    Vector2 position =
                        new Vector2(
                            Mathf.Clamp(
                                raw.x,
                                -21.7f,
                                21.7f),
                            Mathf.Clamp(
                                raw.y,
                                -12.7f,
                                12.7f));

                    if (obstacleMask != 0 &&
                        Physics2D.OverlapCircle(
                            position,
                            0.22f,
                            obstacleMask) != null)
                    {
                        continue;
                    }

                    GameObject go =
                        new GameObject(
                            "Concept Cover Point");

                    go.transform.position =
                        position;

                    go.AddComponent<CoverPoint>();
                }
            }
        }

        private static void RebuildConceptNavigation()
        {
            // Prototype navigation is generated before concept walls and props
            // exist. Rebuild it now so enemies use the same collision layout
            // the player actually sees.
            Physics2D.SyncTransforms();

            foreach (NavigationGrid2D nav in
                     Object.FindObjectsByType<NavigationGrid2D>(
                         FindObjectsSortMode.None))
            {
                if (nav != null)
                    nav.Build();
            }

            foreach (CoverManager cover in
                     Object.FindObjectsByType<CoverManager>(
                         FindObjectsSortMode.None))
            {
                if (cover != null)
                    cover.Refresh();
            }
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

        private static void InstallRuntimeCharacterBootstrap()
        {
            RuntimeCharacterVisualBootstrap bootstrap =
                Object.FindAnyObjectByType<
                    RuntimeCharacterVisualBootstrap>();

            if (bootstrap == null)
            {
                GameObject go =
                    new GameObject(
                        "Runtime Character Visual Bootstrap");

                bootstrap =
                    go.AddComponent<
                        RuntimeCharacterVisualBootstrap>();
            }

            ProductionArtPack pack =
                AssetDatabase.LoadAssetAtPath<ProductionArtPack>(
                    ConceptArtIntegrator.PackPath);

            if (pack == null)
            {
                Debug.LogError(
                    "Runtime character bootstrap cannot find ProductionArtPack.");
                return;
            }

            bootstrap.Configure(
                pack.player,
                pack.pistolier,
                pack.rifleman,
                pack.machineGunner,
                pack.demolitionist);

            EditorUtility.SetDirty(bootstrap);
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
                new Vector2(112f, 80f);

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
                GameObject.Find(
                    "Concept Atmosphere");

            if (existing != null)
                Object.DestroyImmediate(
                    existing);

            GameObject root =
                new GameObject(
                    "Concept Atmosphere");

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

            Vector2[] fogPositions =
            {
                new Vector2(-39f, -23f),
                new Vector2(-31f, -7f),
                new Vector2(-24f, 12f),
                new Vector2(-8f, 1f),
                new Vector2(6f, -8f),
                new Vector2(0f, 23f),
                new Vector2(24f, 10f),
                new Vector2(29f, -18f),
                new Vector2(41f, -5f)
            };

            for (int i = 0;
                 i < fogPositions.Length;
                 i++)
            {
                bool magenta =
                    i % 3 == 1;

                CreateFog(
                    root.transform,
                    "District Fog " + i,
                    magenta
                        ? magentaFog
                        : softFog,
                    fogPositions[i],
                    magenta
                        ? new Vector3(
                            3.6f,
                            1.7f,
                            1f)
                        : new Vector3(
                            4.6f,
                            2.1f,
                            1f),
                    magenta
                        ? new Color(
                            0.56f,
                            0.20f,
                            0.54f,
                            0.12f)
                        : new Color(
                            0.28f,
                            0.33f,
                            0.55f,
                            0.14f),
                    720 +
                    i * 3,
                    new Vector2(
                        i % 2 == 0
                            ? 0.010f
                            : -0.008f,
                        0.002f));
            }

            CreateFog(
                root.transform,
                "Foreground Street Fog West",
                softFog,
                new Vector2(-31f, -17f),
                new Vector3(
                    6.2f,
                    2.4f,
                    1f),
                new Color(
                    0.32f,
                    0.28f,
                    0.50f,
                    0.11f),
                7600,
                new Vector2(
                    0.006f,
                    0.001f));

            CreateFog(
                root.transform,
                "Foreground Plaza Fog",
                softFog,
                new Vector2(1f, -4f),
                new Vector3(
                    7.0f,
                    2.6f,
                    1f),
                new Color(
                    0.30f,
                    0.26f,
                    0.48f,
                    0.10f),
                7602,
                new Vector2(
                    -0.005f,
                    0.001f));

            CreateFog(
                root.transform,
                "Foreground East Fog",
                magentaFog,
                new Vector2(28f, -6f),
                new Vector3(
                    6.0f,
                    2.3f,
                    1f),
                new Color(
                    0.50f,
                    0.17f,
                    0.44f,
                    0.09f),
                7604,
                new Vector2(
                    0.004f,
                    0.001f));

            CreateGlow(
                root.transform,
                "West Cyan Spill",
                cyanGlow,
                new Vector2(-36f, -18f),
                new Vector3(
                    2.4f,
                    1.8f,
                    1f),
                new Color(
                    0.20f,
                    0.78f,
                    1f,
                    0.20f),
                650);

            CreateGlow(
                root.transform,
                "Plaza Red Spill",
                redGlow,
                new Vector2(7f, -8f),
                new Vector3(
                    2.8f,
                    2.2f,
                    1f),
                new Color(
                    1f,
                    0.08f,
                    0.16f,
                    0.22f),
                680);

            CreateGlow(
                root.transform,
                "Warehouse Cyan Spill",
                cyanGlow,
                new Vector2(-14f, 8f),
                new Vector3(
                    2.5f,
                    1.9f,
                    1f),
                new Color(
                    0.18f,
                    0.76f,
                    1f,
                    0.19f),
                650);

            CreateGlow(
                root.transform,
                "Admin Alarm Spill",
                redGlow,
                new Vector2(14f, 9f),
                new Vector3(
                    3.0f,
                    2.3f,
                    1f),
                new Color(
                    1f,
                    0.08f,
                    0.17f,
                    0.24f),
                680);

            CreateGlow(
                root.transform,
                "Barracks Cyan Spill",
                cyanGlow,
                new Vector2(16f, -20f),
                new Vector3(
                    2.2f,
                    1.7f,
                    1f),
                new Color(
                    0.20f,
                    0.66f,
                    1f,
                    0.18f),
                650);

            CreateGlow(
                root.transform,
                "North Cone",
                cone,
                new Vector2(1f, 25f),
                new Vector3(
                    2.3f,
                    2.5f,
                    1f),
                new Color(
                    0.42f,
                    0.55f,
                    1f,
                    0.18f),
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
