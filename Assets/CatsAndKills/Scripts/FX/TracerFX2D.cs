using UnityEngine;

namespace CatsAndKills.FX
{
    public sealed class TracerFX2D : MonoBehaviour
    {
        private LineRenderer _line;
        private LineRenderer _glow;
        private float _life = 0.045f;
        private float _elapsed;

        public void Configure(
            Vector2 start,
            Vector2 end,
            Color color,
            float width,
            float life = 0.045f)
        {
            _life = life;

            _line = gameObject.AddComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.useWorldSpace = true;
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
            _line.startWidth = width;
            _line.endWidth = width * 0.38f;
            _line.numCapVertices = 2;
            _line.sortingOrder = 28;

            Shader shader = Shader.Find("Sprites/Default");
            Material shared = null;

            if (shader != null)
            {
                shared = new Material(shader);
                _line.material = shared;
            }

            _line.startColor = color;
            _line.endColor = new Color(
                color.r,
                color.g,
                color.b,
                0.05f);

            _glow =
                gameObject.AddComponent<LineRenderer>();

            _glow.positionCount = 2;
            _glow.useWorldSpace = true;
            _glow.SetPosition(0, start);
            _glow.SetPosition(1, end);
            _glow.startWidth = width * 3.4f;
            _glow.endWidth = width * 1.5f;
            _glow.numCapVertices = 2;
            _glow.sortingOrder = 27;

            if (shared != null)
                _glow.sharedMaterial = shared;

            _glow.startColor =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.26f);

            _glow.endColor =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.01f);
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.001f, _life));

            if (_line != null)
            {
                Color start = _line.startColor;
                start.a = 1f - t;
                _line.startColor = start;

                Color end = _line.endColor;
                end.a = (1f - t) * 0.15f;
                _line.endColor = end;
            }

            if (_glow != null)
            {
                Color start = _glow.startColor;
                start.a = (1f - t) * 0.26f;
                _glow.startColor = start;

                Color end = _glow.endColor;
                end.a = (1f - t) * 0.04f;
                _glow.endColor = end;
            }

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
