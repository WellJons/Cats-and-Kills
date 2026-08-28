using System.Collections.Generic;
using CatsAndKills.Damage;
using CatsAndKills.Tactical;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class TacticalFireField2D : MonoBehaviour
    {
        private static readonly List<TacticalFireField2D> Active =
            new List<TacticalFireField2D>();

        [SerializeField] private float cellRadius = 0.48f;
        [SerializeField] private int durationRounds = 3;
        [SerializeField] private float damagePerPhase = 11f;
        [SerializeField] private float cellSize = 0.85f;
        [SerializeField] private GameObject owner;

        private readonly List<Vector2> _cells =
            new List<Vector2>();

        private int _expireRound;
        private float _expireRealtime;
        private TacticalCombatDirector _tactical;

        public static bool IsDangerousPoint(
            Vector2 point)
        {
            for (int i = Active.Count - 1;
                 i >= 0;
                 i--)
            {
                TacticalFireField2D fire =
                    Active[i];

                if (fire == null)
                {
                    Active.RemoveAt(i);
                    continue;
                }

                for (int c = 0;
                     c < fire._cells.Count;
                     c++)
                {
                    if (Vector2.Distance(
                            point,
                            fire._cells[c]) <=
                        fire.cellRadius +
                        0.20f)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Configure(
            GameObject source,
            float tacticalCellSize,
            int rounds = 3)
        {
            owner = source;
            cellSize =
                Mathf.Max(
                    0.5f,
                    tacticalCellSize);

            durationRounds =
                Mathf.Max(
                    1,
                    rounds);

            BuildPattern();
            BuildVisuals();
            Bind();
        }

        private void Awake()
        {
            if (!Active.Contains(this))
                Active.Add(this);
        }

        private void Start()
        {
            BuildPattern();
            BuildVisuals();
            Bind();
        }

        private void OnDestroy()
        {
            Active.Remove(this);

            if (_tactical != null)
                _tactical.PhaseChanged -=
                    OnPhaseChanged;
        }

        private void Update()
        {
            if (_tactical == null)
                _tactical =
                    TacticalCombatDirector.Instance;

            if (_tactical != null &&
                _tactical.IsTacticalCombat)
            {
                if (_expireRound <= 0)
                {
                    _expireRound =
                        _tactical.RoundIndex +
                        durationRounds;
                }

                if (_tactical.RoundIndex >=
                    _expireRound)
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (_expireRealtime <= 0f)
                _expireRealtime =
                    Time.unscaledTime +
                    10f;

            if (Time.unscaledTime >=
                _expireRealtime)
            {
                Destroy(gameObject);
            }
        }

        private void Bind()
        {
            _tactical =
                TacticalCombatDirector.Instance;

            if (_tactical != null)
            {
                _tactical.PhaseChanged -=
                    OnPhaseChanged;

                _tactical.PhaseChanged +=
                    OnPhaseChanged;

                _expireRound =
                    _tactical.IsTacticalCombat
                        ? _tactical.RoundIndex +
                          durationRounds
                        : 0;
            }

            _expireRealtime =
                Time.unscaledTime +
                10f;
        }

        private void OnPhaseChanged()
        {
            ApplyBurnDamage();
        }

        private void ApplyBurnDamage()
        {
            var damaged =
                new HashSet<CharacterVitals>();

            for (int i = 0;
                 i < _cells.Count;
                 i++)
            {
                Collider2D[] hits =
                    Physics2D.OverlapCircleAll(
                        _cells[i],
                        cellRadius);

                foreach (Collider2D hit in hits)
                {
                    if (hit == null)
                        continue;

                    CharacterVitals vitals =
                        hit.GetComponentInParent<
                            CharacterVitals>();

                    if (vitals == null ||
                        vitals.IsDead ||
                        damaged.Contains(vitals))
                    {
                        continue;
                    }

                    damaged.Add(vitals);

                    Vector2 direction =
                        ((Vector2)vitals.transform.position -
                         _cells[i]);

                    if (direction.sqrMagnitude <
                        0.01f)
                    {
                        direction = Vector2.up;
                    }

                    vitals.ReceiveDamage(
                        new DamageInfo(
                            damagePerPhase,
                            vitals.transform.position,
                            direction.normalized,
                            0.4f,
                            owner,
                            DamageType.Impact,
                            0f));

                    FXService.Instance?.BloodBurst(
                        vitals.transform.position,
                        direction.normalized,
                        2,
                        0.30f);
                }
            }
        }

        private void BuildPattern()
        {
            if (_cells.Count > 0)
                return;

            Vector2 center =
                transform.position;

            _cells.Add(center);
            _cells.Add(center + Vector2.right * cellSize);
            _cells.Add(center + Vector2.left * cellSize);
            _cells.Add(center + Vector2.up * cellSize);
            _cells.Add(center + Vector2.down * cellSize);
        }

        private void BuildVisuals()
        {
            if (transform.childCount > 0)
                return;

            Sprite sprite =
                CreateFireSprite();

            for (int i = 0;
                 i < _cells.Count;
                 i++)
            {
                GameObject flame =
                    new GameObject(
                        "Burning Cell " + i);

                flame.transform.SetParent(
                    transform,
                    false);

                flame.transform.position =
                    _cells[i];

                SpriteRenderer sr =
                    flame.AddComponent<
                        SpriteRenderer>();

                sr.sprite = sprite;
                sr.color =
                    new Color(
                        1f,
                        Random.Range(
                            0.25f,
                            0.52f),
                        0.04f,
                        0.78f);

                sr.sortingOrder =
                    6800 + i;

                flame.transform.localScale =
                    new Vector3(
                        cellSize * 0.9f,
                        cellSize * 0.58f,
                        1f);

                LightPulse2D pulse =
                    flame.AddComponent<
                        LightPulse2D>();

                pulse.Configure();
            }
        }

        private static Sprite CreateFireSprite()
        {
            const int size = 24;

            Texture2D texture =
                new Texture2D(
                    size,
                    size,
                    TextureFormat.RGBA32,
                    false);

            texture.name =
                "Runtime Fire Cell";

            Color[] pixels =
                new Color[
                    size *
                    size];

            Vector2 center =
                new Vector2(
                    (size - 1) * 0.5f,
                    (size - 1) * 0.42f);

            for (int y = 0;
                 y < size;
                 y++)
            {
                for (int x = 0;
                     x < size;
                     x++)
                {
                    Vector2 p =
                        new Vector2(
                            x,
                            y);

                    float dx =
                        Mathf.Abs(
                            p.x -
                            center.x) /
                        (size * 0.45f);

                    float dy =
                        Mathf.Abs(
                            p.y -
                            center.y) /
                        (size * 0.32f);

                    float alpha =
                        Mathf.Clamp01(
                            1f -
                            dx * dx -
                            dy * dy);

                    pixels[
                        y *
                        size +
                        x] =
                        new Color(
                            1f,
                            0.45f,
                            0.08f,
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
                size);
        }
    }

    public sealed class LightPulse2D : MonoBehaviour
    {
        private UnityEngine.Rendering.Universal.Light2D _light;
        private float _phase;

        public void Configure()
        {
            if (_light != null)
                return;

            _light =
                gameObject.AddComponent<
                    UnityEngine.Rendering.Universal.Light2D>();

            _light.lightType =
                UnityEngine.Rendering.Universal.Light2D.LightType.Point;

            _light.color =
                new Color(
                    1f,
                    0.22f,
                    0.04f);

            _light.pointLightOuterRadius = 1.6f;
            _light.pointLightInnerRadius = 0.15f;
            _light.intensity = 0.8f;

            _phase =
                Random.Range(
                    0f,
                    Mathf.PI *
                    2f);
        }

        private void Update()
        {
            if (_light == null)
                return;

            _light.intensity =
                0.68f +
                Mathf.Sin(
                    Time.unscaledTime *
                    11f +
                    _phase) *
                0.16f;
        }
    }
}
