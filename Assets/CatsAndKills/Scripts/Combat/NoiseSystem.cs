using System;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public readonly struct NoiseEvent
    {
        public readonly Vector2 Position;
        public readonly float Radius;
        public readonly GameObject Source;

        public NoiseEvent(Vector2 position, float radius, GameObject source)
        {
            Position = position;
            Radius = radius;
            Source = source;
        }
    }

    public static class NoiseSystem
    {
        public static event Action<NoiseEvent> Noise;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Noise = null;
        }

        public static void Report(Vector2 position, float radius, GameObject source)
        {
            Noise?.Invoke(new NoiseEvent(position, radius, source));
        }
    }
}
