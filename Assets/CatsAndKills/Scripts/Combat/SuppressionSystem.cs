using System;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public readonly struct SuppressionEvent
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly float Strength;
        public readonly GameObject Source;

        public SuppressionEvent(Vector2 start, Vector2 end, float strength, GameObject source)
        {
            Start = start;
            End = end;
            Strength = strength;
            Source = source;
        }
    }

    public static class SuppressionSystem
    {
        public static event Action<SuppressionEvent> ShotPassed;

        public static void ReportShot(Vector2 start, Vector2 end, float strength, GameObject source)
        {
            ShotPassed?.Invoke(new SuppressionEvent(start, end, Mathf.Max(0f, strength), source));
        }

        public static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSq = ab.sqrMagnitude;
            if (lengthSq < 0.0001f)
                return Vector2.Distance(point, a);

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSq);
            return Vector2.Distance(point, a + ab * t);
        }
    }
}
