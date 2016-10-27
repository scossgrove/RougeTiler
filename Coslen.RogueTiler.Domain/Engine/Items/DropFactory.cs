using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    public static class DropFactory
    {
        public static Drop ItemDrop(string name)
        {
            var itemType = ItemTypeFactory.Instance.ItemTypes[name];
            if (itemType == null)
            {
                throw new ApplicationException("Could not find item type \"" + name + "\".");
            }
            // See if the item is in a group.
            return new ItemDrop(itemType);
        }

        public static Drop parseDrop(string name, int? level = null)
        {
            if (level == null || level == 0)
            {
                return ItemDrop(name);
            }

            // Find an item in this category so we can figure out the full path
            // to it.

            var firstItem = ItemTypeFactory.Instance.ItemTypes.Values.First(item => item.categories.Contains(name.ToLower()));
            var categories = firstItem.categories;

            // Only keep the prefix of the path up to the given category.
            categories = categories.Take(categories.IndexOf(name) + 1).ToList();

            if (categories == null)
            {
                throw new ApplicationException("Could not find category \"" + name + "\".");
            }
            return new CategoryDrop(categories, level);
        }

        /// Creates a single drop [Rarity].
        public static Rarity rarity(int rarity, string name, int? level)
        {
            return new Rarity(rarity, parseDrop(name, level));
        }

        /// Creates a [Drop] that has a [chance]% chance of dropping [drop].
        public static Drop percent(int chance, string drop, int? level = null)
        {
            return new PercentDrop(chance, parseDrop(drop, level));
        }

        /// Creates a [Drop] that drops all of [drops].
        public static Drop dropAllOf(List<Drop> drops)
        {
            return new AllOfDrop(drops);
        }

        /// Creates a [Drop] that has a chance to drop one of [drops] each with its
        /// own [Frequency].
        public static Drop dropOneOf(List<Rarity> drops)
        {
            return new RarityDrop(drops);
        }

        public static Drop repeatDrop(int count, Drop drop)
        {
            return new RepeatDrop(count, drop);
        }
    }
}