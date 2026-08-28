using CatsAndKills.Core;
using CatsAndKills.Narrative;
using CatsAndKills.Tactical;
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

        public void Configure(
            HitscanWeapon2D w,
            WeaponDefinition[] definitions)
        {
            weapon = w;
            slots = definitions;
            InitializeRuntimeAmmo(true);
        }

        private void Awake()
        {
            // Builder-time Configure() cannot preserve these private runtime
            // arrays into Play Mode. Recreate them from the serialized slots.
            InitializeRuntimeAmmo(false);
        }

        private void InitializeRuntimeAmmo(
            bool forceRefill)
        {
            int count =
                slots != null
                    ? slots.Length
                    : 0;

            if (count <= 0)
            {
                _magazines =
                    System.Array.Empty<int>();

                _reserves =
                    System.Array.Empty<int>();

                CurrentSlot = 0;
                return;
            }

            bool needsInit =
                forceRefill ||
                _magazines == null ||
                _reserves == null ||
                _magazines.Length != count ||
                _reserves.Length != count;

            if (!needsInit)
                return;

            _magazines =
                new int[count];

            _reserves =
                new int[count];

            for (int i = 0; i < count; i++)
            {
                WeaponDefinition slot =
                    slots[i];

                if (slot == null)
                    continue;

                _magazines[i] =
                    slot.magazineSize;

                _reserves[i] =
                    slot.startingReserve;
            }

            CurrentSlot =
                Mathf.Clamp(
                    CurrentSlot,
                    0,
                    count - 1);

            if (slots[CurrentSlot] == null)
            {
                for (int i = 0; i < count; i++)
                {
                    if (slots[i] == null)
                        continue;

                    CurrentSlot = i;
                    break;
                }
            }

            if (weapon != null &&
                slots[CurrentSlot] != null)
            {
                weapon.SetDefinition(
                    slots[CurrentSlot],
                    false);

                weapon.SetAmmo(
                    _magazines[CurrentSlot],
                    _reserves[CurrentSlot]);
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f) return;
            if (NarrativeDialogueSystem.IsDialogueOpen) return;

            TacticalCombatDirector tactical =
                TacticalCombatDirector.Instance;

            if (tactical != null &&
                tactical.IsTacticalCombat &&
                !tactical.IsPlayerTurn)
            {
                return;
            }

            SaveCurrentAmmo();

            if (CKInput.Slot1Pressed) Equip(0);
            if (CKInput.Slot2Pressed) Equip(1);
            if (CKInput.Slot3Pressed) Equip(2);

            int cycle =
                CKInput.WeaponCycleDelta;

            if (cycle != 0)
                EquipWrapped(
                    CurrentSlot + cycle);
        }

        private void EquipWrapped(
            int index)
        {
            if (slots == null ||
                slots.Length == 0)
            {
                return;
            }

            int count =
                slots.Length;

            for (int attempt = 0;
                 attempt < count;
                 attempt++)
            {
                index =
                    (index % count + count) %
                    count;

                if (slots[index] != null)
                {
                    Equip(index);
                    return;
                }

                index += 1;
            }
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
            InitializeRuntimeAmmo(false);

            if (_reserves == null ||
                amount <= 0)
            {
                return;
            }

            for (int i = 0; i < _reserves.Length; i++)
                _reserves[i] += amount;

            if (weapon != null && CurrentSlot >= 0 && CurrentSlot < _reserves.Length)
                weapon.SetAmmo(_magazines[CurrentSlot], _reserves[CurrentSlot]);
        }

        public void Equip(int index, bool refill = false)
        {
            if (slots == null || index < 0 || index >= slots.Length || slots[index] == null)
                return;

            InitializeRuntimeAmmo(false);

            if (_magazines == null ||
                _reserves == null ||
                _magazines.Length != slots.Length ||
                _reserves.Length != slots.Length)
            {
                return;
            }

            if (weapon != null &&
                weapon.IsReloading &&
                index != CurrentSlot)
            {
                weapon.CancelReload();
            }

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
