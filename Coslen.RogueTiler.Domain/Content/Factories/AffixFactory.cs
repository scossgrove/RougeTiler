using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Content.Factories
{
    /// A generic "kind" of affix that can create concrete [Affix] instances.
    public class AffixFactory
    {
        public CreateAttack attack;

        /// The names of the categories that this affix can apply to.
        public List<string> categories;

        /// The level of the affix. Higher level affixes tend to only appear on
        /// higher level items.
        public int level;

        public string name;

        public int rarity;

        public AffixFactory(string name, List<string> categories, int level, int rarity, CreateAttack attack)
        {
            this.name = name;
            this.categories = categories;
            this.level = level;
            this.rarity = rarity;
            this.attack = attack;
        }
    }
}