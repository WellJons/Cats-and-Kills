#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Audio;
using CatsAndKills.Combat;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
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

            CreatePickup("Ammo Cache", new Vector2(-6f, 7f), PickupType.Ammo, 45, new Color(0.25f, 0.65f, 0.95f));
            CreatePickup("Field Medkit", new Vector2(7f, -7f), PickupType.Medkit, 38, new Color(0.25f, 0.9f, 0.45f));
            CreatePickup("Grenade Box", new Vector2(15f, 7f), PickupType.Grenades, 2, new Color(0.95f, 0.65f, 0.15f));

            CreateCheckpoint(
                "WAREHOUSE",
                new Vector2(-6.0f, -7.5f),
                new Vector2(-6.0f, -7.0f));

            CreateCheckpoint(
                "ADMIN",
                new Vector2(9.0f, -5.5f),
                new Vector2(9.0f, -5.0f));

            var navGo = new GameObject("Navigation Grid");
            var nav = navGo.AddComponent<NavigationGrid2D>();
            nav.Configure(new Vector2(46f, 28f), 0.65f, 0.27f, 1 << _obstacleLayer);

            var coverManagerGo = new GameObject("Cover Manager");
            var coverManager = coverManagerGo.AddComponent<CoverManager>();
            coverManager.Configure(1 << _obstacleLayer);
            CreateCoverPoints();

            var player = CreatePlayer(new Vector2(-19f, -9f), camera, cameraFollow);

            var squadA = new GameObject("Squad A // Gate").AddComponent<SquadController>();
            var squadB = new GameObject("Squad B // Warehouse").AddComponent<SquadController>();
            var squadC = new GameObject("Squad C // Admin").AddComponent<SquadController>();

            CreateEnemy("Pistolier 01", new Vector2(-13f, -7f), player.transform, nav, coverManager, squadA, EnemyArchetype.Pistolier);
            CreateEnemy("Rifleman 01", new Vector2(-10f, -4f), player.transform, nav, coverManager, squadA, EnemyArchetype.Rifleman);
            CreateEnemy("Rifleman 02", new Vector2(-14f, -1f), player.transform, nav, coverManager, squadA, EnemyArchetype.Rifleman);

            CreateEnemy("Rifleman 03", new Vector2(-3f, -5f), player.transform, nav, coverManager, squadB, EnemyArchetype.Rifleman);
            CreateEnemy("Rifleman 04", new Vector2(0f, 1f), player.transform, nav, coverManager, squadB, EnemyArchetype.Rifleman);
            CreateEnemy("Machine Gunner", new Vector2(3f, -2f), player.transform, nav, coverManager, squadB, EnemyArchetype.MachineGunner);
            CreateEnemy("Demolitionist", new Vector2(1f, 6f), player.transform, nav, coverManager, squadB, EnemyArchetype.Demolitionist);

            CreateEnemy("Rifleman 05", new Vector2(10f, -6f), player.transform, nav, coverManager, squadC, EnemyArchetype.Rifleman);
            CreateEnemy("Rifleman 06", new Vector2(12f, 0f), player.transform, nav, coverManager, squadC, EnemyArchetype.Rifleman);
            CreateEnemy("Pistolier 02", new Vector2(16f, 4f), player.transform, nav, coverManager, squadC, EnemyArchetype.Pistolier);
            CreateEnemy("Demolitionist 02", new Vector2(18f, 7f), player.transform, nav, coverManager, squadC, EnemyArchetype.Demolitionist);

            var responseSquad = new GameObject("Squad D // Response").AddComponent<SquadController>();
            var reinforcementUnits = new[]
            {
                CreateEnemy("Reinforcement Rifle 01", new Vector2(18f, -10f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Rifleman),
                CreateEnemy("Reinforcement Rifle 02", new Vector2(20f, -8f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Rifleman),
                CreateEnemy("Reinforcement MG", new Vector2(20f, -11f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.MachineGunner),
                CreateEnemy("Reinforcement Demo", new Vector2(16f, -11f), player.transform, nav, coverManager, responseSquad, EnemyArchetype.Demolitionist)
            };

            var reinforcementDirector = new GameObject("Reinforcement Director")
                .AddComponent<ReinforcementDirector>();
            reinforcementDirector.Configure(reinforcementUnits);

            coverManager.Refresh();
            nav.Build();
            cameraFollow.Configure(player.transform, player.GetComponent<PlayerAim2D>());

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
            perception.Configure(1 << _obstacleLayer, 14f, 170f, 1f);

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
                    enemyWeapon.ConfigureStats(17f, 3.6f, 5.2f, 1, 2, 11f);
                    break;
                case EnemyArchetype.MachineGunner:
                    enemyWeapon.ConfigureStats(13f, 11.5f, 3.1f, 6, 11, 17f, 2.2f);
                    break;
                case EnemyArchetype.Demolitionist:
                    enemyWeapon.ConfigureStats(17f, 7f, 3.3f, 2, 4, 12f);
                    break;
                default:
                    enemyWeapon.ConfigureStats(18f, 8.4f, 2.25f, 2, 5, 14f);
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

            return root;
        }

        private static void CreateFloor()
        {
            var floor = new GameObject("Floor");
            floor.transform.position = Vector3.zero;
            var sr = floor.AddComponent<SpriteRenderer>();
            sr.sprite = _floorSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(46f, 28f);
            sr.color = Color.white;
            sr.sortingOrder = -100;

            CreateFloorZone(
                "Outer Yard",
                new Vector2(-15.5f, -1.0f),
                new Vector2(13.0f, 23.0f),
                new Color(0.72f, 0.77f, 0.90f));

            CreateFloorZone(
                "Warehouse",
                new Vector2(0.5f, 1.0f),
                new Vector2(13.5f, 22.0f),
                new Color(0.66f, 0.76f, 0.82f));

            CreateFloorZone(
                "Administration",
                new Vector2(16.0f, 1.5f),
                new Vector2(11.5f, 21.0f),
                new Color(0.76f, 0.71f, 0.80f));

            CreateHazardStrip(
                "Warehouse Threshold",
                new Vector2(-7.0f, -7.5f),
                new Vector2(1.1f, 4.2f),
                90f);

            CreateHazardStrip(
                "Admin Threshold",
                new Vector2(9.35f, 0f),
                new Vector2(1.2f, 3.6f),
                90f);

            CreateHazardStrip(
                "Extraction Marking",
                new Vector2(18.5f, -10.0f),
                new Vector2(4.8f, 0.75f),
                0f);

            CreateLightPool(
                "Cold Yard Lamp",
                new Vector2(-13f, 5f),
                new Vector2(7f, 7f),
                new Color(0.28f, 0.52f, 0.92f, 0.23f));

            CreateLightPool(
                "Warehouse Lamp",
                new Vector2(1.0f, 2.0f),
                new Vector2(8f, 8f),
                new Color(0.22f, 0.68f, 0.72f, 0.20f));

            CreateLightPool(
                "Admin Emergency Lamp",
                new Vector2(15.0f, 4.0f),
                new Vector2(6.5f, 6.5f),
                new Color(0.88f, 0.20f, 0.25f, 0.18f));
        }

        private static void CreateGeometry()
        {
            CreateWall("North", new Vector2(0f, 13.5f), new Vector2(46f, 0.8f));
            CreateWall("South", new Vector2(0f, -13.5f), new Vector2(46f, 0.8f));
            CreateWall("West", new Vector2(-22.5f, 0f), new Vector2(0.8f, 28f));
            CreateWall("East", new Vector2(22.5f, 0f), new Vector2(0.8f, 28f));

            CreateWall("Warehouse West", new Vector2(-7f, 2f), new Vector2(0.7f, 18f));
            CreateWall("Warehouse Rack A", new Vector2(-2f, -3f), new Vector2(0.7f, 6f));
            CreateWall("Warehouse Rack B", new Vector2(2.5f, 3f), new Vector2(0.7f, 6f));
            CreateWall("Admin Hall Lower", new Vector2(10f, -3f), new Vector2(0.7f, 4f));
            CreateWall("Admin Hall Upper", new Vector2(10f, 5f), new Vector2(0.7f, 8f));
            CreateDoor("Admin Security Door", new Vector2(10f, 0f), 90f);
            CreateWall("Admin Cross", new Vector2(16f, 2f), new Vector2(10f, 0.7f));

            CreateDestructible("Crate 1", new Vector2(-14f, -5f));
            CreateDestructible("Crate 2", new Vector2(-5f, 5f));
            CreateDestructible("Crate 3", new Vector2(0f, -6f));
            CreateDestructible("Crate 4", new Vector2(6f, 4f));
            CreateDestructible("Crate 5", new Vector2(14f, -5f));

            CreateExplosiveProp("Fuel Drum A", new Vector2(-1f, 5.5f));
            CreateExplosiveProp("Fuel Drum B", new Vector2(6.5f, -2.5f));
            CreateExplosiveProp("Fuel Drum C", new Vector2(15.5f, 5.0f));
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
