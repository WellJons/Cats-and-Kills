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
        [Header("Persistent sprite assets")]
        [SerializeField] private DirectionalSpriteSet playerSet;
        [SerializeField] private DirectionalSpriteSet pistolierSet;
        [SerializeField] private DirectionalSpriteSet riflemanSet;
        [SerializeField] private DirectionalSpriteSet machineGunnerSet;
        [SerializeField] private DirectionalSpriteSet demolitionistSet;

        [Header("Presentation")]
        [SerializeField] private float playerScale = 1.28f;
        [SerializeField] private float enemyScale = 1.22f;
        [SerializeField] private float machineGunnerScale = 1.40f;
        [SerializeField] private float verifyInterval = 0.35f;

        private float _nextVerify;

        public void Configure(
            DirectionalSpriteSet player,
            DirectionalSpriteSet pistolier,
            DirectionalSpriteSet rifleman,
            DirectionalSpriteSet machineGunner,
            DirectionalSpriteSet demolitionist)
        {
            playerSet = player;
            pistolierSet = pistolier;
            riflemanSet = rifleman;
            machineGunnerSet = machineGunner;
            demolitionistSet = demolitionist;
        }

        private IEnumerator Start()
        {
            yield return null;
            RebuildAll();

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
                Object.FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
            {
                Install(
                    player.gameObject,
                    playerSet,
                    player.transform,
                    true,
                    playerScale);
            }

            EnemyBrain[] enemies =
                Object.FindObjectsByType<EnemyBrain>(
                    FindObjectsInactive.Include,
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

                Install(
                    enemy.gameObject,
                    SetFor(enemy.Archetype),
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
                Object.FindAnyObjectByType<PlayerMotor2D>();

            if (player != null)
            {
                EnsureOne(
                    player.gameObject,
                    playerSet,
                    player.transform,
                    true,
                    playerScale);
            }

            EnemyBrain[] enemies =
                Object.FindObjectsByType<EnemyBrain>(
                    FindObjectsInactive.Include,
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
                    SetFor(enemy.Archetype),
                    player != null
                        ? player.transform
                        : null,
                    false,
                    scale);
            }
        }

        private void EnsureOne(
            GameObject root,
            DirectionalSpriteSet set,
            Transform lookTarget,
            bool isPlayer,
            float scale)
        {
            if (root == null || set == null)
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
                    set,
                    lookTarget,
                    isPlayer,
                    scale);
            }
        }

        private void Install(
            GameObject root,
            DirectionalSpriteSet set,
            Transform lookTarget,
            bool isPlayer,
            float scale)
        {
            if (root == null || set == null)
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

            sr.sprite =
                set.GetIdle(
                    CharacterDirection8.South);

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
                    ? root.GetComponentInChildren<
                        HitscanWeapon2D>()
                    : null;

            ThreeQuarterCharacterVisual2D visual =
                go.AddComponent<
                    ThreeQuarterCharacterVisual2D>();

            visual.Configure(
                set,
                sr,
                vitals,
                body,
                aim,
                playerGun,
                isPlayer
                    ? null
                    : lookTarget);

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
            Object.Destroy(old.gameObject);
        }

        private DirectionalSpriteSet SetFor(
            EnemyArchetype type)
        {
            switch (type)
            {
                case EnemyArchetype.Pistolier:
                    return pistolierSet != null
                        ? pistolierSet
                        : riflemanSet;

                case EnemyArchetype.MachineGunner:
                    return machineGunnerSet != null
                        ? machineGunnerSet
                        : riflemanSet;

                case EnemyArchetype.Demolitionist:
                    return demolitionistSet != null
                        ? demolitionistSet
                        : riflemanSet;

                default:
                    return riflemanSet;
            }
        }
    }
}
