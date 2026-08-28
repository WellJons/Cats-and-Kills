using System;
using System.Collections.Generic;
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
        private readonly HashSet<BodyPart> _dismembered =
            new HashSet<BodyPart>();

        public bool IsDismembered(BodyPart part) =>
            _dismembered.Contains(part);

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
            IsDead = false;
            _lastDamageSource = null;

            Health = maxHealth;
            LeftArm = RightArm = armCapacity;
            LeftLeg = RightLeg = legCapacity;
            _dismembered.Clear();
        }

        public void ReceiveDamage(DamageInfo info)
        {
            ApplyDamage(BodyPart.Torso, info, 1f);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            Health = Mathf.Min(maxHealth, Health + amount);

            float limbHeal = amount * 0.38f;

            if (!IsDismembered(BodyPart.LeftArm))
                LeftArm = Mathf.Min(armCapacity, LeftArm + limbHeal);

            if (!IsDismembered(BodyPart.RightArm))
                RightArm = Mathf.Min(armCapacity, RightArm + limbHeal);

            if (!IsDismembered(BodyPart.LeftLeg))
                LeftLeg = Mathf.Min(legCapacity, LeftLeg + limbHeal);

            if (!IsDismembered(BodyPart.RightLeg))
                RightLeg = Mathf.Min(legCapacity, RightLeg + limbHeal);
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
                case BodyPart.LeftArm:
                    LeftArm = UpdateLimb(part, LeftArm, damage, info);
                    break;

                case BodyPart.RightArm:
                    RightArm = UpdateLimb(part, RightArm, damage, info);
                    break;

                case BodyPart.LeftLeg:
                    LeftLeg = UpdateLimb(part, LeftLeg, damage, info);
                    break;

                case BodyPart.RightLeg:
                    RightLeg = UpdateLimb(part, RightLeg, damage, info);
                    break;
            }

            Damaged?.Invoke(info);

            bool explosionDismember =
                info.Type == DamageType.Explosion &&
                info.DismemberPower >= 0.55f &&
                part != BodyPart.Head &&
                part != BodyPart.Torso;

            if (explosionDismember && !_dismembered.Contains(part))
            {
                _dismembered.Add(part);

                switch (part)
                {
                    case BodyPart.LeftArm:
                        LeftArm = 0f;
                        break;
                    case BodyPart.RightArm:
                        RightArm = 0f;
                        break;
                    case BodyPart.LeftLeg:
                        LeftLeg = 0f;
                        break;
                    case BodyPart.RightLeg:
                        RightLeg = 0f;
                        break;
                }

                Dismembered?.Invoke(part, info);
            }

            if (Health <= 0f || (part == BodyPart.Head && damage >= 70f))
                Die();
        }

        private float UpdateLimb(
            BodyPart part,
            float limb,
            float damage,
            DamageInfo info)
        {
            bool wasFunctional = limb > 0f;
            float updated = Mathf.Max(0f, limb - damage);

            if (wasFunctional && updated <= 0f)
                LimbDisabled?.Invoke(part, info);

            return updated;
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
