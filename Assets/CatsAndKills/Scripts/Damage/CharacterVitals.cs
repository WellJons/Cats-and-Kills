using System;
using UnityEngine;

namespace CatsAndKills.Damage
{
    public sealed class CharacterVitals : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float armCapacity = 42f;
        [SerializeField] private float legCapacity = 48f;

        public float Health { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }

        public float LeftArm { get; private set; }
        public float RightArm { get; private set; }
        public float LeftLeg { get; private set; }
        public float RightLeg { get; private set; }

        public bool LeftArmDisabled => LeftArm <= 0f;
        public bool RightArmDisabled => RightArm <= 0f;
        public bool LeftLegDisabled => LeftLeg <= 0f;
        public bool RightLegDisabled => RightLeg <= 0f;

        public bool CanUsePrimaryWeapon => !(LeftArmDisabled && RightArmDisabled);

        public float WeaponStabilityMultiplier
        {
            get
            {
                if (LeftArmDisabled && RightArmDisabled) return 3.2f;
                if (LeftArmDisabled || RightArmDisabled) return 1.65f;
                return 1f;
            }
        }

        public float MovementMultiplier
        {
            get
            {
                if (LeftLegDisabled && RightLegDisabled) return 0.24f;
                if (LeftLegDisabled || RightLegDisabled) return 0.58f;
                return 1f;
            }
        }

        private GameObject _lastDamageSource;

        public event Action<DamageInfo> Damaged;
        public event Action Died;
        public event Action<BodyPart, DamageInfo> LimbDisabled;
        public event Action<BodyPart, DamageInfo> Dismembered;

        private void Awake()
        {
            ResetVitals();
        }

        public void Configure(float health, float arms, float legs)
        {
            maxHealth = health;
            armCapacity = arms;
            legCapacity = legs;
            ResetVitals();
        }

        private void ResetVitals()
        {
            Health = maxHealth;
            LeftArm = RightArm = armCapacity;
            LeftLeg = RightLeg = legCapacity;
        }

        public void ReceiveDamage(DamageInfo info)
        {
            ApplyDamage(BodyPart.Torso, info, 1f);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Health = Mathf.Min(maxHealth, Health + amount);
        }

        public void ApplyDamage(BodyPart part, DamageInfo info, float multiplier)
        {
            if (IsDead) return;

            float damage = Mathf.Max(0f, info.Amount * multiplier);
            if (part == BodyPart.Head) damage *= 1.65f;

            Health -= damage;
            _lastDamageSource = info.Source;

            switch (part)
            {
                case BodyPart.LeftArm: UpdateLimb(part, ref LeftArm, damage, info); break;
                case BodyPart.RightArm: UpdateLimb(part, ref RightArm, damage, info); break;
                case BodyPart.LeftLeg: UpdateLimb(part, ref LeftLeg, damage, info); break;
                case BodyPart.RightLeg: UpdateLimb(part, ref RightLeg, damage, info); break;
            }

            Damaged?.Invoke(info);

            bool explosionDismember =
                info.Type == DamageType.Explosion &&
                info.DismemberPower >= 0.55f &&
                part != BodyPart.Head &&
                part != BodyPart.Torso;

            if (explosionDismember)
                Dismembered?.Invoke(part, info);

            if (Health <= 0f || (part == BodyPart.Head && damage >= 70f))
                Die();
        }

        private void UpdateLimb(BodyPart part, ref float limb, float damage, DamageInfo info)
        {
            bool wasFunctional = limb > 0f;
            limb = Mathf.Max(0f, limb - damage);

            if (wasFunctional && limb <= 0f)
                LimbDisabled?.Invoke(part, info);
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            if (!CompareTag("Player") &&
                _lastDamageSource != null &&
                _lastDamageSource.transform.root.CompareTag("Player"))
            {
                CatsAndKills.Core.CombatStats.Instance?.RecordKill();
            }

            Died?.Invoke();
        }
    }
}
