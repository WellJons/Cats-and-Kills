using CatsAndKills.Damage;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public static class CharacterCombatGeometry2D
    {
        public const float ChestHeight = 0.68f;
        public const float MuzzleForward = 0.64f;

        public static Vector2 AimPoint(
            Transform characterRoot)
        {
            if (characterRoot == null)
                return Vector2.zero;

            return
                (Vector2)characterRoot.position +
                Vector2.up * ChestHeight;
        }

        public static Vector2 MuzzlePoint(
            Transform characterRoot,
            Vector2 direction)
        {
            if (characterRoot == null)
                return Vector2.zero;

            Vector2 dir =
                direction.sqrMagnitude > 0.0001f
                    ? direction.normalized
                    : Vector2.right;

            return
                AimPoint(characterRoot) +
                dir * MuzzleForward;
        }

        public static Vector2 CasingPoint(
            Transform characterRoot,
            Vector2 direction)
        {
            if (characterRoot == null)
                return Vector2.zero;

            Vector2 dir =
                direction.sqrMagnitude > 0.0001f
                    ? direction.normalized
                    : Vector2.right;

            Vector2 side =
                new Vector2(
                    -dir.y,
                    dir.x);

            return
                AimPoint(characterRoot) +
                dir * 0.16f +
                side * 0.12f;
        }

        public static RaycastHit2D FirstMeaningfulHit(
            Vector2 origin,
            Vector2 direction,
            float distance,
            LayerMask mask,
            Transform shooterRoot)
        {
            RaycastHit2D[] hits =
                Physics2D.RaycastAll(
                    origin,
                    direction,
                    distance,
                    mask);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                    continue;

                Transform hitRoot =
                    hit.collider.transform.root;

                if (shooterRoot != null &&
                    hitRoot == shooterRoot)
                {
                    continue;
                }

                IDamageReceiver receiver =
                    hit.collider.GetComponent<IDamageReceiver>();

                if (receiver == null)
                {
                    receiver =
                        hit.collider.GetComponentInParent<
                            IDamageReceiver>();
                }

                CharacterVitals character =
                    hit.collider.GetComponentInParent<
                        CharacterVitals>();

                // A character's movement footprint is intentionally separate
                // from the visible body hitboxes. Do not let the footprint at
                // the feet consume bullets.
                if (character != null &&
                    receiver == null)
                {
                    continue;
                }

                return hit;
            }

            return default;
        }

        public static void PlaceMuzzleTransform(
            Transform muzzle,
            Transform characterRoot,
            Vector2 direction)
        {
            if (muzzle == null ||
                characterRoot == null)
            {
                return;
            }

            Vector2 dir =
                direction.sqrMagnitude > 0.0001f
                    ? direction.normalized
                    : Vector2.right;

            muzzle.position =
                MuzzlePoint(
                    characterRoot,
                    dir);

            float angle =
                Mathf.Atan2(
                    dir.y,
                    dir.x) *
                Mathf.Rad2Deg;

            muzzle.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle);
        }
    }
}
