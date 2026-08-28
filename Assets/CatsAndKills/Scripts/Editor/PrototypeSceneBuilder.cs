#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Audio;
using CatsAndKills.Combat;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.Tactical;
using CatsAndKills.UI;
using CatsAndKills.Visual;
using CatsAndKills.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BodyPart = CatsAndKills.Damage.BodyPart;

namespace CatsAndKills.EditorTools
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/CatsAndKills/Scenes/PlayableCombatSandbox.unity";
        private const string DataFolder = "Assets/CatsAndKills/Data";
        private const string ObstacleLayerName = "Obstacles";

        private static int _obstacleLayer;
        private static Sprite _square;
        private static Sprite _circle;
        private static Sprite _softShadowSprite;
        private static Sprite _softGlowSprite;
        private static Sprite _hazardSprite;
        private static Sprite _floorPanelSprite;
        private static Sprite _catHead;
        private static Sprite _enemyHead;
        private static Sprite _torsoSprite;
        private static Sprite _armSprite;
        private static Sprite _legSprite;
        private static Sprite _rifleSprite;
        private static Sprite _pistolSprite;
        private static Sprite _shotgunSprite;
        private static Sprite _machineGunSprite;
        private static Sprite _grenadeSprite;
        private static Sprite _floorSprite;
        private static Sprite _wallSprite;
        private static Sprite _crateSprite;
        private static Sprite _barrelSprite;
        private static Sprite _doorSprite;
        private static Sprite _muzzleSprite;
        private static Sprite _bloodSprite;
        private static Sprite _sparkSprite;
        private static Sprite _casingSprite;
        private static Sprite _bulletHoleSprite;
        private static Sprite _smokeSprite;
        private static Sprite _explosionSprite;

        [MenuItem("Tools/Cats and Kills/Build Playable v0.1 Sandbox")]
        public static void Build()
        {
            EnsureFolder("Assets/CatsAndKills/Scenes");
            EnsureFolder(DataFolder);
            _obstacleLayer = EnsureLayer(ObstacleLayerName);

            GeneratedArtFactory.RegenerateAll();

            _square = GeneratedArtFactory.Get("ui_square");
            _circle = GeneratedArtFactory.Get("ui_circle");
            _softShadowSprite = GeneratedArtFactory.Get("soft_shadow");
            _softGlowSprite = GeneratedArtFactory.Get("soft_glow");
            _hazardSprite = GeneratedArtFactory.Get("hazard");
            _floorPanelSprite = GeneratedArtFactory.Get("floor_panel");
            _catHead = GeneratedArtFactory.Get("cat_head");
            _enemyHead = GeneratedArtFactory.Get("enemy_head");
            _torsoSprite = GeneratedArtFactory.Get("torso");
            _armSprite = GeneratedArtFactory.Get("arm");
            _legSprite = GeneratedArtFactory.Get("leg");
            _rifleSprite = GeneratedArtFactory.Get("rifle");
            _pistolSprite = GeneratedArtFactory.Get("pistol");
            _shotgunSprite = GeneratedArtFactory.Get("shotgun");
            _machineGunSprite = GeneratedArtFactory.Get("machinegun");
            _grenadeSprite = GeneratedArtFactory.Get("grenade");
            _floorSprite = GeneratedArtFactory.Get("floor");
            _wallSprite = GeneratedArtFactory.Get("wall");
            _crateSprite = GeneratedArtFactory.Get("crate");
            _barrelSprite = GeneratedArtFactory.Get("barrel");
            _doorSprite = GeneratedArtFactory.Get("door");
            _muzzleSprite = GeneratedArtFactory.Get("muzzle");
            _bloodSprite = GeneratedArtFactory.Get("blood");
            _sparkSprite = GeneratedArtFactory.Get("spark");
            _casingSprite = GeneratedArtFactory.Get("casing");
            _bulletHoleSprite = GeneratedArtFactory.Get("bullet_hole");
            _smokeSprite = GeneratedArtFactory.Get("smoke");
            _explosionSprite = GeneratedArtFactory.Get("explosion");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSystems();

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.2f;
            camera.backgroundColor = new Color(0.025f, 0.03f, 0.045f);
            cameraGo.transform.position = new Vector3(-16f, -7f, -10f);
            var cameraFollow = cameraGo.AddComponent<CameraFollow2D>();

            CreateFloor();
            CreateGeometry();

            CreatePickup(
                "West Gate Ammo Cache",
                new Vector2(-36f, -20f),
                PickupType.Ammo,
                55,
                new Color(0.25f, 0.65f, 0.95f));

            CreatePickup(
                "Warehouse Field Medkit",
                new Vector2(-24f, 10f),
                PickupType.Medkit,
                42,
                new Color(0.25f, 0.9f, 0.45f));

            CreatePickup(
                "Central Grenade Crate",
                new Vector2(1f, -2f),
                PickupType.Grenades,
                2,
                new Color(0.95f, 0.65f, 0.15f));

            CreatePickup(
                "Admin Ammo Cache",
                new Vector2(27f, 10f),
                PickupType.Ammo,
                50,
                new Color(0.25f, 0.65f, 0.95f));

            CreatePickup(
                "Barracks Medkit",
                new Vector2(25f, -18f),
                PickupType.Medkit,
                36,
                new Color(0.25f, 0.9f, 0.45f));

            CreateCheckpoint(
                "PLAZA",
                new Vector2(-7f, -8f),
                new Vector2(-8f, -9f));

            CreateCheckpoint(
                "WAREHOUSE",
                new Vector2(-18f, 1f),
                new Vector2(-19f, 0f));

            CreateCheckpoint(
                "ADMIN",
                new Vector2(15f, 2f),
                new Vector2(14f, 1f));

            var navGo =
                new GameObject("Navigation Grid");

            var nav =
                navGo.AddComponent<NavigationGrid2D>();

            nav.Configure(
                new Vector2(96f, 64f),
                0.85f,
                0.29f,
                1 << _obstacleLayer);

            var coverManagerGo =
                new GameObject("Cover Manager");

            var coverManager =
                coverManagerGo.AddComponent<CoverManager>();

            coverManager.Configure(
                1 << _obstacleLayer);

            CreateCoverPoints();

            var player =
                CreatePlayer(
                    new Vector2(-43f, -24f),
                    camera,
                    cameraFollow);

            TacticalCombatDirector tactical =
                Object.FindAnyObjectByType<TacticalCombatDirector>();

            PlayerMotor2D playerMotor =
                player.GetComponent<PlayerMotor2D>();

            HitscanWeapon2D playerWeapon =
                player.GetComponentInChildren<HitscanWeapon2D>(
                    true);

            PlayerGrenadeController playerGrenades =
                player.GetComponent<PlayerGrenadeController>();

            TacticalUtilityBelt utilityBelt =
                player.GetComponent<TacticalUtilityBelt>();

            if (utilityBelt == null)
                utilityBelt =
                    player.AddComponent<TacticalUtilityBelt>();

            utilityBelt.Configure(
                nav,
                _grenadeSprite,
                _smokeSprite);

            TacticalOverwatchController overwatch =
                player.GetComponent<TacticalOverwatchController>();

            if (overwatch == null)
                overwatch =
                    player.AddComponent<TacticalOverwatchController>();

            overwatch.Configure(
                playerWeapon,
                1 << _obstacleLayer);

            PlayerAim2D playerAim =
                player.GetComponent<PlayerAim2D>();

            tactical?.Configure(
                playerMotor,
                nav);

            TacticalPlayerController tacticalPlayer =
                player.AddComponent<TacticalPlayerController>();

            tacticalPlayer.Configure(
                nav,
                tactical,
                playerWeapon,
                playerGrenades,
                utilityBelt,
                overwatch,
                playerAim,
                camera);

            GameObject tacticalHudGo =
                new GameObject("Tactical HUD");

            tacticalHudGo.AddComponent<TacticalHUD>();

            GameObject gridOverlayGo =
                new GameObject("Tactical Grid Overlay");

            TacticalGridOverlay2D gridOverlay =
                gridOverlayGo.AddComponent<TacticalGridOverlay2D>();

            gridOverlay.Configure(
                nav,
                player.transform,
                tactical);

            var squadGate =
                new GameObject(
                    "Squad A // West Gate")
                    .AddComponent<SquadController>();

            var squadPlaza =
                new GameObject(
                    "Squad B // Central Plaza")
                    .AddComponent<SquadController>();

            var squadWarehouse =
                new GameObject(
                    "Squad C // Warehouse")
                    .AddComponent<SquadController>();

            var squadNorth =
                new GameObject(
                    "Squad D // North Alley")
                    .AddComponent<SquadController>();

            var squadAdmin =
                new GameObject(
                    "Squad E // Administration")
                    .AddComponent<SquadController>();

            var squadBarracks =
                new GameObject(
                    "Squad F // Barracks")
                    .AddComponent<SquadController>();

            var squadWorkshop =
                new GameObject(
                    "Squad G // Workshop")
                    .AddComponent<SquadController>();

            // West approach: first contact is spread across road and side lane.
            CreateEnemy("Gate Pistolier 01", new Vector2(-36f, -23f), player.transform, nav, coverManager, squadGate, EnemyArchetype.Pistolier);
            CreateEnemy("Gate Rifleman 01", new Vector2(-33f, -19f), player.transform, nav, coverManager, squadGate, EnemyArchetype.Rifleman);
            CreateEnemy("Gate Rifleman 02", new Vector2(-30f, -25f), player.transform, nav, coverManager, squadGate, EnemyArchetype.Rifleman);
            CreateEnemy("Gate Rifleman 03", new Vector2(-27f, -20f), player.transform, nav, coverManager, squadGate, EnemyArchetype.Rifleman);

            // Central square: open combat with crossfire and a suppressor.
            CreateEnemy("Plaza Rifleman 01", new Vector2(-8f, -4f), player.transform, nav, coverManager, squadPlaza, EnemyArchetype.Rifleman);
            CreateEnemy("Plaza Rifleman 02", new Vector2(-3f, 2f), player.transform, nav, coverManager, squadPlaza, EnemyArchetype.Rifleman);
            CreateEnemy("Plaza Rifleman 03", new Vector2(5f, -5f), player.transform, nav, coverManager, squadPlaza, EnemyArchetype.Rifleman);
            CreateEnemy("Plaza Machine Gunner", new Vector2(7f, 3f), player.transform, nav, coverManager, squadPlaza, EnemyArchetype.MachineGunner);
            CreateEnemy("Plaza Demolitionist", new Vector2(1f, 6f), player.transform, nav, coverManager, squadPlaza, EnemyArchetype.Demolitionist);

            // Warehouse interior and loading apron.
            CreateEnemy("Warehouse Rifleman 01", new Vector2(-27f, 7f), player.transform, nav, coverManager, squadWarehouse, EnemyArchetype.Rifleman);
            CreateEnemy("Warehouse Rifleman 02", new Vector2(-21f, 13f), player.transform, nav, coverManager, squadWarehouse, EnemyArchetype.Rifleman);
            CreateEnemy("Warehouse Pistolier", new Vector2(-16f, 7f), player.transform, nav, coverManager, squadWarehouse, EnemyArchetype.Pistolier);
            CreateEnemy("Warehouse Machine Gunner", new Vector2(-28f, 17f), player.transform, nav, coverManager, squadWarehouse, EnemyArchetype.MachineGunner);
            CreateEnemy("Warehouse Demolitionist", new Vector2(-14f, 15f), player.transform, nav, coverManager, squadWarehouse, EnemyArchetype.Demolitionist);

            // North alley can flank both warehouse and administration.
            CreateEnemy("North Rifleman 01", new Vector2(-8f, 22f), player.transform, nav, coverManager, squadNorth, EnemyArchetype.Rifleman);
            CreateEnemy("North Rifleman 02", new Vector2(1f, 24f), player.transform, nav, coverManager, squadNorth, EnemyArchetype.Rifleman);
            CreateEnemy("North Pistolier", new Vector2(9f, 21f), player.transform, nav, coverManager, squadNorth, EnemyArchetype.Pistolier);
            CreateEnemy("North Rifleman 03", new Vector2(15f, 25f), player.transform, nav, coverManager, squadNorth, EnemyArchetype.Rifleman);

            // Administration: denser defensive group.
            CreateEnemy("Admin Rifleman 01", new Vector2(19f, 6f), player.transform, nav, coverManager, squadAdmin, EnemyArchetype.Rifleman);
            CreateEnemy("Admin Rifleman 02", new Vector2(25f, 13f), player.transform, nav, coverManager, squadAdmin, EnemyArchetype.Rifleman);
            CreateEnemy("Admin Rifleman 03", new Vector2(31f, 6f), player.transform, nav, coverManager, squadAdmin, EnemyArchetype.Rifleman);
            CreateEnemy("Admin Machine Gunner", new Vector2(30f, 17f), player.transform, nav, coverManager, squadAdmin, EnemyArchetype.MachineGunner);
            CreateEnemy("Admin Demolitionist", new Vector2(20f, 17f), player.transform, nav, coverManager, squadAdmin, EnemyArchetype.Demolitionist);

            // Southern barracks.
            CreateEnemy("Barracks Rifleman 01", new Vector2(19f, -20f), player.transform, nav, coverManager, squadBarracks, EnemyArchetype.Rifleman);
            CreateEnemy("Barracks Rifleman 02", new Vector2(27f, -15f), player.transform, nav, coverManager, squadBarracks, EnemyArchetype.Rifleman);
            CreateEnemy("Barracks Pistolier", new Vector2(31f, -22f), player.transform, nav, coverManager, squadBarracks, EnemyArchetype.Pistolier);
            CreateEnemy("Barracks Demolitionist", new Vector2(21f, -12f), player.transform, nav, coverManager, squadBarracks, EnemyArchetype.Demolitionist);

            // South-west workshop / service yard.
            CreateEnemy("Workshop Rifleman 01", new Vector2(-26f, -14f), player.transform, nav, coverManager, squadWorkshop, EnemyArchetype.Rifleman);
            CreateEnemy("Workshop Rifleman 02", new Vector2(-20f, -18f), player.transform, nav, coverManager, squadWorkshop, EnemyArchetype.Rifleman);
            CreateEnemy("Workshop Pistolier", new Vector2(-15f, -13f), player.transform, nav, coverManager, squadWorkshop, EnemyArchetype.Pistolier);
            CreateEnemy("Workshop Machine Gunner", new Vector2(-18f, -24f), player.transform, nav, coverManager, squadWorkshop, EnemyArchetype.MachineGunner);

            var responseSquad =
                new GameObject(
                    "Squad H // Response")
                    .AddComponent<SquadController>();

            var reinforcementUnits =
                new[]
                {
                    CreateEnemy("Response Rifle 01", new Vector2(42f, -27f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Rifleman),
                    CreateEnemy("Response Rifle 02", new Vector2(44f, -23f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Rifleman),
                    CreateEnemy("Response Rifle 03", new Vector2(40f, -19f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Rifleman),
                    CreateEnemy("Response Pistolier", new Vector2(43f, -15f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Pistolier),
                    CreateEnemy("Response MG", new Vector2(45f, -26f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.MachineGunner),
                    CreateEnemy("Response Demo", new Vector2(39f, -28f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Demolitionist)
                };

            var reinforcementDirector =
                new GameObject(
                    "Reinforcement Director")
                    .AddComponent<ReinforcementDirector>();

            reinforcementDirector.Configure(
                reinforcementUnits);

            coverManager.Refresh();
            nav.Build();
            cameraFollow.Configure(
                player.transform,
                player.GetComponent<PlayerAim2D>());

            cameraFollow.ConfigureBounds(
                new Vector2(-48f, -32f),
                new Vector2(48f, 32f),
                camera);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            Debug.Log("Cats and Kills playable sandbox created: " + ScenePath);
        }

        private static void CreateSystems()
        {
            new GameObject("Combat Director").AddComponent<CombatDirector>();
            new GameObject("Tactical Combat").AddComponent<TacticalCombatDirector>();
            new GameObject("Facility Alarm").AddComponent<FacilityAlarmDirector>();
            new GameObject("Combat Stats").AddComponent<CombatStats>();
            new GameObject("Haptics").AddComponent<HapticsManager>();
            new GameObject("Radio Dialogue").AddComponent<RadioDialogueSystem>();
            new GameObject("World Callouts").AddComponent<WorldCalloutSystem>();
            new GameObject("Runtime Game Menu").AddComponent<RuntimeGameMenu>();
            new GameObject("Propaganda Broadcast").AddComponent<PropagandaBroadcast2D>();

            var fxGo = new GameObject("FX Service");
            var fx = fxGo.AddComponent<FXService>();
            fx.bloodSprite = _bloodSprite;
            fx.sparkSprite = _sparkSprite;
            fx.casingSprite = _casingSprite;
            fx.bulletHoleSprite = _bulletHoleSprite;
            fx.explosionSprite = _explosionSprite;
            fx.smokeSprite = _smokeSprite;

            var musicGo = new GameObject("Adaptive Music");
            var music = musicGo.AddComponent<AdaptiveMusicDirector>();
            music.Configure(null, null, null);
        }

        private static GameObject CreatePlayer(Vector2 position, Camera camera, CameraFollow2D cameraFollow)
        {
            var root = new GameObject("Player");
            root.tag = "Player";
            root.transform.position = position;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var bodyRenderer = root.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = _circle;
            bodyRenderer.enabled = false;

            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.36f;

            var vitals = root.AddComponent<CharacterVitals>();
            vitals.Configure(115f, 50f, 58f);

            Transform bodyRig = CreateModularRig(
                root,
                vitals,
                false,
                new Color(0.90f, 0.92f, 0.97f));

            CreateActorShadow(root.transform, new Vector2(1.05f, 0.62f), 6);

            var motor = root.AddComponent<PlayerMotor2D>();
            root.AddComponent<PlayerInteraction2D>();
            root.AddComponent<PlayerNoiseEmitter2D>();
            var suppressionFeedback = root.AddComponent<PlayerSuppression2D>();
            suppressionFeedback.Configure(cameraFollow);

            var damageFeedback = root.AddComponent<PlayerDamageFeedback2D>();
            damageFeedback.Configure(vitals, cameraFollow);

            var aim = root.AddComponent<PlayerAim2D>();
            var death = root.AddComponent<PlayerDeathController>();
            death.Configure(vitals);
            var collar = root.AddComponent<CollarAbility>();

            var aimPivot = new GameObject("Aim Pivot");
            aimPivot.transform.SetParent(root.transform, false);
            aim.Configure(camera, aimPivot.transform, bodyRig);

            var weaponGo = new GameObject("Weapon");
            weaponGo.transform.SetParent(aimPivot.transform, false);
            weaponGo.transform.localPosition = new Vector3(0.72f, 0f, 0f);
            weaponGo.transform.localScale = Vector3.one * 0.58f;

            var weaponRenderer = weaponGo.AddComponent<SpriteRenderer>();
            weaponRenderer.sprite = _rifleSprite;
            weaponRenderer.color = Color.white;
            weaponRenderer.sortingOrder = 20;

            var visualRecoil = weaponGo.AddComponent<WeaponVisualRecoil2D>();

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(weaponGo.transform, false);
            muzzle.transform.localPosition = new Vector3(1.02f, 0f, 0f);

            var flashGo = new GameObject("Muzzle Flash");
            flashGo.transform.SetParent(muzzle.transform, false);
            var flashRenderer = flashGo.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = _muzzleSprite;
            flashRenderer.color = Color.white;
            flashRenderer.sortingOrder = 30;
            flashGo.transform.localScale = Vector3.one * 0.62f;
            var flash = flashGo.AddComponent<MuzzleFlash2D>();
            flash.Configure(flashRenderer);

            var casing = new GameObject("Casing Port");
            casing.transform.SetParent(weaponGo.transform, false);
            casing.transform.localPosition = new Vector3(0f, -0.35f, 0f);

            var audio = weaponGo.AddComponent<AudioSource>();
            audio.spatialBlend = 0.35f;

            var rifle = CreateWeaponDefinition("CK74", true, 34f, 9.5f, 30, 120, 0.4f, 1.2f, 1f, 6.5f, 1);
            var pistol = CreateWeaponDefinition("Service Pistol", false, 46f, 4.2f, 12, 60, 0.35f, 0.8f, 1.5f, 5f, 1);
            var shotgun = CreateWeaponDefinition("KS-12", false, 15f, 1.15f, 6, 30, 3.2f, 4.3f, 2.6f, 9f, 8);

            rifle.shotClip = null;
            pistol.shotClip = null;
            shotgun.shotClip = null;
            rifle.reloadClip = null;
            pistol.reloadClip = null;
            shotgun.reloadClip = null;

            var weapon = weaponGo.AddComponent<HitscanWeapon2D>();
            weapon.Configure(rifle, aim, motor, muzzle.transform, casing.transform, cameraFollow, weaponRenderer, visualRecoil, flash, audio);
            visualRecoil.ConfigureAnchor(
                root.transform,
                aim,
                weaponRenderer,
                weapon);

            var arsenal = root.AddComponent<PlayerArsenal>();
            arsenal.Configure(weapon, new[] { rifle, pistol, shotgun });

            var grenadeController = root.AddComponent<PlayerGrenadeController>();
            grenadeController.Configure(
                aim,
                _grenadeSprite,
                null,
                null);

            collar.Configure(null);

            var hudGo = new GameObject("Prototype HUD");
            var hud = hudGo.AddComponent<PrototypeHUD>();
            hud.Configure(vitals, arsenal, grenadeController, collar, null);
            hud.BindSuppression(suppressionFeedback);

            var crosshairGo = new GameObject("Crosshair UI");
            var crosshair = crosshairGo.AddComponent<CrosshairUI>();
            crosshair.Configure(aim);

            var promptGo = new GameObject("Interaction Prompt UI");
            var prompt = promptGo.AddComponent<InteractionPromptUI>();
            prompt.Configure(root.transform);

            return root;
        }

        private static Transform CreateModularRig(
            GameObject root,
            CharacterVitals vitals,
            bool enemy,
            Color tint)
        {
            var visualRoot = new GameObject("Body Rig");
            visualRoot.transform.SetParent(root.transform, false);

            // Gameplay roots rotate for aiming, but pseudo-isometric body
            // hitboxes must stay upright on screen like the rendered cat.
            visualRoot.AddComponent<WorldUpright2D>();

            var limbBindings = new System.Collections.Generic.List<ModularCharacter2D.LimbBinding>();
            var tintTargets = new System.Collections.Generic.List<SpriteRenderer>();

            GameObject torso = CreateBodyPart(
                visualRoot.transform,
                "Torso",
                _torsoSprite,
                new Vector2(0f, 0.68f),
                new Vector2(0.70f, 0.84f),
                tint,
                10);

            var torsoCollider = torso.AddComponent<BoxCollider2D>();
            torsoCollider.size = new Vector2(0.90f, 0.98f);
            torsoCollider.isTrigger = true;

            var torsoHit = torso.AddComponent<BodyPartHitbox>();
            torsoHit.Configure(vitals, BodyPart.Torso, 1f);

            tintTargets.Add(torso.GetComponent<SpriteRenderer>());

            GameObject head = CreateBodyPart(
                visualRoot.transform,
                "Head",
                enemy ? _enemyHead : _catHead,
                new Vector2(0f, 1.34f),
                new Vector2(0.62f, 0.62f),
                Color.white,
                13);

            var headCollider = head.AddComponent<CircleCollider2D>();
            headCollider.radius = 0.46f;
            headCollider.isTrigger = true;

            var headHit = head.AddComponent<BodyPartHitbox>();
            headHit.Configure(vitals, BodyPart.Head, 1.10f);

            GameObject leftArm = CreateBodyPart(
                visualRoot.transform,
                "Left Arm",
                _armSprite,
                new Vector2(-0.38f, 0.72f),
                new Vector2(0.60f, 0.68f),
                tint,
                11);

            GameObject rightArm = CreateBodyPart(
                visualRoot.transform,
                "Right Arm",
                _armSprite,
                new Vector2(0.38f, 0.72f),
                new Vector2(0.60f, 0.68f),
                tint,
                11);

            GameObject leftLeg = CreateBodyPart(
                visualRoot.transform,
                "Left Leg",
                _legSprite,
                new Vector2(-0.18f, 0.18f),
                new Vector2(0.54f, 0.62f),
                tint * 0.82f,
                8);

            GameObject rightLeg = CreateBodyPart(
                visualRoot.transform,
                "Right Leg",
                _legSprite,
                new Vector2(0.18f, 0.18f),
                new Vector2(0.54f, 0.62f),
                tint * 0.82f,
                8);

            limbBindings.Add(AddLimbHitbox(leftArm, vitals, BodyPart.LeftArm));
            limbBindings.Add(AddLimbHitbox(rightArm, vitals, BodyPart.RightArm));
            limbBindings.Add(AddLimbHitbox(leftLeg, vitals, BodyPart.LeftLeg));
            limbBindings.Add(AddLimbHitbox(rightLeg, vitals, BodyPart.RightLeg));

            tintTargets.Add(leftArm.GetComponent<SpriteRenderer>());
            tintTargets.Add(rightArm.GetComponent<SpriteRenderer>());
            tintTargets.Add(leftLeg.GetComponent<SpriteRenderer>());
            tintTargets.Add(rightLeg.GetComponent<SpriteRenderer>());

            var modular = visualRoot.AddComponent<ModularCharacter2D>();
            modular.Configure(vitals, limbBindings, tintTargets.ToArray());

            return visualRoot.transform;
        }

        private static GameObject CreateBodyPart(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 localPosition,
            Vector2 scale,
            Color color,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            return go;
        }

        private static ModularCharacter2D.LimbBinding AddLimbHitbox(
            GameObject part,
            CharacterVitals vitals,
            BodyPart bodyPart)
        {
            var col = part.AddComponent<BoxCollider2D>();

            col.size =
                bodyPart == BodyPart.LeftArm ||
                bodyPart == BodyPart.RightArm
                    ? new Vector2(0.58f, 0.92f)
                    : new Vector2(0.52f, 0.78f);

            col.isTrigger = true;

            var hit = part.AddComponent<BodyPartHitbox>();
            hit.Configure(vitals, bodyPart, 0.90f);

            return new ModularCharacter2D.LimbBinding
            {
                part = bodyPart,
                visual = part.transform,
                hitbox = col
            };
        }

        private static WeaponDefinition CreateWeaponDefinition(
            string name,
            bool automatic,
            float damage,
            float rate,
            int magazine,
            int reserve,
            float spread,
            float movingSpread,
            float recoil,
            float recoilMax,
            int pellets)
        {
            string safe = name.Replace(" ", "_");
            string path = DataFolder + "/" + safe + ".asset";
            var def = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);

            if (def == null)
            {
                def = ScriptableObject.CreateInstance<WeaponDefinition>();
                AssetDatabase.CreateAsset(def, path);
            }

            def.weaponName = name;

            string lowerName = name.ToLowerInvariant();
            def.weaponSprite =
                lowerName.Contains("pistol")
                    ? _pistolSprite
                    : lowerName.Contains("ks-12")
                        ? _shotgunSprite
                        : _rifleSprite;
            def.automatic = automatic;
            def.damage = damage;
            def.range = pellets > 1 ? 15f : 28f;
            def.fireRate = rate;
            def.pellets = pellets;
            def.magazineSize = magazine;
            def.startingReserve = reserve;
            def.baseSpread = spread;
            def.movingSpread = movingSpread;
            def.recoilPerShot = recoil;
            def.recoilMax = recoilMax;
            def.recoilRecovery = automatic ? 8.5f : 12f;
            def.recoilHorizontal = automatic ? 0.38f : 0.65f;

            bool isPistol =
                lowerName.Contains("pistol");

            bool isShotgun =
                lowerName.Contains("ks-12") ||
                pellets > 1;

            def.impactForce =
                isShotgun
                    ? 6.2f
                    : isPistol
                        ? 2.7f
                        : 4.2f;

            def.dismemberPower =
                isShotgun
                    ? 0.92f
                    : isPistol
                        ? 0.08f
                        : 0.24f;

            def.reloadTime =
                isShotgun
                    ? 2.35f
                    : isPistol
                        ? 1.25f
                        : 1.65f;

            def.visualKickDistance = pellets > 1 ? 0.20f : 0.10f;
            def.visualKickRotation = pellets > 1 ? 7f : 3.5f;
            def.cameraKick = pellets > 1 ? 0.22f : 0.10f;
            def.cameraKickDecay = 22f;
            def.rumbleLow = pellets > 1 ? 0.42f : 0.18f;
            def.rumbleHigh = pellets > 1 ? 0.68f : 0.32f;
            def.rumbleDuration = pellets > 1 ? 0.1f : 0.055f;
            def.hitMask = ~0;

            EditorUtility.SetDirty(def);
            return def;
        }

        private static GameObject CreateEnemy(
            string name,
            Vector2 position,
            Transform player,
            NavigationGrid2D nav,
            CoverManager coverManager,
            SquadController squad,
            EnemyArchetype archetype)
        {
            var root = new GameObject(name);
            root.transform.position = position;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            Color enemyTint = archetype switch
            {
                EnemyArchetype.Pistolier => new Color(0.70f, 0.62f, 0.52f),
                EnemyArchetype.MachineGunner => new Color(0.35f, 0.50f, 0.43f),
                EnemyArchetype.Demolitionist => new Color(0.58f, 0.28f, 0.30f),
                _ => new Color(0.46f, 0.58f, 0.52f)
            };

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = _circle;
            renderer.enabled = false;

            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = archetype == EnemyArchetype.MachineGunner ? 0.42f : 0.36f;

            var vitals = root.AddComponent<CharacterVitals>();
            var suppression = root.AddComponent<SuppressionReceiver2D>();
            var grenadeAwareness = root.AddComponent<GrenadeAwareness2D>();
            var morale = root.AddComponent<EnemyMorale2D>();

            switch (archetype)
            {
                case EnemyArchetype.Pistolier:
                    vitals.Configure(78f, 34f, 40f);
                    morale.Configure(0.48f);
                    break;
                case EnemyArchetype.MachineGunner:
                    vitals.Configure(145f, 62f, 70f);
                    morale.Configure(0.92f);
                    break;
                case EnemyArchetype.Demolitionist:
                    vitals.Configure(105f, 50f, 54f);
                    morale.Configure(0.82f);
                    break;
                default:
                    vitals.Configure(100f, 44f, 50f);
                    morale.Configure(0.74f);
                    break;
            }

            Transform enemyRig = CreateModularRig(
                root,
                vitals,
                true,
                enemyTint);

            CreateActorShadow(
                root.transform,
                archetype == EnemyArchetype.MachineGunner
                    ? new Vector2(1.18f, 0.72f)
                    : new Vector2(1.00f, 0.58f),
                6);

            if (archetype == EnemyArchetype.MachineGunner)
                enemyRig.localScale = Vector3.one * 1.12f;

            root.AddComponent<EnemyDeathPresentation2D>();

            var motor = root.AddComponent<EnemyMotor2D>();
            motor.Configure(nav, archetype == EnemyArchetype.MachineGunner ? 2.45f : 3.15f);

            var perception = root.AddComponent<EnemyPerception2D>();

            float sightDistance =
                archetype == EnemyArchetype.MachineGunner
                    ? 12f
                    : archetype == EnemyArchetype.Rifleman
                        ? 10.5f
                        : 9.2f;

            perception.Configure(
                1 << _obstacleLayer,
                sightDistance,
                145f,
                1f);

            var gun = new GameObject("Gun");
            gun.transform.SetParent(root.transform, false);
            gun.transform.localPosition = new Vector3(0.70f, 0f, 0f);
            gun.transform.localScale = Vector3.one * 0.52f;
            var gunRenderer = gun.AddComponent<SpriteRenderer>();
            gunRenderer.sprite = archetype switch
            {
                EnemyArchetype.Pistolier => _pistolSprite,
                EnemyArchetype.MachineGunner => _machineGunSprite,
                _ => _rifleSprite
            };
            gunRenderer.color = Color.white;
            gunRenderer.sortingOrder = 20;

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(gun.transform, false);
            muzzle.transform.localPosition = new Vector3(1.02f, 0f, 0f);

            var flashGo = new GameObject("Muzzle Flash");
            flashGo.transform.SetParent(muzzle.transform, false);
            var flashRenderer = flashGo.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = _muzzleSprite;
            flashRenderer.color = Color.white;
            flashRenderer.sortingOrder = 30;
            flashGo.transform.localScale = Vector3.one * 0.52f;
            var flash = flashGo.AddComponent<MuzzleFlash2D>();
            flash.Configure(flashRenderer);

            var audio = gun.AddComponent<AudioSource>();
            audio.spatialBlend = 0.45f;

            var enemyWeapon = root.AddComponent<EnemyWeapon2D>();
            enemyWeapon.Configure(
                player,
                muzzle.transform,
                audio,
                null,
                flash,
                ~0);

            switch (archetype)
            {
                case EnemyArchetype.Pistolier:
                    enemyWeapon.ConfigureStats(17f, 3.6f, 5.2f, 1, 2, 8.0f);
                    break;
                case EnemyArchetype.MachineGunner:
                    enemyWeapon.ConfigureStats(13f, 11.5f, 3.1f, 5, 9, 11.0f, 2.2f);
                    break;
                case EnemyArchetype.Demolitionist:
                    enemyWeapon.ConfigureStats(17f, 7f, 3.3f, 2, 4, 8.8f);
                    break;
                default:
                    enemyWeapon.ConfigureStats(18f, 8.4f, 2.25f, 2, 4, 9.5f);
                    break;
            }

            var grenadeThrower = root.AddComponent<EnemyGrenadeThrower>();
            int grenadeCount = archetype == EnemyArchetype.Demolitionist ? 3 :
                               archetype == EnemyArchetype.Rifleman ? 1 :
                               archetype == EnemyArchetype.Pistolier ? 1 : 0;
            grenadeThrower.Configure(
                player,
                _grenadeSprite,
                null,
                grenadeCount);

            var brain = root.AddComponent<EnemyBrain>();
            brain.Configure(player, motor, perception, enemyWeapon, grenadeThrower, squad, coverManager, vitals, archetype);

            if (archetype != EnemyArchetype.MachineGunner)
            {
                var patrol = root.AddComponent<EnemyPatrol2D>();
                patrol.Configure(
                    motor,
                    brain,
                    archetype == EnemyArchetype.Pistolier ? 2.2f : 3.2f);
            }

            if (archetype == EnemyArchetype.Demolitionist)
            {
                var charge = root.AddComponent<DemolitionistCharge2D>();
                charge.Configure(player, vitals);
            }

            root.AddComponent<TacticalEnemyAgent>();

            return root;
        }

        private static void CreateFloor()
        {
            // Keep the prototype base deliberately plain. The concept builder
            // owns all district roads, plaza surfaces and interior floors.
            // Creating Floor Zone/floor_panel/hazard objects here caused them
            // to survive above the final level and cover the new environment.
            var floor =
                new GameObject("Floor");

            floor.transform.position =
                Vector3.zero;

            var sr =
                floor.AddComponent<SpriteRenderer>();

            sr.sprite =
                _square;

            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size =
                new Vector2(
                    96f,
                    64f);

            sr.color =
                new Color(
                    0.075f,
                    0.085f,
                    0.115f,
                    1f);

            sr.sortingOrder =
                -100;
        }

        private static void CreateGeometry()
        {
            // Only the outer shell remains authoritative after concept-art
            // conversion. Interior concept buildings create their own wall
            // collision and door openings.
            CreateWall(
                "North",
                new Vector2(0f, 31.5f),
                new Vector2(96f, 0.9f));

            CreateWall(
                "South",
                new Vector2(0f, -31.5f),
                new Vector2(96f, 0.9f));

            CreateWall(
                "West",
                new Vector2(-47.5f, 0f),
                new Vector2(0.9f, 64f));

            CreateWall(
                "East",
                new Vector2(47.5f, 0f),
                new Vector2(0.9f, 64f));

            // Prototype-only blockers provide a sensible raw sandbox before
            // the concept builder replaces them with semantic buildings.
            CreateWall(
                "Prototype Warehouse Back",
                new Vector2(-23f, 20f),
                new Vector2(26f, 0.7f));

            CreateWall(
                "Prototype Admin Back",
                new Vector2(25f, 20f),
                new Vector2(22f, 0.7f));

            CreateWall(
                "Prototype Barracks Back",
                new Vector2(25f, -11f),
                new Vector2(20f, 0.7f));

            CreateWall(
                "Prototype Workshop Back",
                new Vector2(-22f, -11f),
                new Vector2(18f, 0.7f));

            CreateDestructible(
                "Crate 1",
                new Vector2(-34f, -20f));

            CreateDestructible(
                "Crate 2",
                new Vector2(-6f, -4f));

            CreateDestructible(
                "Crate 3",
                new Vector2(6f, 2f));

            CreateDestructible(
                "Crate 4",
                new Vector2(18f, -6f));

            CreateDestructible(
                "Crate 5",
                new Vector2(32f, -18f));

            CreateExplosiveProp(
                "Fuel Drum A",
                new Vector2(-29f, -16f));

            CreateExplosiveProp(
                "Fuel Drum B",
                new Vector2(2f, 4f));

            CreateExplosiveProp(
                "Fuel Drum C",
                new Vector2(29f, 4f));
        }

        private static void CreateFloorZone(
            string name,
            Vector2 position,
            Vector2 size,
            Color tint)
        {
            var go = new GameObject("Floor Zone // " + name);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _floorPanelSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.color = tint;
            sr.sortingOrder = -92;
        }

        private static void CreateHazardStrip(
            string name,
            Vector2 position,
            Vector2 size,
            float rotation)
        {
            var go = new GameObject("Hazard // " + name);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _hazardSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.color = new Color(0.86f, 0.86f, 0.86f, 0.92f);
            sr.sortingOrder = -78;
        }

        private static void CreateLightPool(
            string name,
            Vector2 position,
            Vector2 scale,
            Color color)
        {
            var go = new GameObject("Light Pool // " + name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _softGlowSprite;
            sr.color = color;
            sr.sortingOrder = -70;
        }

        private static void CreateActorShadow(
            Transform parent,
            Vector2 scale,
            int sortingOrder)
        {
            if (_softShadowSprite == null) return;

            var shadow = new GameObject("Actor Shadow");
            shadow.transform.SetParent(parent, false);
            shadow.transform.localPosition = new Vector3(-0.06f, -0.12f, 0f);
            shadow.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var sr = shadow.AddComponent<SpriteRenderer>();
            sr.sprite = _softShadowSprite;
            sr.color = new Color(0f, 0f, 0f, 0.72f);
            sr.sortingOrder = sortingOrder;
        }

        private static void CreatePropShadow(
            Transform parent,
            Vector2 scale)
        {
            if (_softShadowSprite == null) return;

            var shadow = new GameObject("Prop Shadow");
            shadow.transform.SetParent(parent, false);
            shadow.transform.localPosition = new Vector3(0.12f, -0.16f, 0f);
            shadow.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var sr = shadow.AddComponent<SpriteRenderer>();
            sr.sprite = _softShadowSprite;
            sr.color = new Color(0f, 0f, 0f, 0.62f);
            sr.sortingOrder = 1;
        }

        private static Door2D CreateDoor(
            string name,
            Vector2 position,
            float rotation)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            root.transform.rotation =
                Quaternion.Euler(0f, 0f, rotation);
            root.layer = _obstacleLayer;

            var visual = new GameObject("Door Visual");
            visual.transform.SetParent(root.transform, false);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = _doorSprite;
            sr.sortingOrder = 6;
            sr.color = Color.white;

            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.85f, 0.38f);

            var door = root.AddComponent<Door2D>();
            door.Configure(col, visual.transform, false);

            return door;
        }

        private static GameObject CreateWall(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.layer = _obstacleLayer;

            var shadow = new GameObject("Wall Shadow");
            shadow.transform.SetParent(go.transform, false);
            shadow.transform.localPosition = new Vector3(0.16f, -0.18f, 0f);
            var shadowRenderer = shadow.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = _square;
            shadowRenderer.drawMode = SpriteDrawMode.Tiled;
            shadowRenderer.size = size + new Vector2(0.14f, 0.14f);
            shadowRenderer.color = new Color(0.015f, 0.018f, 0.028f, 0.62f);
            shadowRenderer.sortingOrder = 0;

            var side = new GameObject("Wall Side");
            side.transform.SetParent(go.transform, false);
            side.transform.localPosition = new Vector3(0f, -0.12f, 0f);
            var sideRenderer = side.AddComponent<SpriteRenderer>();
            sideRenderer.sprite = _wallSprite;
            sideRenderer.drawMode = SpriteDrawMode.Tiled;
            sideRenderer.size = size;
            sideRenderer.color = new Color(0.48f, 0.52f, 0.62f, 1f);
            sideRenderer.sortingOrder = 1;

            var top = new GameObject("Wall Top");
            top.transform.SetParent(go.transform, false);
            top.transform.localPosition = new Vector3(-0.03f, 0.07f, 0f);
            var sr = top.AddComponent<SpriteRenderer>();
            sr.sprite = _wallSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.color = new Color(0.94f, 0.97f, 1f, 1f);
            sr.sortingOrder = 3;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
            return go;
        }

        private static void CreateExplosiveProp(string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.72f, 1.05f, 1f);

            CreatePropShadow(go.transform, new Vector2(0.95f, 0.50f));

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _barrelSprite;
            sr.color = Color.white;
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.48f;

            var prop = go.AddComponent<ExplosiveProp2D>();
            prop.Configure(
                42f,
                _grenadeSprite,
                null);
        }

        private static void CreateDestructible(string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 1.25f;
            go.layer = _obstacleLayer;

            CreatePropShadow(go.transform, new Vector2(1.05f, 0.58f));

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _crateSprite;
            sr.color = Color.white;
            sr.sortingOrder = 3;

            go.AddComponent<BoxCollider2D>();
            go.AddComponent<DestructibleCover>().Configure(80f, true);
        }

        private static void CreateCheckpoint(
            string label,
            Vector2 position,
            Vector2 respawn)
        {
            var go = new GameObject("Checkpoint // " + label);
            go.transform.position = position;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 2.2f);

            var trigger = go.AddComponent<CheckpointTrigger>();
            trigger.Configure(label, respawn);
        }

        private static void CreatePickup(
            string name,
            Vector2 position,
            PickupType type,
            int amount,
            Color color)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.52f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.color = color;
            sr.sortingOrder = 8;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;

            var pickup = go.AddComponent<Pickup2D>();
            pickup.Configure(type, amount);
        }

        private static void CreateCoverPoints()
        {
            Vector2[] positions =
            {
                new Vector2(-15f, -4f), new Vector2(-12f, -1f),
                new Vector2(-8f, 5f), new Vector2(-5.5f, -4f),
                new Vector2(-3f, 4f), new Vector2(1f, -4f),
                new Vector2(4f, 4f), new Vector2(7f, -3f),
                new Vector2(9f, 5f), new Vector2(12f, -4f),
                new Vector2(15f, 4f), new Vector2(18f, -1f)
            };

            foreach (var position in positions)
            {
                var go = new GameObject("Cover Point");
                go.transform.position = position;
                go.AddComponent<CoverPoint>();
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string leaf = System.IO.Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            if (!string.IsNullOrEmpty(parent))
                AssetDatabase.CreateFolder(parent, leaf);
        }

        private static int EnsureLayer(string layerName)
        {
            int existing = LayerMask.NameToLayer(layerName);
            if (existing >= 0) return existing;

            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            var layers = tagManager.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(layer.stringValue)) continue;

                layer.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }

            return 0;
        }
    }
}
#endif
