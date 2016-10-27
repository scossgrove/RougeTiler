using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Environment;

namespace Coslen.RogueTiler.Domain.Engine.Quests
{
    /// Builds a quest for killing a certain number of a certain [Monster].
    public class MonsterQuestBuilder : QuestBuilder
    {
        public Breed Breed { get; set; }
        public int Count { get; set; }

        public MonsterQuestBuilder(Breed breed, int count)
        {
            this.Breed = breed;
            Count = count;
        }

        public override Quest Generate(Game game, Stage stage)
        { 
            // Make at least one "boss" one far away.
            if (Count == 1)
            {
                var pos = stage.FindDistantOpenTile(10);
                stage.SpawnMonster(game, Breed, pos);
            }

            // Scatter any others a little more freely.
            for (var i = 1; i < Count; i++)
            {
                var pos = stage.FindDistantOpenTile(3);
                stage.SpawnMonster(game, Breed, pos);
            }

            return new MonsterQuest(Breed, Count);
        }
    }
}