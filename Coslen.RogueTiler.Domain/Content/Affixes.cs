using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Content
{
    public delegate Attack CreateAttack();

    public class Affixes
    {
        private static Affixes instance;

        public List<AffixFactory> Prefixes = new List<AffixFactory>();
        public List<AffixFactory> Suffixes = new List<AffixFactory>();

        private Affixes()
        {
        }

        public static Affixes Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Affixes();
                }
                return instance;
            }
        }

        public void Initialise()
        {
            brand("Glimmering", 12, 3, ElementFactory.Instance.Light, 0, 1.0f);
            brand("Shining", 24, 4, ElementFactory.Instance.Light, 2, 1.1f);
            brand("Radiant", 48, 5, ElementFactory.Instance.Light, 4, 1.2f);

            brand("Dim", 16, 3, ElementFactory.Instance.Dark, 0, 1.1);
            brand("Dark", 32, 4, ElementFactory.Instance.Dark, 1, 1.2);
            brand("Black", 56, 5, ElementFactory.Instance.Dark, 3, 1.3);

            brand("Freezing", 20, 3, ElementFactory.Instance.Cold, 2, 1.2);

            brand("Burning", 20, 3, ElementFactory.Instance.Fire, 2, 1.2);
            brand("Flaming", 40, 4, ElementFactory.Instance.Fire, 4, 1.3);
            brand("Searing", 60, 5, ElementFactory.Instance.Fire, 6, 1.4);

            brand("Electric", 50, 5, ElementFactory.Instance.Lightning, 4, 1.6);
            brand("Shocking", 70, 5, ElementFactory.Instance.Lightning, 6, 1.8);

            brand("Poisoned", 35, 5, ElementFactory.Instance.Poison, 5, 1.3);
            brand("Venomous", 70, 5, ElementFactory.Instance.Poison, 6, 1.5);

            brand("Ghostly", 45, 5, ElementFactory.Instance.Spirit, 3, 1.3);

            // TODO: Should these scale damage?
            Damage("of Harming", 8, 1, 1, 4);
            Damage("of Wounding", 15, 1, 3, 4);
            Damage("of Maiming", 35, 1, 6, 3);
            Damage("of Slaying", 65, 1, 10, 3);

            BowDamage("Ash", 10, 1, 3, 4);
            BowDamage("Yew", 20, 1, 5, 3);

            // TODO: "of Accuracy" increases range of bows.
        }

        // Defines a new prefix [Affix].
        private void AffixFactory(List<AffixFactory> factories, string name, int level, int rarity, string groups, CreateAttack createAttack)
        {
            factories.Add(new AffixFactory(name, groups.Split(' ').ToList(), level, rarity, createAttack));
        }

        /// A weapon suffix for adding damage.
        private void Damage(string name, int level, int rarity, int baseDamage, int taper)
        {
            Func<Attack> myCreateAttack = delegate
            {
                var newAttack = new Attack("", baseDamage, ElementFactory.Instance.None, null);
                newAttack.Modifier(null, null, Rng.Instance.taper(baseDamage, taper), null);
                return newAttack;
            };

            AffixFactory(Suffixes, name, level, rarity, "weapon", new CreateAttack(myCreateAttack));
        }

        /// bow prefix for adding damage.
        private void BowDamage(string name, int level, int rarity, int baseDamage, int taper)
        {
            Func<Attack> myCreateAttack = delegate
            {
                var newAttack = new Attack("", baseDamage, ElementFactory.Instance.None, null);
                newAttack.Modifier(null, null, Rng.Instance.taper(baseDamage, taper), null);
                return newAttack;
            };
            AffixFactory(Prefixes, name, level, rarity, "bow", new CreateAttack(myCreateAttack));
        }

        /// A weapon prefix for giving an elemental brand.
        private void brand(string name, int level, int rarity, Element element, int bonus, double scale)
        {
            Func<Attack> myCreateAttack = delegate
            {
                var newAttack = new Attack("", 0, ElementFactory.Instance.None, null);
                newAttack.Modifier(element, null, Rng.Instance.taper(bonus, 5), Rng.Instance.taper((int) (scale + 10), 4)/10);
                return newAttack;
            };

            AffixFactory(Prefixes, name, level, rarity, "weapon", new CreateAttack(myCreateAttack));
        }

        /// Creates a new [Item] of [itemType] and chooses affixes for it.
        public Item CreateItem(ItemType itemType, int levelOffset = 0)
        {
            // Uncategorized items don't have any affixes.
            if (itemType.category == null)
            {
                return new Item(itemType, null, null);
            }

            // Give items a chance to boost their effective level when choosing a
            // affixes.
            var level = Rng.Instance.taper(itemType.level, 2);
            //var level = Rng.Instance.Range(2, itemType.level);

            var prefix = ChooseAffix(Prefixes, itemType, level, levelOffset);
            var suffix = ChooseAffix(Suffixes, itemType, level, levelOffset);

            // Decide if the item may have just a prefix, just a suffix, or (rarely)
            // both. This is mainly to make dual-affix items less common since they
            // look a bit funny.
            switch (Rng.Instance.Range(5))
            {
                case 0:
                case 1:
                    return new Item(itemType, prefix, null);
                case 2:
                case 3:
                    return new Item(itemType, null, suffix);
                default:
                    return new Item(itemType, prefix, suffix);
            }
        }

        public Affix ChooseAffix(List<AffixFactory> factories, ItemType itemType, int level, int chanceOffset)
        {
            // Get the affixes that can apply to the item.
            factories = factories.Where(factory => factory.level <= level).Where(factory => factory.categories.Any(category => itemType.categories.Contains(category))).ToList();

            // TODO: For high level drops, consider randomly discarding some of the
            // lower-level affixes.

            // Try all of the affixes and see if one sticks.
            // TODO: The way this works means adding more affixes makes them more
            // common. Should probably choose one instead of trying them all.

            //factories.shuffle();
            foreach (var factory in factories)
            {
                // There's a chance of not selecting the affix at all.
                if (Rng.Instance.Range(100) < 60 + chanceOffset)
                {
                    continue;
                }

                // Take the rarity into account.
                if (Rng.Instance.Range(100) < 100/factory.rarity)
                {
                    var attack = factory.attack();
                    return new Affix(factory.name, attack);
                }
            }

            return null;
        }
    }
}