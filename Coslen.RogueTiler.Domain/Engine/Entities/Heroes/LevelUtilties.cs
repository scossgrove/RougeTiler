namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    public static class LevelUtilties
    {
        public static int CalculateLevel(int experienceCents)
        {
            var experience = experienceCents/100;

            for (var level = 1; level <= Option.HeroLevelMax; level++)
            {
                if (experience < CalculateLevelCost(level))
                {
                    return level - 1;
                }
            }

            return Option.HeroLevelMax;
        }

        /// Returns how much experience is needed to reach [level] or `null` if [level]
        /// is greater than the maximum level.
        public static int? CalculateLevelCost(int level)
        {
            if (level > Option.HeroLevelMax)
            {
                return null;
            }
            return (level - 1)*(level - 1)*Option.HeroLevelCost;
        }
    }
}