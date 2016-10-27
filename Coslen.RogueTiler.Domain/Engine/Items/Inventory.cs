using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    public abstract class ItemCollection : List<Item>
    {
        //    public Item operator[](int index) { return null; }
        //    public Item removeAt(int index) { return null; }
        public bool tryAdd(Item item)
        {
            return false;
        }
    }

    /// The collection of [Item]s held by an [Actor].
    public class Inventory : ItemCollection
    {
        private readonly int capacity;
        private Item _lastUnequipped;

        public Inventory(int capacity)
        {
            this.capacity = capacity;
        }

        /// If the [Hero] had to unequip an item in order to equip another one, this
        /// will refer to the index of the item that was unequipped.
        /// 
        /// If the hero isn't holding an unequipped item, returns `-1`.
        public int lastUnequipped
        {
            get { return IndexOf(_lastUnequipped); }
        }

        /// Creates a new copy of this Inventory. This is done when the [Hero] enters
        /// a [Stage] so that any inventory changes that happen in the stage are
        /// discarded if the hero dies.
        public Inventory clone()
        {
            // TODO: If items themselves ever become mutable, will need to deep
            // clone them too.
            var inventory = new Inventory(capacity);
            foreach (var item in this)
            {
                inventory.tryAdd(item);
            }
            return inventory;
        }

        /// Removes all items from the inventory.
        public void clear()
        {
            clear();
            _lastUnequipped = null;
        }

        public Item removeAt(int index)
        {
            var item = this[index];
            RemoveAt(index);
            if (_lastUnequipped == item)
            {
                _lastUnequipped = null;
            }
            return item;
        }

        public new void tryAdd(Item item)
        {
             tryAdd(item, false);
        }

        public bool tryAdd(Item item, bool wasUnequipped = false)
        {
            // TODO: Merge stacks.
            if (Count >= capacity)
            {
                return false;
            }

            Add(item);
            Sort();

            if (wasUnequipped)
            {
                _lastUnequipped = item;
            }

            return true;
        }
    }
}