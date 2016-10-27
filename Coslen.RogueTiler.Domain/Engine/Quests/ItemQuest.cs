using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Quests
{
    /// A quest to find an [Item] of a certain [ItemType].
    public class ItemQuest : Quest
    {
        public ItemType ItemType { get; set; }

        public ItemQuest(ItemType itemType)
        {
            ItemType = itemType;
        }

        public override void Announce(Log log)
        {
            // TODO: Handle a/an.
            log.Quest($"You must find a {ItemType.name}.");
        }

        public override bool OnPickUpItem(Game game, Item item) => item.type == ItemType;
    }
}