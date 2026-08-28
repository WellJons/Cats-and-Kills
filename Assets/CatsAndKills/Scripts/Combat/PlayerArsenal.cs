using CatsAndKills.Core;
using UnityEngine;

namespace CatsAndKills.Combat
{
    public sealed class PlayerArsenal : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon2D weapon;
        [SerializeField] private WeaponDefinition[] slots = new WeaponDefinition[3];

        private int[] _magazines;
        private int[] _reserves;

        public int CurrentSlot { get; private set; }
        public HitscanWeapon2D Weapon => weapon;
        public WeaponDefinition Current => weapon != null ? weapon.Definition : null;

        public void Configure(HitscanWeapon2D w, WeaponDefinition[] definitions)
        {
            weapon = w;
            slots = definitions;

            _magazines = new int[slots.Length];
            _reserves = new int[slots.Length];

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                _magazines[i] = slots[i].magazineSize;
                _reserves[i] = slots[i].startingReserve;
            }

            Equip(0, true);
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;

            SaveCurrentAmmo();

            if (CKInput.Slot1Pressed) Equip(0);
            if (CKInput.Slot2Pressed) Equip(1);
            if (CKInput.Slot3Pressed) Equip(2);
        }

        private void SaveCurrentAmmo()
        {
            if (weapon == null || _magazines == null) return;
            if (CurrentSlot < 0 || CurrentSlot >= _magazines.Length) return;

            _magazines[CurrentSlot] = weapon.Magazine;
            _reserves[CurrentSlot] = weapon.Reserve;
        }

        public void AddAmmo(int amount)
        {
            if (_reserves == null || amount <= 0) return;

            for (int i = 0; i < _reserves.Length; i++)
                _reserves[i] += amount;

            if (weapon != null && CurrentSlot >= 0 && CurrentSlot < _reserves.Length)
                weapon.SetAmmo(_magazines[CurrentSlot], _reserves[CurrentSlot]);
        }

        public void Equip(int index, bool refill = false)
        {
            if (slots == null || index < 0 || index >= slots.Length || slots[index] == null)
                return;

            if (weapon != null &&
                weapon.IsReloading &&
                index != CurrentSlot)
                return;

            SaveCurrentAmmo();

            CurrentSlot = index;
            weapon?.SetDefinition(slots[index], false);

            if (refill)
            {
                _magazines[index] = slots[index].magazineSize;
                _reserves[index] = slots[index].startingReserve;
            }

            weapon?.SetAmmo(_magazines[index], _reserves[index]);
        }
    }
}
