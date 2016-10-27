using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Quests
{
    /// A quest to stand on a tile of a certain [TileType].
    public class TileQuest : Quest
    {
        public string Description { get; set; }
        public TileType TileType { get; set; }

        public TileQuest(string description, TileType tileType)
        {
            Description = description;
            TileType = tileType;
        }

        public override void Announce(Log log)
        {
            log.Quest($"You must find {Description}.");
        }

        public override bool OnEnterTile(Game game, Tile tile) => tile.Type == TileType;
    }
}