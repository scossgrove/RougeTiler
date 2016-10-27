using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Content.Factories;

namespace Coslen.RogueTiler.Domain.Engine.Entities
{
    public static class MonsterFactory
    {
        public static Breed GenerateBreedOfMonster(int stageNumber)
        {
            return AreaFactory.Instance.GenerateBreedOfMonsterForStage(stageNumber);
        }
    }
}
