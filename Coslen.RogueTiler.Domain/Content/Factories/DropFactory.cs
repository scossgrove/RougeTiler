using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public static class DropFactory
    {
        public static Drop parseDrop(string name, int? level = null)
        {
            if (level == null) return ItemDrop(name);

            // Find an item in this category so we can figure out the full path
            // to it.
            var categories = ItemTypeFactory.Instance.ItemTypes.Values.First(item => item.categories.Contains(name)).categories;

            // Only keep the prefix of the path up to the given category.
            categories = categories.Take(categories.IndexOf(name) + 1).ToList();

            if (categories == null) throw new ApplicationException($"Could not find category {name}.");
            return new CategoryDrop(categories, level);
        }

        public static Drop ItemDrop(String name)
        {
            var itemType = ItemTypeFactory.Instance.ItemTypes[name];
            if (itemType == null) throw new ApplicationException($"Could not find item type {name}.");

            // See if the item is in a group.
            return new ItemDrop(itemType);
        }

        /// Creates a single drop [Rarity].
        public static Rarity RarityDrop(int rarity, String name, int? level = null)
        {
            return new Rarity(rarity, parseDrop(name, level));
        }

        /// Creates a [Drop] that has a [chance]% chance of dropping [drop].
        public static Drop PercentDrop(int chance, string drop, int? level = null)
        {
            return new PercentDrop(chance, parseDrop(drop, level));
        }

        /// Creates a [Drop] that drops all of [drops].
        public static Drop dropAllOf(List<Drop> drops) => new AllOfDrop(drops);

        /// Creates a [Drop] that has a chance to drop one of [drops] each with its
        /// own [Frequency].
        public static Drop dropOneOf(List<Rarity> drops) => new RarityDrop(drops);

        public static Drop repeatDrop(int count, Drop drop) => new RepeatDrop(count, drop);

    }
}
