using System;
using System.Collections.Generic;
using System.Linq;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    public enum EquipementSlot
    {
        weapon,
        bow,
        ring,
        necklace,
        body,
        cloak,
        shield,
        helm,
        gloves,
        boots
    };

    /// The collection of wielded [Item]s held by the [Hero]. Unlike [Inventory],
    /// the [Equipment] holds each Item in a categorized slot.
    public class Equipment
    {
        private int numberOfSlots = 10;

        public Dictionary<EquipementSlot, Item> slots;

        public Equipment()
        {
            slots = new Dictionary<EquipementSlot, Item>();
            foreach (var slotType in Enum.GetValues(typeof (EquipementSlot)))
            {
                slots.Add((EquipementSlot) slotType, null);
            }
        }

        public List<Item> Items
        {
            get { return slots.Values.ToList(); }
        }

        public Item this[EquipementSlot type]
        {
            get { return slots[type]; }
            set { slots[type] = value; }
        }

        public Item this[int slotNumber]
        {
            get { return slots[(EquipementSlot) slotNumber]; }
            set { slots[(EquipementSlot) slotNumber] = value; }
        }

        /// Gets the [Item] in the weapon slot, if any.
        public Item Weapon
        {
            get { return slots[EquipementSlot.weapon]; }
        }

        /// Gets the number of equipped item. Ignores empty slots.
        public int length
        {
            get { return slots.Count(item => item.Value != null); }
        }

//        /// Gets the equipped item at the given index. Ignores empty slots.
//        public Item operator[](int index) {
//            // Find the slot, skipping over empty ones.
//            for (var i = 0; i< slotTypes.length; i++) {
//              if (slots[i] != null) {
//                if (index == 0) return slots[i];
//                index--;
//              }
//}

//            throw "unreachable";
//          }

        /// Creates a new copy of this Equipment. This is done when the [Hero] enters
        /// a [Level] so that any inventory changes that happen in the level are
        /// discarded if the hero dies.
        public Equipment clone()
        {
            // TODO: If items themselves ever become mutable, will need to deep
            // clone them too.
            var equipment = new Equipment();
            foreach (var entry in slots)
            {
                equipment.slots[entry.Key] = entry.Value;
            }

            return equipment;
        }

        /// Gets the [Item] currently equipped in [slotType], if any.
        public Item find(EquipementSlot slotType)
        {
            return slots[slotType];
        }

        /// Gets whether or not there is a slot to equip [item].
        public bool canEquip(Item item)
        {
            return Enum.IsDefined(typeof (EquipementSlot), item.equipSlot);
        }

        /// Tries to add the item. This will only succeed if there is an empty slot
        /// that allows the item. Unlike [equip], this will not swap items. It is
        /// used by the [HomeScreen].
        public bool tryAdd(Item item)
        {
            // TODO: Need to handle multiple slots of the same type. In that case,
            // should prefer an empty slot before reusing an in-use one.
            var targetSlot = (EquipementSlot) Enum.Parse(typeof (EquipementSlot), item.equipSlot);
            if (slots[targetSlot] == null)
            {
                slots[targetSlot] = item;
                return true;
            }

            return false;
        }

        /// Equips [item]. Returns the previously equipped item in that slot, if any.
        public Item equip(Item item)
        {
            // TODO: Need to handle multiple slots of the same type. In that case,
            // should prefer an empty slot before reusing an in-use one.
            var targetSlot = (EquipementSlot) Enum.Parse(typeof (EquipementSlot), item.equipSlot);
            if (slots[targetSlot] == null)
            {
                slots[targetSlot] = item;
                return null;
            }
            var unequipped = slots[targetSlot];
            slots[targetSlot] = item;
            return unequipped;
        }

        /// Equips [item]. Returns the previously equipped item in that slot, if any.
        public void replace(Item item)
        {
            // TODO: Need to handle multiple slots of the same type. In that case,
            // should prefer an empty slot before reusing an in-use one.
            var targetSlot = (EquipementSlot)Enum.Parse(typeof(EquipementSlot), item.equipSlot);
            if (slots[targetSlot] == null)
            {
                slots[targetSlot] = item;
                return;
            }
            return;
        }

        /// Unequips and returns the [Item] at [index].
        public Item removeAt(EquipementSlot slotType)
        {
            var item = slots[slotType];
            slots[slotType] = null;
            return item;
        }
    }
}