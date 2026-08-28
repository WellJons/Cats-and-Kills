using System.Collections;
using CatsAndKills.AI;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class TacticalUtilityBelt : MonoBehaviour
    {
        [SerializeField] private int molotovCount = 2;
        [SerializeField] private int smokeCount = 2;
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private Sprite smokeSprite;
        [SerializeField] private NavigationGrid2D navigation;

        public int MolotovCount => molotovCount;
        public int SmokeCount => smokeCount;

        public void Configure(
            NavigationGrid2D nav,
            Sprite bottleSprite,
            Sprite smoke)
        {
            navigation = nav;
            projectileSprite = bottleSprite;
            smokeSprite = smoke;
        }

        public bool ThrowMolotovAt(
            Vector2 target)
        {
            if (molotovCount <= 0)
                return false;

            molotovCount--;

            StartCoroutine(
                ThrowUtility(
                    target,
                    true));

            return true;
        }

        public bool ThrowSmokeAt(
            Vector2 target)
        {
            if (smokeCount <= 0)
                return false;

            smokeCount--;

            StartCoroutine(
                ThrowUtility(
                    target,
                    false));

            return true;
        }

        private IEnumerator ThrowUtility(
            Vector2 target,
            bool fire)
        {
            Vector2 origin =
                transform.position;

            Vector2 destination =
                navigation != null
                    ? navigation.SnapToCell(
                        target)
                    : target;

            GameObject projectile =
                new GameObject(
                    fire
                        ? "Molotov Projectile"
                        : "Smoke Grenade Projectile");

            projectile.transform.position =
                origin;

            SpriteRenderer sr =
                projectile.AddComponent<
                    SpriteRenderer>();

            sr.sprite = projectileSprite;
            sr.color =
                fire
                    ? new Color(
                        1f,
                        0.42f,
                        0.12f)
                    : new Color(
                        0.72f,
                        0.78f,
                        0.82f);

            sr.sortingOrder = 7600;

            float duration = 0.34f;
            float elapsed = 0f;

            while (elapsed <
                   duration)
            {
                elapsed +=
                    Time.deltaTime;

                float t =
                    Mathf.Clamp01(
                        elapsed /
                        duration);

                Vector2 pos =
                    Vector2.Lerp(
                        origin,
                        destination,
                        t);

                pos.y +=
                    Mathf.Sin(
                        t *
                        Mathf.PI) *
                    0.75f;

                projectile.transform.position =
                    pos;

                projectile.transform.Rotate(
                    0f,
                    0f,
                    720f *
                    Time.deltaTime);

                yield return null;
            }

            Destroy(projectile);

            if (fire)
            {
                FXService.Instance?.ExplosionBurst(
                    destination);

                GameObject field =
                    new GameObject(
                        "Molotov Fire Field");

                field.transform.position =
                    destination;

                TacticalFireField2D fireField =
                    field.AddComponent<
                        TacticalFireField2D>();

                fireField.Configure(
                    gameObject,
                    navigation != null
                        ? navigation.CellSize
                        : 0.85f,
                    3);
            }
            else
            {
                GameObject cloud =
                    new GameObject(
                        "Smoke Field");

                cloud.transform.position =
                    destination;

                TacticalSmokeField2D smoke =
                    cloud.AddComponent<
                        TacticalSmokeField2D>();

                smoke.Configure(
                    smokeSprite,
                    2.15f,
                    3);
            }
        }
    }
}
