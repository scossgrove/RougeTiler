using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Utilities;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    public class Recipe
    {
        public List<ItemType> Ingredients { get; set; } 

        public Drop Result { get; set; }

        // TODO: Instead of hard-coding the word wrapping here, wrap it in the UI.
        /// If this recipe results in a specific item, [produces] will store that
        /// item's name. Otherwise, [produces] will be null.
        public List<string> Produces { get; set; }

        public Recipe(List<ItemType> ingredients, Drop result, List<string> produces)
        {
            Ingredients = ingredients;
            Result = result;
            Produces = produces;
        }

        /// Returns `true` if [items] are valid (but not necessarily complete)
        /// ingredients for this recipe.
        public bool Allows(List<Item> items) => HasCorrectIngredients(items);

        /// Returns `true` if [items] are the complete ingredients needed for this
        /// recipe.
        public bool IsComplete(List<Item> items)
        {
            var missing = GetMissingIngredients(items);
            // Check for
            // 1. no item type is missing
            // 2. no extra items are present
            return missing != null && missing.Count == 0 && items.Count == Ingredients.Count;
        }

        public bool HasCorrectIngredients(List<Item> items)
        {
            var isValid = true;

            // This is the list of items passed in.
            var types = items.Select(i => i.type.name).Distinct().ToList();
            var requiredTypes = Ingredients.Select(i => i.name).Distinct().ToList();

            var remainingTypes = types.Where(p => !requiredTypes.Any(p2 => p2 == p)).ToList();
            

            return remainingTypes.Any() == false;
        }


        /// Gets the remaining ingredients needed to complete this recipe given [items] ingredients. 
        /// Returns `null` if [items] contains invalid ingredients.
        public List<ItemType> GetMissingIngredients(List<Item> items)
        {
            var missing = Ingredients.Clone().ToList();
            if (items == null)
            {
                return missing.ToList();
            }

            foreach (var itemType in items.Select(i => i.type))
            {
                missing.Remove(itemType);
            }
            
            return missing.ToList();
        }
    }
}
