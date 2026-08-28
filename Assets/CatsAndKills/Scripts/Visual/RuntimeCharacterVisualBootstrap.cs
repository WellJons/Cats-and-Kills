using System.Collections;
using CatsAndKills.AI;
using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.Visual
{
    [DefaultExecutionOrder(10000)]
    [DisallowMultipleComponent]
    public sealed class RuntimeCharacterVisualBootstrap : MonoBehaviour
    {
        [Header("Source atlases")]
        [SerializeField] private Texture2D playerAtlas;
        [SerializeField] private Texture2D pistolierAtlas;
        [SerializeField] private Texture2D riflemanAtlas;
        [SerializeField] private Texture2D machineGunnerAtlas;
        [SerializeField] private Texture2D demolitionistAtlas;

        [Header("Presentation")]
        [SerializeField] private float playerScale = 1.28f;
        [SerializeField] private float enemyScale = 1.22f;
        [SerializeField] private float machineGunnerScale = 1.40f;
        [SerializeField] private float verifyInterval = 0.40f;

        private float _nextVerify;

        public void Configure(
            Texture2D player,
            Texture2D pistolier,
            Texture2D rifleman,
            Texture2D machineGunner,
            Texture2D demolitionist)
        {
            playerAtlas = player;
            pistolierAtlas = pistolier;
            riflemanAtlas = rifleman;
            machineGunnerAtlas = machineGunner;
            demolitionistAtlas = demolitionist;
        }

        private IEnumerator Start()
        {
            // Let every gameplay Awake/Start finish first, then install visuals.
            yield return null;
            RebuildAll();

            // Run once more after the first rendered frame. This makes the
            // setup deterministic even if another component altered renderers
            // during its first Start/LateUpdate.
            yield return null;
            EnsureAllVisible();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextVerify)
                return;

            _nextVerify =
                Time.unscaledTime +
                Mathf.Max(0.15f, verifyInterval);

            EnsureAllVisible();
        }

        public void RebuildAll()
        {
            PlayerMotor2D player =
                FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
            {
                Install(
                    player.gameObject,
                    playerAtlas,
                    player.transform,
                    true,
                    playerScale);
            }

            EnemyBrain[] enemies =
                FindObjectsByType<EnemyBrain>(
                    FindObjectsSortMode.None);

            foreach (EnemyBrain enemy in enemies)
            {
                if (enemy == null)
                    continue;

                Texture2D atlas =
                    AtlasFor(enemy.Archetype);

                float scale =
                    enemy.Archetype ==
                    EnemyArchetype.MachineGunner
                        ? machineGunnerScale
                        : enemyScale;

                Install(
                    enemy.gameObject,
                    atlas,
                    player != null
                        ? player.transform
                        : null,
                    false,
                    scale);
            }
        }

        private void EnsureAllVisible()
        {
            PlayerMotor2D player =
                FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
            {
                EnsureOne(
                    player.gameObject,
                    playerAtlas,
                    player.transform,
                    true,
                    playerScale);
            }

            EnemyBrain[] enemies =
                FindObjectsByType<EnemyBrain>(
                    FindObjectsSortMode.None);

            foreach (EnemyBrain enemy in enemies)
            {
                if (enemy == null)
                    continue;

                float scale =
                    enemy.Archetype ==
                    EnemyArchetype.MachineGunner
                        ? machineGunnerScale
                        : enemyScale;

                EnsureOne(
                    enemy.gameObject,
                    AtlasFor(enemy.Archetype),
                    player != null
                        ? player.transform
                        : null,
                    false,
                    scale);
            }
        }

        private void EnsureOne(
            GameObject root,
            Texture2D atlas,
            Transform lookTarget,
            bool isPlayer,
            float scale)
        {
            if (root == null || atlas == null)
                return;

            Transform visual =
                root.transform.Find(
                    "Runtime Character Visual");

            SpriteRenderer sr =
                visual != null
                    ? visual.GetComponent<SpriteRenderer>()
                    : null;

            bool valid =
                visual != null &&
                visual.gameObject.activeInHierarchy &&
                sr != null &&
                sr.enabled &&
                !sr.forceRenderingOff &&
                sr.sprite != null &&
                sr.color.a > 0.01f;

            if (!valid)
            {
                Install(
                    root,
                    atlas,
                    lookTarget,
                    isPlayer,
                    scale);
            }
        }

        private void Install(
            GameObject root,
            Texture2D atlas,
            Transform lookTarget,
            bool isPlayer,
            float scale)
        {
            if (root == null || atlas == null)
                return;

            RemoveOldVisual(
                root.transform,
                "Runtime Character Visual");

            RemoveOldVisual(
                root.transform,
                "Concept Atlas Visual");

            RemoveOldVisual(
                root.transform,
                isPlayer
                    ? "Player 3-4 Visual"
                    : "Enemy 3-4 Visual");

            GameObject go =
                new GameObject(
                    "Runtime Character Visual");

            go.transform.SetParent(
                root.transform,
                false);

            go.transform.localPosition =
                Vector3.zero;

            go.transform.localRotation =
                Quaternion.identity;

            go.transform.localScale =
                Vector3.one * scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.enabled = true;
            sr.forceRenderingOff = false;
            sr.color = Color.white;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 10;

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
                    ? root.GetComponentInChildren<HitscanWeapon2D>()
                    : null;

            EnemyWeapon2D enemyGun =
                isPlayer
                    ? null
                    : root.GetComponent<EnemyWeapon2D>();

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
                isPlayer
                    ? null
                    : lookTarget,
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

        private static void RemoveOldVisual(
            Transform root,
            string name)
        {
            Transform old =
                root.Find(name);

            if (old == null)
                return;

            old.gameObject.SetActive(false);
            Destroy(old.gameObject);
        }

        private Texture2D AtlasFor(
            EnemyArchetype type)
        {
            switch (type)
            {
                case EnemyArchetype.Pistolier:
                    return pistolierAtlas != null
                        ? pistolierAtlas
                        : riflemanAtlas;

                case EnemyArchetype.MachineGunner:
                    return machineGunnerAtlas != null
                        ? machineGunnerAtlas
                        : riflemanAtlas;

                case EnemyArchetype.Demolitionist:
                    return demolitionistAtlas != null
                        ? demolitionistAtlas
                        : riflemanAtlas;

                default:
                    return riflemanAtlas;
            }
        }
    }
}
