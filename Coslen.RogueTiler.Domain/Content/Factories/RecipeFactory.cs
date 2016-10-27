using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public class RecipeFactory
    {
        private static RecipeFactory instance;

        private RecipeFactory()
        {
            initialize();
        }

        public static RecipeFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RecipeFactory();
                }
                return instance;
            }
        }


        public List<Recipe> Recipes { get; set; } = new List<Recipe>();

        private void initialize()
        {
            healing();
            teleportation();
            armor();
            items();
        }

        private void healing()
        {
            recipe("Healing Poultice", new List<string>
            {
                "Flower",
                "Soothing Balm"
            });

            recipe("Antidote", new List<string>
            {
                "Soothing Balm",
                "Stinger"
            });

            recipe("Soothing Balm", new List<string>
            {
                "Flower",
                "Flower",
                "Flower"
            });

            recipe("Mending Salve", new List<string>
            {
                "Soothing Balm",
                "Soothing Balm",
                "Soothing Balm"
            });

            recipe("Healing Poultice", new List<string>
            {
                "Mending Salve",
                "Mending Salve",
                "Mending Salve"
            });

            recipe("Potion of Amelioration", new List<string>
            {
                "Healing Poultice",
                "Healing Poultice",
                "Healing Poultice"
            });

            recipe("Potion of Rejuvenation", new List<string>
            {
                "Potion of Amelioration",
                "Potion of Amelioration",
                "Potion of Amelioration",
                "Potion of Amelioration"
            });
        }

        private void teleportation()
        {
            recipe("Scroll of Sidestepping", new List<string>
            {
                "Insect Wing",
                "Black Feather"
            });

            recipe("Scroll of Phasing", new List<string>
            {
                "Scroll of Sidestepping",
                "Scroll of Sidestepping"
            });

            recipe("Scroll of Teleportation", new List<string>
            {
                "Scroll of Phasing",
                "Scroll of Phasing"
            });

            recipe("Scroll of Disappearing", new List<string>
            {
                "Scroll of Teleportation",
                "Scroll of Teleportation"
            });
        }

        private void items()
        {
            recipe("RockBlade", new List<string>
            {
                "Rock",
                "Rock"
            });

            recipe("RockBall", new List<string>
            {
                "Rock",
                "Rock",
                "Rock"
            });

            recipe("RockMoon", new List<string>
            {
                "Rock",
                "Rock",
                "Rock",
                "Rock"
            });

            recipe("RockSwordBlade", new List<string>
            {
                "RockBlade",
                "RockBlade"
            });

            recipe("Knife", new List<string>
            {
                "Stick",
                "RockBlade"
            });

            recipe("Rapier", new List<string>
            {
                "Stick",
                "RockSwordBlade"
            });

            recipe("Hatchet", new List<string>
            {
                "Stick",
                "RockMoon"
            });

            recipe("Hammer", new List<string>
            {
                "Stick",
                "RockBall"
            });

            recipe("Walking Stick", new List<string>
            {
                "Stick",
                "Stick"
            });

            recipe("Pointed Stick", new List<string>
            {
                "Walking Stick",
                "RockBlade"
            });
        }

        private void armor()
        {
            recipe("Fur Cloak", new List<string>
            {
                "Fox Pelt"
            });

            recipe("Fur Cloak", new List<string>
            {
                "Fur Pelt",
                "Fur Pelt",
                "Fur Pelt"
            });

            recipe("Fur-lined Robe", new List<string>
            {
                "Robe",
                "Fur Pelt",
                "Fur Pelt"
            });

            recipe("Fur-lined Robe", new List<string>
            {
                "Robe",
                "Fox Pelt"
            });
        }

        private void recipe(object drop, List<string> ingredientNames)
        {
            List<string> produces;
            var ingredients = ItemTypeFactory.Instance.ToItemType(ingredientNames);
            Drop processedDrop = null;
            if (drop is string)
            {
                produces = new List<string> {$"Produces: {(string) drop}"};
                processedDrop = DropFactory.parseDrop((string) drop);
            }
            else
            {
                produces = new List<string>
                {
                    "May create a random piece of equipment similar to",
                    "the placed item. Add coins to improve the quality",
                    "and chance of a successful forging."
                };
                processedDrop = drop as Drop;
            }
            Recipes.Add(new Recipe(ingredients, processedDrop, produces));
        }
    }
}