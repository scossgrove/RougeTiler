using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine.Quests
{
    /// Builds a quest for finding an item on the ground in the [Stage].
    public class FloorItemQuestBuilder : QuestBuilder
    {
        public ItemType ItemType { get; set; }

        public FloorItemQuestBuilder(ItemType itemType)
        {
            ItemType = itemType;
        }

        public override Quest Generate(Game game, Stage stage) {
            var item = new Item(ItemType);
            item.Position = stage.FindDistantOpenTile(10);
            stage.Items.Add(item);

            return new ItemQuest(ItemType);
        }
    }
}