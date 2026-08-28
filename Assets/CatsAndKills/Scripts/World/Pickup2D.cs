using CatsAndKills.Combat;
using CatsAndKills.Damage;
using CatsAndKills.Player;
using UnityEngine;

namespace CatsAndKills.World
{
    public enum PickupType
    {
        Ammo,
        Medkit,
        Grenades
    }

    public sealed class Pickup2D : MonoBehaviour
    {
        [SerializeField] private PickupType type;
        [SerializeField] private int amount = 1;
        [SerializeField] private float rotateSpeed = 45f;

        public void Configure(PickupType pickupType, int pickupAmount)
        {
            type = pickupType;
            amount = pickupAmount;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerMotor2D player = other.GetComponentInParent<PlayerMotor2D>();
            if (player == null) return;

            switch (type)
            {
                case PickupType.Ammo:
                {
                    var arsenal = player.GetComponent<PlayerArsenal>();
                    if (arsenal == null) return;
                    arsenal.AddAmmo(amount);
                    break;
                }

                case PickupType.Medkit:
                {
                    var vitals = player.GetComponent<CharacterVitals>();
                    if (vitals == null) return;
                    vitals.Heal(amount);
                    break;
                }

                case PickupType.Grenades:
                {
                    var grenades = player.GetComponent<PlayerGrenadeController>();
                    if (grenades == null) return;
                    grenades.AddGrenades(amount);
                    break;
                }
            }

            Destroy(gameObject);
        }
    }
}
