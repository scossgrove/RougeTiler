using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;
using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Content
{
    public abstract class Content
    {
    }

    public class GameContent : Content
    {
        private static GameContent instance;

        private GameContent()
        {}

        public static GameContent Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameContent();
                }
                return instance;
            }
        }

        public Dictionary<string, Area> areas
        {
            get { return AreaFactory.Instance.Areas; }
        }

        public Dictionary<string, Breed> breeds
        {
            get { return BreedFactory.Instance.Breeds; }
        }

        public Dictionary<string, ItemType> items
        {
            get { return ItemTypeFactory.Instance.ItemTypes; }
        }

        public List<Recipe> Recipes
        {
            get { return RecipeFactory.Instance.Recipes; }
        } 

        //public List<Shop> get shops => Shops.all;

        public HeroSave createHero(string name, HeroClass heroClass)
        {
            var hero = new HeroSave(name, heroClass);

            // Starting Items
            var startingItems = new List<ItemType>();
            startingItems.Add(ItemTypeFactory.Instance.ItemTypes["Mending Salve"]);
            startingItems.Add(ItemTypeFactory.Instance.ItemTypes["Scroll of Sidestepping"]);
            startingItems.Add(ItemTypeFactory.Instance.ItemTypes["Stick"]);

            foreach (var itemType in startingItems)
            {
                hero.Inventory.tryAdd(Affixes.Instance.CreateItem(itemType));
            }

            // Starting Stats
            hero.Health = new Stat(Option.HeroHealthStart, Option.HeroHealthStart);
            hero.Charge = new Stat(0, Option.HeroChargeStart);
            hero.Gold = Option.HeroGoldStart;

            return hero;
        }
    }
}