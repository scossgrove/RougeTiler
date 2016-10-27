using Coslen.RogueTiler.Domain.Engine.Environment;

namespace Coslen.RogueTiler.Domain.Engine.Quests
{
    /// Builds a quest for standing on a [TileType] on the [Stage].
    public class TileQuestBuilder : QuestBuilder
    {
        public string Description { get; set; }
        public TileType TileType { get; set; }

        public TileQuestBuilder(string description, TileType tileType)
        {
            Description = description;
            TileType = tileType;
        }

        public override Quest Generate(Game game, Stage stage)
        {
            var pos = stage.FindDistantOpenTile(5);
            stage[pos].Type = TileType;

            return new TileQuest(Description, TileType);
        }
    }
}