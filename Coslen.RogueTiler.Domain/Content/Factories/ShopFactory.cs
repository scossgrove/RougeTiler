using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public class ShopFactory
    {
        private static ShopFactory instance;

        private ShopFactory()
        {
            initialize();
        }

        public static ShopFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShopFactory();
                }
                return instance;
            }
        }

        public Dictionary<string, Shop> Shops { get; set; } = new Dictionary<string, Shop>();

        private void initialize()
        {
            shop("The General's General Store", new List<string>
            {
                "Club",
                "Staff",
                "Quarterstaff",
                "Whip",
                "Dagger",
                "Hatchet",
                "Axe",
                "Sling"
            });

            shop("Dirk's Death Emporium", new List<string>
            {
                "Hammer",
                "Mattock",
                "War Hammer",
                "Morningstar",
                "Mace",
                "Chain Whip",
                "Flail",
                "Falchion",
                "Rapier",
                "Shortsword",
                "Scimitar",
                "Cutlass",
                "Spear",
                "Angon",
                "Lance",
                "Partisan",
                "Valaska",
                "Battleaxe",
                "Short Bow",
                "Longbow",
                "Crossbow"
            });

            shop("Skullduggery and Bamboozelry", new List<string>
            {
                "Dirk",
                "Dagger",
                "Stiletto",
                "Rondel",
                "Baselard"
            });

            shop("Garthag's Armoury", new List<string>
            {
                "Cloak",
                "Fur Cloak",
                "Cloth Shirt",
                "Leather Shirt",
                "Jerkin",
                "Leather Armor",
                "Padded Armor",
                "Studded Leather Armor",
                "Mail Hauberk",
                "Scale Mail",
                "Robe",
                "Fur-lined Robe",
                "Leather Sandals",
                "Leather Shoes",
                "Leather Boots",
                "Metal Shod Boots",
                "Greaves"
            });

            /*
              Unguence the Alchemist
              The Droll Magery
              Glur's Rare Artifacts
            */
        }

        private void shop(string name, List<string> itemTypes)
        {
            var items = ItemTypeFactory.Instance.ToItemType(itemTypes);
            Shops.Add(name, new Shop(name, items));
        }
    }
}