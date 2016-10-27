using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    public class Shop
    {
        public string Name { get; set; }
        public List<ItemType> Items { get; set; }

        public Shop(string name, List<ItemType> items)
        {
            Name = name;
            Items = items;
        }

        //int get length => _items.length;

        //List<Item> get iterator => _items.iterator;

        //Item operator[](int index) => _items[index];

        //Item removeAt(int index) => _items[index].clone();

        //      /// Any item can be "added" to a shop.
        //      ///
        //      /// This just means the item is sold and the hero gains some gold. The item
        //      /// itself does not appear in the shop.
        //      // TODO: Add the item to the shop? This would let the player buy back an
        //      // erroneous sale, but it means we have to deal with making sure there is
        //      // room for it.
        //      bool tryAdd(Item item) => true;
    }
}