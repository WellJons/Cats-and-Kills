using System.Collections;
using UnityEngine;

namespace CatsAndKills.FX
{
    public sealed class DebrisLifetime2D : MonoBehaviour
    {
        private float _life = 4f;
        private bool _fade;

        public void SetLifetime(float value, bool fade = false)
        {
            _life = value;
            _fade = fade;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_life);

            if (_fade)
            {
                var sr = GetComponent<SpriteRenderer>();
                float t = 0f;
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = Mathf.Lerp(1f, 0f, t / 0.5f);
                        sr.color = c;
                    }
                    yield return null;
                }
            }

            Destroy(gameObject);
        }
    }
}
