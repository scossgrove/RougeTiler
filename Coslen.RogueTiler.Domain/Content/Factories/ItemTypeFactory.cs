using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    public class ItemTypeFactory
    {
        // colour constants
        private const string aqua = "aqua";
        private const string darkAqua = "darkAqua";
        private const string brown = "brown";
        private const string darkGold = "darkGold";
        private const string gray = "gray";
        private const string lightGold = "lightGold";
        private const string gold = "gold";
        private const string lightGray = "lightGray";
        private const string lightAqua = "lightAqua";
        private const string lightBrown = "lightBrown";
        private const string purple = "purple";
        private const string orange = "orange";
        private const string red = "red";
        private const string darkGray = "darkGray";
        private const string lightRed = "lightRed";
        private const string darkRed = "darkRed";
        private const string darkPurple = "darkPurple";
        private const string green = "green";
        private const string lightGreen = "lightGreen";
        private const string darkGreen = "darkGreen";
        private const string lightBlue = "lightBlue";
        private const string lightYellow = "lightYellow";
        private const string lightPurple = "lightPurple";
        private const string blue = "blue";
        private const string lightOrange = "lightOrange"; // = "";
        private const string darkBlue = "darkBlue";
        private const string white = "white";
        private const string black = "black";
        private const string darkBrown = "darkBrown";
        private static ItemTypeFactory instance;

        /// Percent chance of objects in the current category breaking when thrown.
        private int? _breakage;

        private string _category;
        private string _equipSlot;

        /// The current glyph. Any items defined will use this. Can be a string or
        /// a character code.
        private Glyph _glyph;

        private int _sortIndex;

        private int? _tossDamage;
        private Element _tossElement;
        private int? _tossRange;
        private string _verb;

        private ItemTypeFactory()
        {
            initialize();
        }

        public static ItemTypeFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemTypeFactory();
                }
                return instance;
            }
        }


        public Dictionary<string, ItemType> ItemTypes { get; set; } = new Dictionary<string, ItemType>();

        private void initialize()
        {
            // From Angband:
            // !   A potion (or flask)    /   A pole-arm
            // ?   A scroll (or book)     |   An edged weapon
            // ,   Food                   \   A hafted weapon
            // -   A wand or rod          }   A sling, bow, or x-bow
            // _   A staff                {   A shot, arrow, or bolt
            // =   A ring                 (   Soft armour (cloak, robes, leather armor)
            // "   An amulet              [   Hard armour (metal armor)
            // $   Gold or gems           ]   Misc. armour (gloves, helm, boots)
            // ~   Pelts and body parts   )   A shield
            // &   Chests, Containers

            // Unused: ; : ` % ^ < >

            category(",", null);
            tossable(4, range: 8, element: ElementFactory.Instance.Earth, breakage: 10);
            item("Rock", 1, lightBrown);
            item("RockBlade", 2, lightBrown);       // Knife(s) blade
            item("RockSwordBlade", 3, lightBrown);  // Sword(s) blade
            item("RockBall", 3, lightBrown);        // Hammer Head
            item("RockMoon", 4, lightBrown);        // Axe Head

            treasures();
            pelts();
            potions();
            scrolls();
            weapons();
            bodyArmor();
            boots();
            gloves();
            helmets();
            shields();
        }

        private void shields()
        {

            /*
            Small Leather Shield[s]
            Wooden Targe[s]
            Large Leather Shield[s]
            Steel Buckler[s]
            Kite Shield[s]
            */
            category(")", "equipment/armor/shield", "shield");
            armor("Small Shield", 2, 6, lightBrown, 1);
            armor("Large Shield", 14, 77, brown, 2);
        }

        private void helmets()
        {

            /*
            Leather Cap[s]
            Chainmail Coif[s]
            Steel Cap[s]
            Visored Helm[s]
            Great Helm[s]
            */
            category("^", "equipment/armor/helm", "helm");
            armor("Leather Cap", 2, 6, lightBrown, 1);
            armor("Chain Mail Cap", 14, 77, brown, 2);
        }

        private void gloves()
        {
            /*
            Pair[s] of Leather Gloves
            Set[s] of Bracers
            Pair[s] of Gauntlets
            */
            category("[", "equipment/armor/gloves", "gloves");
            armor("Leather Gloves", 2, 6, lightBrown, 1);
            armor("Chain Mail Gloves", 14, 77, brown, 2);
        }

        private void treasures()
        {
            // TODO: Make monsters and areas drop these.
            // Coins.
            category("¢", "treasure/coin");
            treasure("Copper Coins", 1, brown, 1);
            treasure("Bronze Coins", 7, darkGold, 8);
            treasure("Silver Coins", 11, gray, 20);
            treasure("Electrum Coins", 20, lightGold, 50);
            treasure("Gold Coins", 30, gold, 100);
            treasure("Platinum Coins", 40, lightGray, 300);

            // Bars.
            category("$", "treasure/bar");
            treasure("Copper Bar", 35, brown, 150);
            treasure("Bronze Bar", 50, darkGold, 500);
            treasure("Silver Bar", 60, gray, 800);
            treasure("Electrum Bar", 70, lightGold, 1200);
            treasure("Gold Bar", 80, gold, 2000);
            treasure("Platinum Bar", 90, lightGray, 3000);

            // TODO: Could add more treasure using other currency symbols.

            // TODO: Instead of treasure, make these recipe components.
            /*
            // Gems
            category(r"$", "treasure/gem");
            tossable(damage: 2, range: 7, breakage: 5);
            treasure("Amethyst",      3,  lightPurple,   100);
            treasure("Sapphire",      12, blue,          200);
            treasure("Emerald",       20, green,         300);
            treasure("Ruby",          35, red,           500);
            treasure("Diamond",       60, white,        1000);
            treasure("Blue Diamond",  80, lightBlue,    2000);

            // Rocks
            category(r"$", "treasure/rock");
            tossable(damage: 2, range: 7, breakage: 5);
            treasure("Turquoise Stone", 15, aqua,         60);
            treasure("Onyx Stone",      20, darkGray,    160);
            treasure("Malachite Stone", 25, lightGreen,  400);
            treasure("Jade Stone",      30, darkGreen,   400);
            treasure("Pearl",           35, lightYellow, 600);
            treasure("Opal",            40, lightPurple, 800);
            treasure("Fire Opal",       50, lightOrange, 900);
            */
        }

        private void pelts()
        {
            category("%", null);
            item("Flower", 1, lightAqua); // TODO: Use in recipe.
            item("Fur Pelt", 1, lightBrown);
            item("Insect Wing", 1, purple);
            item("Fox Pelt", 2, orange);
            item("Red Feather", 2, red); // TODO: Use in recipe.
            item("Black Feather", 2, darkGray);
            item("Stinger", 2, gold);
        }

        private void potions()
        {
            // TODO: Make these foods?

            // TODO: Potions should shatter when thrown. Some should perform an effect.

            // Healing.
            category("!", "magic/potion/healing");
            tossable(1, range: 8, breakage: 100);
            healing("Soothing Balm", 1, 10, lightRed, 24);
            healing("Mending Salve", 7, 40, red, 48);
            healing("Healing Poultice", 12, 80, darkRed, 64, true);
            healing("of Amelioration", 24, 200, darkPurple, 120, true);
            healing("of Rejuvenation", 65, 500, purple, 1000, true);

            healing("Antidote", 15, 18, green, 0, true);

            category("!", "magic/potion/resistance");
            tossable(1, range: 8, breakage: 100);
            resistSalve("Heat", 5, 20, orange, ElementFactory.Instance.Fire);
            resistSalve("Cold", 6, 24, lightBlue, ElementFactory.Instance.Cold);
            resistSalve("Light", 7, 28, lightYellow, ElementFactory.Instance.Light);
            resistSalve("Wind", 8, 32, lightAqua, ElementFactory.Instance.Air);
            resistSalve("Electricity", 9, 36, lightPurple, ElementFactory.Instance.Lightning);
            resistSalve("Darkness", 10, 40, darkGray, ElementFactory.Instance.Dark);
            resistSalve("Earth", 13, 44, brown, ElementFactory.Instance.Earth);
            resistSalve("Water", 16, 48, blue, ElementFactory.Instance.Water);
            resistSalve("Acid", 19, 52, lightOrange, ElementFactory.Instance.Acid);
            resistSalve("Poison", 23, 56, green, ElementFactory.Instance.Poison);
            resistSalve("Death", 30, 60, purple, ElementFactory.Instance.Spirit);

            // TODO: "Insulation", "the Elements" and other multi-element resistances.

            // Speed.
            category("!", "magic/potion/speed");
            tossable(1, range: 8, breakage: 100);
            potion("of Quickness", 3, 20, lightGreen, () => new HasteAction(20, 1));
            potion("of Alacrity", 18, 40, green, () => new HasteAction(30, 2));
            potion("of Speed", 34, 200, darkGreen, () => new HasteAction(40, 3));

            // dram, draught, elixir, philter

            // TODO: Make monsters drop these.
            category("?", "magic/potion/bottled");
            tossable(1, range: 8, breakage: 100);
            bottled("Wind", 4, 30, white, ElementFactory.Instance.Air, 8, "blasts");
            bottled("Ice", 7, 55, lightBlue, ElementFactory.Instance.Cold, 15, "freezes");
            bottled("Fire", 11, 70, red, ElementFactory.Instance.Fire, 22, "burns");
            bottled("Water", 12, 110, blue, ElementFactory.Instance.Water, 26, "drowns");
            bottled("Earth", 13, 150, brown, ElementFactory.Instance.Earth, 28, "crushes");
            bottled("Lightning", 16, 200, lightPurple, ElementFactory.Instance.Lightning, 34, "shocks");
            bottled("Acid", 18, 250, lightGreen, ElementFactory.Instance.Acid, 38, "corrodes");
            bottled("Poison", 22, 330, darkGreen, ElementFactory.Instance.Poison, 42, "infects");
            bottled("Shadows", 28, 440, black, ElementFactory.Instance.Dark, 48, "torments", "the darkness");
            bottled("Radiance", 34, 600, white, ElementFactory.Instance.Light, 52, "sears");
            bottled("Spirits", 40, 1000, darkGray, ElementFactory.Instance.Spirit, 58, "haunts");
        }

        private void scrolls()
        {
            // Teleportation.
            category("?", "magic/scroll/teleportation");
            tossable(1, range: 4, breakage: 75);
            scroll("of Sidestepping", 2, 9, lightPurple, () => new TeleportAction(6));
            scroll("of Phasing", 6, 17, purple, () => new TeleportAction(12));
            scroll("of Teleportation", 15, 33, darkPurple, () => new TeleportAction(24));
            scroll("of Disappearing", 26, 47, darkBlue, () => new TeleportAction(48));

            // Detection.
            category("?", "magic/scroll/detection");
            tossable(1, range: 4, breakage: 75);
            scroll("of Item Detection", 7, 27, lightOrange, () => new DetectItemsAction());
        }

        private void weapons()
        {
            // Bludgeons.
            category(@"\", "equipment/weapon/club", verb: "hit[s]");

            tossable(breakage: 25, range: 7);
            weapon("Stick", 1, 0, brown, 4, 3);
            weapon("Cudgel", 3, 9, gray, 5, 4);
            weapon("Club", 6, 21, lightBrown, 6, 5);

            // Staves.
            category("_", "equipment/weapon/staff", verb: "hit[s]");
            tossable(breakage: 35, range: 6);
            weapon("Walking Stick", 2, 9, darkBrown, 5, 3);
            weapon("Staff", 5, 38, lightBrown, 7, 5);
            weapon("Quarterstaff", 11, 250, brown, 12, 8);

            // Hammers.
            category("=", "equipment/weapon/hammer", verb: "bash[es]");
            tossable(breakage: 15, range: 5);
            weapon("Hammer", 27, 621, brown, 16, 12);
            weapon("Mattock", 39, 1225, darkBrown, 20, 16);
            weapon("War Hammer", 45, 2106, lightGray, 24, 20);

            category("=", "equipment/weapon/mace", verb: "bash[es]");
            tossable(breakage: 15, range: 5);
            weapon("Morningstar", 24, 324, gray, 13, 11);
            weapon("Mace", 33, 891, darkGray, 18, 16);

            category("~", "equipment/weapon/whip", verb: "whip[s]");
            tossable(breakage: 25, range: 5);
            weapon("Whip", 4, 9, lightBrown, 5, 1);
            weapon("Chain Whip", 15, 95, darkGray, 9, 2);
            weapon("Flail", 27, 409, darkGray, 14, 4);

            category("|", "equipment/weapon/sword", verb: "slash[es]");
            tossable(breakage: 20, range: 6);
            weapon("Rapier", 7, 188, gray, 11, 4);
            weapon("Shortsword", 11, 324, darkGray, 13, 6);
            weapon("Scimitar", 18, 748, lightGray, 17, 9);
            weapon("Cutlass", 24, 1417, lightGold, 21, 11);
            weapon("Falchion", 38, 2374, white, 25, 15);

            /*

            // Two-handed swords.
            Bastard Sword[s]
            Longsword[s]
            Broadsword[s]
            Claymore[s]
            Flamberge[s]

            */

            // Knives.
            category("|", "equipment/weapon/dagger", verb: "stab[s]");
            tossable(breakage: 2, range: 10);
            weapon("Knife", 3, 9, gray, 5, 5);
            weapon("Dirk", 4, 21, lightGray, 6, 6);
            weapon("Dagger", 6, 63, white, 8, 8);
            weapon("Stiletto", 10, 188, darkGray, 11, 11);
            weapon("Rondel", 20, 409, lightAqua, 14, 14);
            weapon("Baselard", 30, 621, lightBlue, 16, 16);
            // Main-guache
            // Unique dagger: "Mercygiver" (see Misericorde at Wikipedia)

            // Spears.
            category(@"\", "equipment/weapon/spear", verb: "stab[s]");
            tossable(breakage: 3, range: 11);
            weapon("Pointed Stick", 2, 0, brown, 5, 9);
            weapon("Spear", 7, 137, gray, 10, 15);
            weapon("Angon", 14, 621, lightGray, 16, 20);
            weapon("Lance", 28, 2106, white, 24, 28);
            weapon("Partisan", 35, 6833, darkGray, 36, 40);

            // glaive, voulge, halberd, pole-axe, lucerne hammer,
            category(@"\", "equipment/weapon/axe", verb: "chop[s]");
            tossable(breakage: 4);
            weapon("Hatchet", 6, 137, darkGray, 10, 12, 10);
            weapon("Axe", 12, 621, lightBrown, 16, 18, 9);
            weapon("Valaska", 24, 2664, gray, 26, 26, 8);
            weapon("Battleaxe", 40, 4866, lightBlue, 32, 32, 7);

            // Sling. In a category itself because many box affixes don't apply to it.
            category("}", "equipment/weapon/sling", "bow", verb: "hit[s]");
            tossable(breakage: 15, range: 5);
            ranged("Sling", 3, 20, darkBrown, "the stone", 2, 10, 1);

            // Bows.
            category("}", "equipment/weapon/bow", "bow", verb: "hit[s]");
            tossable(breakage: 50, range: 5);
            ranged("Short Bow", 5, 180, brown, "the arrow", 4, 12, 2);
            ranged("Longbow", 13, 600, lightBrown, "the arrow", 8, 14, 3);
            ranged("Crossbow", 28, 2000, gray, "the bolt", 12, 16, 4);
        }

        private void bodyArmor()
        {
            // TODO: Make some armor throwable.

            category("(", "equipment/armor/cloak", "cloak");
            armor("Cloak", 3, 19, darkBlue, 2);
            armor("Fur Cloak", 9, 42, lightBrown, 3);

            category("(", "equipment/armor/body", "body");
            armor("Cloth Shirt", 2, 19, lightGray, 2);
            armor("Leather Shirt", 5, 126, lightBrown, 5);
            armor("Jerkin", 7, 191, orange, 6);
            armor("Leather Armor", 10, 377, brown, 8);
            armor("Padded Armor", 14, 819, darkBrown, 11);
            armor("Studded Leather Armor", 17, 1782, gray, 15);
            armor("Mail Hauberk", 20, 2835, darkGray, 18);
            armor("Scale Mail", 23, 4212, lightGray, 21);

            category("(", "equipment/armor/body/robe", "body");
            armor("Robe", 2, 77, aqua, 4);
            armor("Fur-lined Robe", 9, 191, darkAqua, 6);

            /*
            Metal Lamellar Armor[s]
            Chain Mail Armor[s]
            Metal Scale Mail[s]
            Plated Mail[s]
            Brigandine[s]
            Steel Breastplate[s]
            Partial Plate Armor[s]
            Full Plate Armor[s]
            */
        }

        private void boots()
        {
            category("]", "equipment/armor/boots", "boots");
            armor("Leather Sandals", 2, 6, lightBrown, 1);
            armor("Leather Shoes", 8, 19, brown, 2);
            armor("Leather Boots", 14, 77, darkBrown, 4);
            armor("Metal Shod Boots", 22, 274, gray, 7);
            armor("Greaves", 47, 1017, lightGray, 12);
        }

        private void category(Glyph glyph, string category, string equip = null, string verb = null)
        {
            _glyph = glyph;
            if (category == null)
            {
                _category = null;
            }
            else
            {
                _category = $"item/{category}";
            }

            _equipSlot = equip;
            _verb = verb;

            // Default to not throwable.
            _tossDamage = null;
            _tossRange = null;
            _breakage = null;
        }

        /// Makes items in the current category throwable.
        /// 
        /// This must be called *after* [category] is called.
        private void tossable(int? damage = null, Element element = null, int? range = null, int? breakage = null)
        {
            if (element == null) element = ElementFactory.Instance.None;

            _tossDamage = damage;
            _tossElement = element;
            _tossRange = range;
            _breakage = breakage;
        }

        private void treasure(string name, int level, string foreColour, int price)
        {
            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item(name, level, localGlyph, treasure: true, price: price);
        }

        private void potion(string name, int level, int price, string foreColour, ItemUse use)
        {
            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            if (name.StartsWith("of")) name = $"Potion {name}";

            item(name, level, localGlyph, price: price, use: use);
        }

        private void healing(string name, int level, int price, string foreColour, int amount, bool curePoison = false)
        {
            potion(name, level, price, foreColour, () => new HealAction(amount, curePoison));
        }

        private void resistSalve(string name, int level, int price, string foreColour, Element element)
        {
            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item($"Salve of {name} Resistance", level, localGlyph,
                price: price, use: () => new ResistAction(40, element));
        }

        private void bottled(string name, int level, int price, string foreColour, Element element, int damage,
            string verb, string noun = "")
        {
            if (noun == null) noun = "the ${name.toLowerCase()}";

            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item($"Bottled {name}", level, localGlyph, price: price,
                use: () => new RingSelfAction(
                    new RangedAttack(new Noun(noun), verb, damage, element, 6)));
        }

        private void scroll(string name, int level, int price, string foreColour, ItemUse use)
        {
            if (name.StartsWith("of")) name = $"Scroll {name}";

            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item(name, level, localGlyph, price: price, use: use);
        }

        private void weapon(string name, int level, int price, string foreColour, int damage,
            int tossDamage, int? tossRange = null)
        {
            var toss = new RangedAttack(new Noun($"the {name.ToLower()}"),
                Log.MakeVerbsAgree(_verb, Pronouns.It), tossDamage, ElementFactory.Instance.None,
                tossRange.HasValue ? tossRange.Value : _tossRange.HasValue ? _tossRange.Value : 0);

            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item(name, level, localGlyph, "weapon",
                attack: new Attack(_verb, damage, ElementFactory.Instance.None),
                tossAttack: toss,
                price: price);
        }

        private void ranged(string name, int level, int price, string foreColour, string noun, int damage, int range,
            int tossDamage)
        {
            var toss = new RangedAttack(new Noun($"the {name.ToLower()}"),
                Log.MakeVerbsAgree(_verb, Pronouns.It), tossDamage, ElementFactory.Instance.None,
                _tossRange.HasValue ? _tossRange.Value : 0);
            
            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item(name, level, localGlyph, "bow",
                attack: new RangedAttack(noun, "pierce[s]", damage, ElementFactory.Instance.None, range),
                tossAttack: toss,
                price: price);
        }

        private void armor(string name, int level, int price, string foreColour, int armor)
        {
            var localGlyph = _glyph.Clone() as Glyph;
            localGlyph.Fore = foreColour;

            item(name, level, localGlyph, armor: armor, price: price);
        }

        private void item(string name, int level, object appearance,
            string equipSlot = null, ItemUse use = null,
            Attack attack = null, RangedAttack tossAttack = null,
            int armor = 0, int price = 0,
            bool treasure = false)
        {
            // If the appearance isn"t an actual glyph, it should be a color function
            // that will be applied to the current glyph.
            Glyph localGlyph = null;
            if (!(appearance is Glyph) && appearance is string)
            {
                var gylphForColor = appearance as string;
                localGlyph = _glyph;
                localGlyph.Fore = gylphForColor;
            }
            else
            {
                localGlyph = appearance as Glyph;
            }

            var categories = new List<string>();
            if (_category != null) categories = _category.Split('/').ToList();

            if (equipSlot == null) equipSlot = _equipSlot;

            if (tossAttack == null && _tossDamage != null)
            {
                tossAttack = new RangedAttack(new Noun($"the {name.ToLower()}"), "hits",
                    (double) _tossDamage, _tossElement, _tossRange.HasValue ? _tossRange.Value : 0);
            }

            var newItemType = new ItemType(name, localGlyph, level, _sortIndex++,
                categories, equipSlot, use, attack, tossAttack, _breakage, armor, price,
                treasure);

            ItemTypes.Add(name, newItemType);
        }

        public List<ItemType> ToItemType(List<string> ingredientNames)
        {
            var itemTypeList = new List<ItemType>();
            foreach (var ingredientName in ingredientNames)
            {
                itemTypeList.Add(ItemTypes[ingredientName]);
            }
            return itemTypeList;
        }
    }
}