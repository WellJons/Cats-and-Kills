#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.Narrative;
using CatsAndKills.UI;
using CatsAndKills.Visual;
using CatsAndKills.World;
using CatsAndKills.Tactical;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.EditorTools
{
    public static class ConceptArtPlayableBuilder
    {
        [MenuItem("Tools/Cats and Kills/Build Playable Vertical Slice")]
        public static void BuildVerticalSlice()
        {
            Build();
        }

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

            if (!ValidateGeneratedLevel())
            {
                Debug.LogError(
                    "Concept level validation failed. " +
                    "The scene was not accepted as a valid build.");
                return;
            }

            RebuildConceptCoverPoints();
            RebuildConceptNavigation();

            DistrictVerticalSliceBuilder.Apply(
                pack);

            if (!ValidateDistrictVerticalSlice())
            {
                Debug.LogError(
                    "District vertical slice validation failed. " +
                    "The scene was not saved as a playable build.");
                return;
            }

            // BuildWithPack creates the only character visual pipeline.
            // A runtime bootstrap could recreate/replace that visual after
            // Play starts, which is exactly how duplicate weapon/body layers
            // kept reappearing.
            RemoveRuntimeCharacterBootstrap();

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

        private static bool ValidateDistrictVerticalSlice()
        {
            NarrativeWorldState state =
                Object.FindAnyObjectByType<
                    NarrativeWorldState>();

            NarrativeDialogueSystem dialogue =
                Object.FindAnyObjectByType<
                    NarrativeDialogueSystem>();

            DistrictVerticalSliceDirector district =
                Object.FindAnyObjectByType<
                    DistrictVerticalSliceDirector>();

            MissionDirector mission =
                Object.FindAnyObjectByType<
                    MissionDirector>();

            CityCivilian2D[] civilians =
                Object.FindObjectsByType<
                    CityCivilian2D>(
                    FindObjectsSortMode.None);

            PropagandaPoster2D[] posters =
                Object.FindObjectsByType<
                    PropagandaPoster2D>(
                    FindObjectsSortMode.None);

            DialogueInteractable2D[] talkers =
                Object.FindObjectsByType<
                    DialogueInteractable2D>(
                    FindObjectsSortMode.None);

            TacticalUtilityBelt belt =
                Object.FindAnyObjectByType<
                    TacticalUtilityBelt>();

            TacticalOverwatchController overwatch =
                Object.FindAnyObjectByType<
                    TacticalOverwatchController>();

            CityPatrolRoute2D[] patrolRoutes =
                Object.FindObjectsByType<
                    CityPatrolRoute2D>(
                    FindObjectsSortMode.None);

            DistrictZoneTrigger2D[] zones =
                Object.FindObjectsByType<
                    DistrictZoneTrigger2D>(
                    FindObjectsSortMode.None);

            CityClubAmbience2D club =
                Object.FindAnyObjectByType<
                    CityClubAmbience2D>();

            bool valid =
                state != null &&
                dialogue != null &&
                district != null &&
                mission != null &&
                civilians.Length >= 10 &&
                posters.Length >= 3 &&
                talkers.Length >= 4 &&
                belt != null &&
                overwatch != null &&
                patrolRoutes.Length >= 4 &&
                zones.Length >= 5 &&
                club != null;

            if (!valid)
            {
                Debug.LogError(
                    "Vertical slice validation: state=" +
                    (state != null) +
                    ", dialogue=" +
                    (dialogue != null) +
                    ", district=" +
                    (district != null) +
                    ", mission=" +
                    (mission != null) +
                    ", civilians=" +
                    civilians.Length +
                    ", posters=" +
                    posters.Length +
                    ", talkers=" +
                    talkers.Length +
                    ", utilityBelt=" +
                    (belt != null) +
                    ", overwatch=" +
                    (overwatch != null) +
                    ", patrolRoutes=" +
                    patrolRoutes.Length +
                    ", zones=" +
                    zones.Length +
                    ", club=" +
                    (club != null));
            }
            else
            {
                Debug.Log(
                    "Vertical slice validated: " +
                    civilians.Length +
                    " civilians, " +
                    posters.Length +
                    " propaganda posters, " +
                    talkers.Length +
                    " dialogue NPCs, " +
                    patrolRoutes.Length +
                    " authored patrol routes, " +
                    zones.Length +
                    " named zones.");
            }

            return valid;
        }

        private static bool ValidateGeneratedLevel()
        {
            BuildingRoofFader2D[] buildings =
                Object.FindObjectsByType<BuildingRoofFader2D>(
                    FindObjectsSortMode.None);

            GameObject polish =
                GameObject.Find(
                    "Concept Visual Polish");

            int solidColliders = 0;

            if (polish != null)
            {
                foreach (BoxCollider2D collider in
                         polish.GetComponentsInChildren<BoxCollider2D>(
                             true))
                {
                    if (collider != null &&
                        collider.enabled &&
                        !collider.isTrigger)
                    {
                        solidColliders++;
                    }
                }
            }

            bool legacyFloorPresent =
                GameObject.Find("Floor") != null;

            bool legacyPanelPresent = false;
            bool legacyHazardPresent = false;

            foreach (SpriteRenderer renderer in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (renderer == null)
                    continue;

                string objectName =
                    renderer.gameObject.name;

                string spriteName =
                    renderer.sprite != null
                        ? renderer.sprite.name
                        : string.Empty;

                if (objectName.Contains("Floor Zone") ||
                    spriteName.Contains("floor_panel"))
                {
                    legacyPanelPresent = true;
                }

                if (objectName.Contains("Hazard //"))
                    legacyHazardPresent = true;
            }

            bool legacyBootstrapPresent =
                Object.FindAnyObjectByType<
                    RuntimeCharacterVisualBootstrap>() != null;

            TacticalCombatDirector tactical =
                Object.FindAnyObjectByType<
                    TacticalCombatDirector>();

            TacticalPlayerController tacticalPlayer =
                Object.FindAnyObjectByType<
                    TacticalPlayerController>();

            TacticalEnemyAgent[] tacticalEnemies =
                Object.FindObjectsByType<
                    TacticalEnemyAgent>(
                    FindObjectsSortMode.None);

            bool tacticalReady =
                tactical != null &&
                tacticalPlayer != null &&
                tacticalEnemies.Length >= 12;

            bool valid =
                polish != null &&
                buildings.Length >= 8 &&
                solidColliders >= 28 &&
                tacticalReady &&
                !legacyFloorPresent &&
                !legacyPanelPresent &&
                !legacyHazardPresent &&
                !legacyBootstrapPresent;

            if (!valid)
            {
                Debug.LogError(
                    "Generated level validation: buildings=" +
                    buildings.Length +
                    ", solidColliders=" +
                    solidColliders +
                    ", legacyFloor=" +
                    legacyFloorPresent +
                    ", legacyPanel=" +
                    legacyPanelPresent +
                    ", legacyHazard=" +
                    legacyHazardPresent +
                    ", runtimeBootstrap=" +
                    legacyBootstrapPresent +
                    ", tacticalReady=" +
                    tacticalReady +
                    ", tacticalEnemies=" +
                    tacticalEnemies.Length);

                return false;
            }

            Debug.Log(
                "Generated level validated: " +
                buildings.Length +
                " buildings, " +
                solidColliders +
                " solid environment colliders.");

            return true;
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

                if (!HasVisibleCharacterPixels(
                        set.GetIdle(direction)))
                {
                    Debug.LogError(
                        "Character body generation failed: " +
                        label +
                        " / " +
                        direction +
                        " contains too little visible sprite content.");

                    return false;
                }

                var uniqueWalkFrames =
                    new System.Collections.Generic.HashSet<Sprite>();

                for (int frame = 0;
                     frame < 8;
                     frame++)
                {
                    Sprite walk =
                        set.GetWalkFrame(
                            direction,
                            frame);

                    if (walk != null)
                        uniqueWalkFrames.Add(walk);
                }

                if (uniqueWalkFrames.Count < 4)
                {
                    Debug.LogError(
                        "Walk cycle generation failed: " +
                        label +
                        " / " +
                        direction +
                        " has only " +
                        uniqueWalkFrames.Count +
                        " unique frames.");

                    return false;
                }
            }

            return true;
        }

        private static bool HasVisibleCharacterPixels(
            Sprite sprite)
        {
            if (sprite == null ||
                sprite.texture == null)
            {
                return false;
            }

            Texture2D texture =
                sprite.texture;

            Color32[] pixels;

            try
            {
                pixels =
                    texture.GetPixels32(0);
            }
            catch
            {
                return true;
            }

            Rect rect =
                sprite.rect;

            int x0 =
                Mathf.Clamp(
                    Mathf.FloorToInt(rect.x),
                    0,
                    texture.width - 1);

            int y0 =
                Mathf.Clamp(
                    Mathf.FloorToInt(rect.y),
                    0,
                    texture.height - 1);

            int x1 =
                Mathf.Clamp(
                    Mathf.CeilToInt(rect.xMax),
                    x0 + 1,
                    texture.width);

            int y1 =
                Mathf.Clamp(
                    Mathf.CeilToInt(rect.yMax),
                    y0 + 1,
                    texture.height);

            int visible = 0;
            int area =
                Mathf.Max(
                    1,
                    (x1 - x0) *
                    (y1 - y0));

            int required =
                Mathf.Max(
                    120,
                    Mathf.RoundToInt(
                        area * 0.012f));

            for (int y = y0;
                 y < y1 &&
                 visible < required;
                 y++)
            {
                for (int x = x0;
                     x < x1;
                     x++)
                {
                    if (pixels[
                            y *
                            texture.width +
                            x].a > 32)
                    {
                        visible++;
                    }
                }
            }

            return
                visible >= required;
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
                                -46.0f,
                                46.0f),
                            Mathf.Clamp(
                                raw.y,
                                -30.0f,
                                30.0f));

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

        private static void RemoveRuntimeCharacterBootstrap()
        {
            foreach (RuntimeCharacterVisualBootstrap bootstrap in
                     Object.FindObjectsByType<RuntimeCharacterVisualBootstrap>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (bootstrap != null)
                    Object.DestroyImmediate(
                        bootstrap.gameObject);
            }
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
