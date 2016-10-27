namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// A rarity for a single case in a [_RarityDrop].
    /// 
    /// This determines how rare a drop is relative to other cases in the drop. A
    /// rarity of five means other drops are five times more common that this one.
    /// 
    /// Frequency and rarity are inverses of each other. If one case becomes more
    /// rare, that's equivalent to the frequencies of all other drops increasing.
    public class Rarity
    {
        public Drop _drop;

        /// The inverse of [_rarity]. Calculated by [_RarityDrop].
        public int _frequency = 1;

        public int _rarity;

        public Rarity(int rarity, Drop drop)
        {
            _rarity = rarity;
            _drop = drop;
        }

        /// Creates a single drop [Rarity].
        public static Rarity BuildRarity(int rarity, string name, int? level = null)
        {
            return new Rarity(rarity, DropFactory.parseDrop(name, level));
        }
    }
}