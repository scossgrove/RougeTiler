using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// Drops a randomly selected item near a level from a category.
    public class CategoryDrop : Drop
    {
        /// The path to the category to choose from.
        public List<string> _category;

        /// The average level of the drop.
        public int? _level;

        public CategoryDrop(List<string> category, int? level)
        {
            _category = category;
            _level = level;
        }

        public override void SpawnDrop(AddItem addItem)
        {
            // Possibly choose from the parent category.
            var categoryDepth = _category.Count - 1;
            if (categoryDepth > 1 && Rng.Instance.OneIn(10))
            {
                categoryDepth--;
            }

            // Chance of out-of-depth items.
            var level = _level;
            if (Rng.Instance.OneIn(1000))
            {
                level += Rng.Instance.Range(20, 100);
            }
            else if (Rng.Instance.OneIn(100))
            {
                level += Rng.Instance.Range(5, 20);
            }
            else if (Rng.Instance.OneIn(10))
            {
                level += Rng.Instance.Range(1, 5);
            }

            // Find all of the items at or below the max level and in the category.
            var category = _category[categoryDepth];
            var items = ItemTypeFactory.Instance.ItemTypes.Where(item => item.Value.level <= level && item.Value.categories.Contains(category)).ToList();

            if (!items.Any())
            {
                return;
            }

            // TODO: Item rarity?

            // Pick an item. Try a few times and take the best.
            var itemType = Rng.Instance.Item(items);
            for (var i = 0; i < 3; i++)
            {
                var thisType = Rng.Instance.Item(items);
                if (thisType.Value.level > itemType.Value.level)
                {
                    itemType = thisType;
                }
            }

            // Compare the item's actual level to the original desired level. If the
            // item is below that level, it increases the chances of an affix. (A weaker
            // item deeper in the dungeon is more likely to be magic.) Likewise, an
            // already-out-of-depth item is less likely to also be special.
            var levelOffset = itemType.Value.level - _level;

            addItem(Affixes.Instance.CreateItem(itemType.Value, levelOffset.Value));
        }
    }
}