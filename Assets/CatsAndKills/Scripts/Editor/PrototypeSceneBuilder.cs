#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Core;
using CatsAndKills.Damage;
using CatsAndKills.FX;
using CatsAndKills.Player;
using CatsAndKills.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

        [MenuItem("Tools/Cats and Kills/Build Playable v0.1 Sandbox")]
        public static void Build()
        {
            EnsureFolder("Assets/CatsAndKills/Scenes");
            EnsureFolder(DataFolder);
            _obstacleLayer = EnsureLayer(ObstacleLayerName);

            _square = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            _circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

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
            new GameObject("Haptics").AddComponent<HapticsManager>();
            new GameObject("Radio Dialogue").AddComponent<RadioDialogueSystem>();
            new GameObject("World Callouts").AddComponent<WorldCalloutSystem>();

            var fxGo = new GameObject("FX Service");
            var fx = fxGo.AddComponent<FXService>();
            fx.bloodSprite = _circle;
            fx.sparkSprite = _circle;
            fx.casingSprite = _square;
            fx.bulletHoleSprite = _circle;
            fx.explosionSprite = _circle;
            fx.smokeSprite = _circle;
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
            bodyRenderer.color = new Color(0.92f, 0.93f, 0.97f);
            bodyRenderer.sortingOrder = 10;
            root.transform.localScale = Vector3.one * 0.78f;

            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.46f;

            var vitals = root.AddComponent<CharacterVitals>();
            vitals.Configure(115f, 50f, 58f);

            var torso = root.AddComponent<BodyPartHitbox>();
            torso.Configure(vitals, BodyPart.Torso, 1f);

            var motor = root.AddComponent<PlayerMotor2D>();
            var aim = root.AddComponent<PlayerAim2D>();
            var death = root.AddComponent<PlayerDeathController>();
            death.Configure(vitals);
            var collar = root.AddComponent<CollarAbility>();

            var aimPivot = new GameObject("Aim Pivot");
            aimPivot.transform.SetParent(root.transform, false);
            aim.Configure(camera, aimPivot.transform, root.transform);

            var weaponGo = new GameObject("Weapon");
            weaponGo.transform.SetParent(aimPivot.transform, false);
            weaponGo.transform.localPosition = new Vector3(0.75f, 0f, 0f);
            weaponGo.transform.localScale = new Vector3(1.5f, 0.22f, 1f);

            var weaponRenderer = weaponGo.AddComponent<SpriteRenderer>();
            weaponRenderer.sprite = _square;
            weaponRenderer.color = new Color(0.13f, 0.15f, 0.18f);
            weaponRenderer.sortingOrder = 20;

            var visualRecoil = weaponGo.AddComponent<WeaponVisualRecoil2D>();

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(weaponGo.transform, false);
            muzzle.transform.localPosition = new Vector3(0.65f, 0f, 0f);

            var flashGo = new GameObject("Muzzle Flash");
            flashGo.transform.SetParent(muzzle.transform, false);
            var flashRenderer = flashGo.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = _circle;
            flashRenderer.color = new Color(1f, 0.62f, 0.12f);
            flashRenderer.sortingOrder = 30;
            flashGo.transform.localScale = Vector3.one * 0.38f;
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

            var weapon = weaponGo.AddComponent<HitscanWeapon2D>();
            weapon.Configure(rifle, aim, motor, muzzle.transform, casing.transform, cameraFollow, weaponRenderer, visualRecoil, flash, audio);

            var arsenal = root.AddComponent<PlayerArsenal>();
            arsenal.Configure(weapon, new[] { rifle, pistol, shotgun });

            var grenadeController = root.AddComponent<PlayerGrenadeController>();
            grenadeController.Configure(aim, _circle, null, null);

            var hudGo = new GameObject("Prototype HUD");
            var hud = hudGo.AddComponent<PrototypeHUD>();
            hud.Configure(vitals, arsenal, grenadeController, collar, null);

            return root;
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
            def.weaponSprite = _square;
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

            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = _circle;
            renderer.color = archetype switch
            {
                EnemyArchetype.Pistolier => new Color(0.70f, 0.62f, 0.52f),
                EnemyArchetype.MachineGunner => new Color(0.35f, 0.50f, 0.43f),
                EnemyArchetype.Demolitionist => new Color(0.58f, 0.28f, 0.30f),
                _ => new Color(0.46f, 0.58f, 0.52f)
            };
            renderer.sortingOrder = 10;
            root.transform.localScale = archetype == EnemyArchetype.MachineGunner
                ? Vector3.one * 0.92f
                : Vector3.one * 0.76f;

            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.46f;

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

            var hit = root.AddComponent<BodyPartHitbox>();
            hit.Configure(vitals, BodyPart.Torso, 1f);

            var motor = root.AddComponent<EnemyMotor2D>();
            motor.Configure(nav, archetype == EnemyArchetype.MachineGunner ? 2.45f : 3.15f);

            var perception = root.AddComponent<EnemyPerception2D>();
            perception.Configure(1 << _obstacleLayer, 14f, 170f, 1f);

            var gun = new GameObject("Gun");
            gun.transform.SetParent(root.transform, false);
            gun.transform.localPosition = new Vector3(0.72f, 0f, 0f);
            gun.transform.localScale = new Vector3(1.3f, 0.20f, 1f);
            var gunRenderer = gun.AddComponent<SpriteRenderer>();
            gunRenderer.sprite = _square;
            gunRenderer.color = new Color(0.1f, 0.11f, 0.13f);
            gunRenderer.sortingOrder = 20;

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(gun.transform, false);
            muzzle.transform.localPosition = new Vector3(0.6f, 0f, 0f);

            var flashGo = new GameObject("Muzzle Flash");
            flashGo.transform.SetParent(muzzle.transform, false);
            var flashRenderer = flashGo.AddComponent<SpriteRenderer>();
            flashRenderer.sprite = _circle;
            flashRenderer.color = new Color(1f, 0.55f, 0.08f);
            flashRenderer.sortingOrder = 30;
            flashGo.transform.localScale = Vector3.one * 0.3f;
            var flash = flashGo.AddComponent<MuzzleFlash2D>();
            flash.Configure(flashRenderer);

            var audio = gun.AddComponent<AudioSource>();
            audio.spatialBlend = 0.45f;

            var enemyWeapon = root.AddComponent<EnemyWeapon2D>();
            enemyWeapon.Configure(player, muzzle.transform, audio, null, flash, ~0);

            switch (archetype)
            {
                case EnemyArchetype.Pistolier:
                    enemyWeapon.ConfigureStats(17f, 3.6f, 5.2f, 1, 2, 11f);
                    break;
                case EnemyArchetype.MachineGunner:
                    enemyWeapon.ConfigureStats(13f, 11.5f, 3.1f, 6, 11, 17f);
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
            grenadeThrower.Configure(player, _circle, null, grenadeCount);

            var brain = root.AddComponent<EnemyBrain>();
            brain.Configure(player, motor, perception, enemyWeapon, grenadeThrower, squad, coverManager, vitals, archetype);

            return root;
        }

        private static void CreateFloor()
        {
            var floor = new GameObject("Floor");
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(46f, 28f, 1f);
            var sr = floor.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.color = new Color(0.10f, 0.12f, 0.17f);
            sr.sortingOrder = -100;
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
            CreateWall("Admin Hall", new Vector2(10f, 2f), new Vector2(0.7f, 14f));
            CreateWall("Admin Cross", new Vector2(16f, 2f), new Vector2(10f, 0.7f));

            CreateDestructible("Crate 1", new Vector2(-14f, -5f));
            CreateDestructible("Crate 2", new Vector2(-5f, 5f));
            CreateDestructible("Crate 3", new Vector2(0f, -6f));
            CreateDestructible("Crate 4", new Vector2(6f, 4f));
            CreateDestructible("Crate 5", new Vector2(14f, -5f));
        }

        private static GameObject CreateWall(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            go.layer = _obstacleLayer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.color = new Color(0.25f, 0.29f, 0.38f);
            sr.sortingOrder = 2;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            return go;
        }

        private static void CreateDestructible(string name, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 1.25f;
            go.layer = _obstacleLayer;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _square;
            sr.color = new Color(0.42f, 0.30f, 0.22f);
            sr.sortingOrder = 3;

            go.AddComponent<BoxCollider2D>();
            go.AddComponent<DestructibleCover>().Configure(80f, true);
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
