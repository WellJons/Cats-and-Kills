using UnityEngine;

namespace CatsAndKills.FX
{
    public sealed class TracerFX2D : MonoBehaviour
    {
        private LineRenderer _line;
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
            if (shader != null)
                _line.material = new Material(shader);

            _line.startColor = color;
            _line.endColor = new Color(
                color.r,
                color.g,
                color.b,
                0.05f);
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

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
