using System.Collections.Generic;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class TacticalSmokeField2D : MonoBehaviour
    {
        private static readonly List<TacticalSmokeField2D> Active =
            new List<TacticalSmokeField2D>();

        [SerializeField] private float radius = 2.1f;
        [SerializeField] private int durationRounds = 3;
        [SerializeField] private Sprite smokeSprite;

        private int _expireRound;
        private float _expireRealtime;

        public float Radius => radius;

        public static bool IsLineObscured(
            Vector2 a,
            Vector2 b)
        {
            for (int i = Active.Count - 1;
                 i >= 0;
                 i--)
            {
                TacticalSmokeField2D smoke =
                    Active[i];

                if (smoke == null)
                {
                    Active.RemoveAt(i);
                    continue;
                }

                float distance =
                    DistancePointToSegment(
                        smoke.transform.position,
                        a,
                        b);

                if (distance <=
                    smoke.radius)
                {
                    return true;
                }
            }

            return false;
        }

        public void Configure(
            Sprite sprite,
            float newRadius = 2.1f,
            int rounds = 3)
        {
            smokeSprite = sprite;
            radius = newRadius;
            durationRounds = Mathf.Max(1, rounds);
            BuildVisuals();
            ResetLifetime();
        }

        private void Awake()
        {
            if (!Active.Contains(this))
                Active.Add(this);
        }

        private void Start()
        {
            BuildVisuals();
            ResetLifetime();
        }

        private void OnDestroy()
        {
            Active.Remove(this);
        }

        private void Update()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null &&
                tactical.IsTacticalCombat)
            {
                if (_expireRound <= 0)
                {
                    _expireRound =
                        tactical.RoundIndex +
                        durationRounds;
                }

                if (tactical.RoundIndex >=
                    _expireRound)
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (_expireRealtime <= 0f)
                _expireRealtime =
                    Time.unscaledTime +
                    11f;

            if (Time.unscaledTime >=
                _expireRealtime)
            {
                Destroy(gameObject);
            }
        }

        private void ResetLifetime()
        {
            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            _expireRound =
                tactical != null &&
                tactical.IsTacticalCombat
                    ? tactical.RoundIndex +
                      durationRounds
                    : 0;

            _expireRealtime =
                Time.unscaledTime +
                11f;
        }

        private void BuildVisuals()
        {
            if (transform.childCount > 0)
                return;

            Sprite sprite =
                smokeSprite != null
                    ? smokeSprite
                    : CreateDiscSprite();

            for (int i = 0;
                 i < 7;
                 i++)
            {
                GameObject puff =
                    new GameObject(
                        "Smoke Puff " + i);

                puff.transform.SetParent(
                    transform,
                    false);

                float angle =
                    i /
                    7f *
                    Mathf.PI *
                    2f;

                float distance =
                    i == 0
                        ? 0f
                        : radius *
                          Random.Range(
                              0.18f,
                              0.58f);

                puff.transform.localPosition =
                    new Vector3(
                        Mathf.Cos(angle) *
                        distance,
                        Mathf.Sin(angle) *
                        distance *
                        0.55f,
                        0f);

                float scale =
                    Random.Range(
                        0.75f,
                        1.25f);

                puff.transform.localScale =
                    Vector3.one *
                    scale;

                SpriteRenderer sr =
                    puff.AddComponent<
                        SpriteRenderer>();

                sr.sprite = sprite;
                sr.color =
                    new Color(
                        0.50f,
                        0.56f,
                        0.64f,
                        Random.Range(
                            0.22f,
                            0.36f));

                sr.sortingOrder =
                    7200 + i;
            }
        }

        private static float DistancePointToSegment(
            Vector2 point,
            Vector2 a,
            Vector2 b)
        {
            Vector2 ab =
                b - a;

            float sqr =
                ab.sqrMagnitude;

            if (sqr < 0.0001f)
                return Vector2.Distance(
                    point,
                    a);

            float t =
                Mathf.Clamp01(
                    Vector2.Dot(
                        point - a,
                        ab) /
                    sqr);

            return Vector2.Distance(
                point,
                a + ab * t);
        }

        private static Sprite CreateDiscSprite()
        {
            const int size = 32;

            Texture2D texture =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false);

            texture.name =
                "Runtime Smoke Disc";

            Color[] pixels =
                new Color[
                    size *
                    size];

            Vector2 center =
                new Vector2(
                    (size - 1) * 0.5f,
                    (size - 1) * 0.5f);

            float radius =
                size * 0.48f;

            for (int y = 0;
                 y < size;
                 y++)
            {
                for (int x = 0;
                     x < size;
                     x++)
                {
                    float d =
                        Vector2.Distance(
                            new Vector2(x, y),
                            center) /
                        radius;

                    float alpha =
                        Mathf.Clamp01(
                            1f - d);

                    alpha =
                        alpha *
                        alpha;

                    pixels[
                        y *
                        size +
                        x] =
                        new Color(
                            1f,
                            1f,
                            1f,
                            alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    size,
                    size),
                new Vector2(
                    0.5f,
                    0.5f),
                14f);
        }
    }
}
