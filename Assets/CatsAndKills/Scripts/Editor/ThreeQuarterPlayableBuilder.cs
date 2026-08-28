#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using CatsAndKills.Visual;
using CatsAndKills.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CatsAndKills.EditorTools
{
    public static class ThreeQuarterPlayableBuilder
    {
        [MenuItem("Tools/Cats and Kills/Build Playable 3-4 Starter Room")]
        public static void Build()
        {
            ProductionArtPack pack =
                ThreeQuarterStarterArtFactory.EnsureStarterPack();

            BuildWithPack(
                pack,
                "starter");
        }

        public static void BuildWithPack(
            ProductionArtPack pack,
            string label = "custom")
        {
            if (pack == null || !pack.HasMinimumPlayableArt)
            {
                Debug.LogError(
                    "3/4 art pack is incomplete: " + label);
                return;
            }

            PrototypeSceneBuilder.Build();

            PlayerMotor2D player =
                Object.FindAnyObjectByType<PlayerMotor2D>();

            if (player == null)
            {
                Debug.LogError(
                    "Player was not found after sandbox build.");
                return;
            }

            ConvertPlayer(player.gameObject, pack);

            EnemyBrain[] enemies =
                Object.FindObjectsByType<EnemyBrain>(
                    FindObjectsSortMode.None);

            foreach (EnemyBrain enemy in enemies)
                ConvertEnemy(enemy, player.transform, pack);

            ConvertEnvironment(pack);
            AddDecor(pack);

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.orthographicSize = 5.25f;
                camera.backgroundColor =
                    new Color(0.018f, 0.022f, 0.034f);
            }

            EditorSceneManager.MarkSceneDirty(
                EditorSceneManager.GetActiveScene());

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player.gameObject;

            Debug.Log(
                "Cats and Kills playable 3/4 room rebuilt with " +
                label +
                " art. Press Play and open the Game tab.");
        }

        private static void ConvertPlayer(
            GameObject root,
            ProductionArtPack pack)
        {
            HideLegacyActorVisuals(root);

            CharacterVitals vitals =
                root.GetComponent<CharacterVitals>();

            Rigidbody2D body =
                root.GetComponent<Rigidbody2D>();

            PlayerAim2D aim =
                root.GetComponent<PlayerAim2D>();

            HitscanWeapon2D weapon =
                root.GetComponentInChildren<HitscanWeapon2D>();

            if (aim != null)
                aim.SetBodyRotationEnabled(false);

            GameObject visual =
                CreateCharacterVisual(
                    root.transform,
                    "Player 3-4 Visual",
                    pack.player,
                    vitals,
                    body,
                    aim,
                    weapon,
                    null,
                    1.18f);

            ReplaceShadow(
                root.transform,
                pack.softShadow,
                new Vector3(1.18f, 0.52f, 1f));

            if (visual != null)
                visual.transform.localPosition =
                    Vector3.zero;
        }

        private static void ConvertEnemy(
            EnemyBrain brain,
            Transform player,
            ProductionArtPack pack)
        {
            GameObject root = brain.gameObject;
            HideLegacyActorVisuals(root);

            DirectionalSpriteSet set =
                GetEnemySet(brain.Archetype, pack);

            float scale =
                brain.Archetype == EnemyArchetype.MachineGunner
                    ? 1.26f
                    : 1.10f;

            GameObject visual =
                CreateCharacterVisual(
                    root.transform,
                    "Enemy 3-4 Visual",
                    set,
                    root.GetComponent<CharacterVitals>(),
                    root.GetComponent<Rigidbody2D>(),
                    null,
                    null,
                    player,
                    scale);

            if (visual != null)
                visual.transform.localPosition =
                    Vector3.zero;

            ReplaceShadow(
                root.transform,
                pack.softShadow,
                brain.Archetype == EnemyArchetype.MachineGunner
                    ? new Vector3(1.32f, 0.58f, 1f)
                    : new Vector3(1.12f, 0.48f, 1f));
        }

        private static GameObject CreateCharacterVisual(
            Transform root,
            string name,
            DirectionalSpriteSet set,
            CharacterVitals vitals,
            Rigidbody2D body,
            PlayerAim2D aim,
            HitscanWeapon2D weapon,
            Transform lookTarget,
            float scale)
        {
            if (set == null)
                return null;

            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localScale =
                Vector3.one * scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite =
                set.GetIdle(CharacterDirection8.South);

            sr.color = Color.white;
            sr.sortingOrder = 10;
            sr.enabled = true;

            Shader spriteShader =
                Shader.Find("Sprites/Default");

            if (spriteShader != null)
            {
                sr.sharedMaterial =
                    new Material(spriteShader)
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
            }

            ThreeQuarterCharacterVisual2D visual =
                go.AddComponent<ThreeQuarterCharacterVisual2D>();

            visual.Configure(
                set,
                sr,
                vitals,
                body,
                aim,
                weapon,
                lookTarget);

            DepthSortedSprite2D depth =
                go.AddComponent<DepthSortedSprite2D>();

            SpriteRenderer weaponRenderer =
                weapon != null
                    ? weapon.GetComponent<SpriteRenderer>()
                    : null;

            if (weaponRenderer != null)
            {
                weaponRenderer.enabled = true;
                weaponRenderer.forceRenderingOff = false;

                depth.Configure(
                    new[] { sr, weaponRenderer },
                    5000,
                    -0.58f);
            }
            else
            {
                depth.Configure(
                    new[] { sr },
                    5000,
                    -0.58f);
            }

            return go;
        }

        private static void HideLegacyActorVisuals(
            GameObject root)
        {
            foreach (SpriteRenderer sr in
                     root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr == null)
                    continue;

                string n = sr.gameObject.name;

                if (n.Contains("Muzzle Flash") ||
                    n.Contains("Actor Shadow") ||
                    n == "Weapon")
                {
                    // Weapon is a gameplay renderer and must remain visible so
                    // switching slots can actually change the held gun.
                    sr.enabled = true;
                    sr.forceRenderingOff = false;
                    continue;
                }

                sr.enabled = false;
            }
        }

        private static void ReplaceShadow(
            Transform root,
            Sprite shadowSprite,
            Vector3 scale)
        {
            if (shadowSprite == null)
                return;

            Transform existing =
                root.Find("Actor Shadow");

            GameObject go;

            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject("Actor Shadow");
                go.transform.SetParent(root, false);
            }

            go.transform.localPosition =
                new Vector3(0.06f, -0.03f, 0f);

            go.transform.localScale = scale;

            SpriteRenderer sr =
                go.GetComponent<SpriteRenderer>();

            if (sr == null)
                sr = go.AddComponent<SpriteRenderer>();

            sr.sprite = shadowSprite;
            sr.color = new Color(0f, 0f, 0f, 0.58f);
            sr.sortingOrder = -5;
            sr.enabled = true;

            WorldUpright2D upright =
                go.GetComponent<WorldUpright2D>();

            if (upright == null)
                upright = go.AddComponent<WorldUpright2D>();

            upright.Configure();
        }

        private static DirectionalSpriteSet GetEnemySet(
            EnemyArchetype type,
            ProductionArtPack pack)
        {
            switch (type)
            {
                case EnemyArchetype.Pistolier:
                    return pack.pistolier;

                case EnemyArchetype.MachineGunner:
                    return pack.machineGunner;

                case EnemyArchetype.Demolitionist:
                    return pack.demolitionist;

                default:
                    return pack.rifleman;
            }
        }

        private static void ConvertEnvironment(
            ProductionArtPack pack)
        {
            GameObject floor = GameObject.Find("Floor");
            if (floor != null)
            {
                SpriteRenderer sr =
                    floor.GetComponent<SpriteRenderer>();

                if (sr != null)
                {
                    sr.sprite = pack.floorIndustrial;
                    sr.drawMode = SpriteDrawMode.Tiled;
                    sr.size = new Vector2(46f, 28f);
                    sr.color = new Color(
                        0.86f,
                        0.90f,
                        1f,
                        1f);
                    sr.sortingOrder = -100;
                }
            }

            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null)
                    continue;

                string n = sr.gameObject.name;
                string parent =
                    sr.transform.parent != null
                        ? sr.transform.parent.name
                        : string.Empty;

                if (parent.Contains("Floor Zone") ||
                    n.Contains("Floor Zone"))
                {
                    sr.sprite =
                        parent.Contains("Administration")
                            ? pack.floorOffice
                            : pack.floorIndustrial;

                    sr.color = Color.white;
                    continue;
                }

                if (n == "Wall Top" ||
                    n == "Wall Side")
                {
                    // These are prototype tiled renderers whose size is tied
                    // to gameplay geometry. The large concept wall illustration
                    // cannot be tiled here: doing so creates giant repeated
                    // vertical strips. ConceptVisualPolishBuilder owns the
                    // visible 3/4 wall art.
                    sr.sprite = GeneratedArtFactory.Get("wall");
                    sr.drawMode = SpriteDrawMode.Tiled;
                    sr.color =
                        n == "Wall Top"
                            ? new Color(0.86f, 0.90f, 0.98f, 1f)
                            : new Color(0.40f, 0.46f, 0.58f, 1f);

                    continue;
                }

                if (parent.Contains("Admin Security Door") ||
                    n == "Door Visual")
                {
                    sr.sprite = pack.reinforcedDoor;
                    sr.color = Color.white;
                    sr.drawMode = SpriteDrawMode.Simple;
                    sr.transform.localScale =
                        Vector3.one * 0.82f;

                    continue;
                }

                if (parent.StartsWith("Crate") ||
                    n.StartsWith("Crate"))
                {
                    sr.sprite = pack.crateLight;
                    sr.color = Color.white;
                    continue;
                }

                if (parent.StartsWith("Fuel Drum") ||
                    n.StartsWith("Fuel Drum"))
                {
                    sr.sprite = pack.fuelDrum;
                    sr.color = Color.white;
                    continue;
                }
            }
        }

        private static void AddDecor(
            ProductionArtPack pack)
        {
            CreateDecor(
                "Pipe Cluster A",
                pack.pipeCluster,
                new Vector2(-20.1f, 4.5f),
                0.9f,
                1300);

            CreateDecor(
                "Pipe Cluster B",
                pack.pipeCluster,
                new Vector2(8.8f, 7.7f),
                0.82f,
                1300);

            CreateDecor(
                "Propaganda Poster A",
                pack.propagandaPoster,
                new Vector2(-21.3f, 2.0f),
                0.72f,
                1250);

            CreateDecor(
                "Propaganda Poster B",
                pack.propagandaPoster,
                new Vector2(9.1f, 5.8f),
                0.65f,
                1250);

            CreateDecor(
                "Fence A",
                pack.fence,
                new Vector2(-16.8f, 9.0f),
                0.85f,
                1220);

            CreateDecor(
                "Lamp A",
                pack.lamp,
                new Vector2(-15.2f, 6.0f),
                0.78f,
                1400);

            CreateDecor(
                "Lamp B",
                pack.lamp,
                new Vector2(3.4f, 6.0f),
                0.78f,
                1400);

            for (int i = 0; i < 10; i++)
            {
                Vector2 p =
                    new Vector2(
                        -18f + i * 3.7f,
                        -10.6f + Mathf.Sin(i * 1.7f) * 0.9f);

                CreateDecor(
                    "Debris " + i,
                    pack.debris,
                    p,
                    Random.Range(0.35f, 0.65f),
                    1180);
            }
        }

        private static void CreateDecor(
            string name,
            Sprite sprite,
            Vector2 position,
            float scale,
            int baseOrder)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject("3-4 Decor // " + name);

            go.transform.position = position;
            go.transform.localScale =
                Vector3.one * scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = 0;

            DepthSortedSprite2D depth =
                go.AddComponent<DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                baseOrder,
                -0.2f);
        }
    }
}
#endif
