using UnityEngine;

namespace CatsAndKills.Damage
{
    public enum BodyPart
    {
        Head,
        Torso,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    public enum DamageType
    {
        Bullet,
        Pellet,
        Explosion,
        Impact
    }

    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector2 Point;
        public readonly Vector2 Direction;
        public readonly float Force;
        public readonly float DismemberPower;
        public readonly DamageType Type;
        public readonly GameObject Source;

        public DamageInfo(
            float amount,
            Vector2 point,
            Vector2 direction,
            float force,
            GameObject source,
            DamageType type = DamageType.Bullet,
            float dismemberPower = 0f)
        {
            Amount = amount;
            Point = point;
            Direction = direction;
            Force = force;
            Source = source;
            Type = type;
            DismemberPower = dismemberPower;
        }
    }

    public interface IDamageReceiver
    {
        void ReceiveDamage(DamageInfo info);
    }
}
