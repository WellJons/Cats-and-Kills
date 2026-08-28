using UnityEngine;

namespace CatsAndKills.Damage
{
    public sealed class BodyPartHitbox : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private CharacterVitals owner;
        [SerializeField] private BodyPart bodyPart = BodyPart.Torso;
        [SerializeField] private float damageMultiplier = 1f;

        public CharacterVitals Owner => owner;
        public BodyPart Part => bodyPart;

        public void Configure(CharacterVitals newOwner, BodyPart part, float multiplier)
        {
            owner = newOwner;
            bodyPart = part;
            damageMultiplier = multiplier;
        }

        private void Awake()
        {
            if (owner == null) owner = GetComponentInParent<CharacterVitals>();
        }

        public void ReceiveDamage(DamageInfo info)
        {
            owner?.ApplyDamage(bodyPart, info, damageMultiplier);
        }
    }
}
