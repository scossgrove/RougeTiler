using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    public class ItemDataJson
    {
        public string ItemType { get; set; }
        public ItemAffixJson Prefix { get; set; }
        public ItemAffixJson Suffix { get; set; }
        public VectorBase Position { get; set; }
    }
}