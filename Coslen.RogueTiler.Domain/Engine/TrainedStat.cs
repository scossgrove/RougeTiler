using System;

namespace Coslen.RogueTiler.Domain.Engine
{
    /// A learned ability that can increase in level based on some occurrence
    /// happening a certain number of times.
    public class TrainedStat
    {
        /// The number of occurrences required to reach the next level.
        /// 
        /// This will be the cost to reach level 1. After that, the cost per level is
        /// increased by [_increase], yielding a geometric progression. A higher cost
        /// makes it harder to gain levels.
        public int Cost;

        /// The amount the [_cost] increases at each level.
        public int Increase;

        public TrainedStat(int cost, int increase)
        {
            Cost = cost;
            Increase = increase;
        }

        /// The current count of occurrences.
        public int Count { get; private set; }

        /// How far into reaching the next level the stat is, as a percentage.
        public int percentUntilNext
        {
            get
            {
                var left = Count;
                var cost = Cost;

                while (left >= cost)
                {
                    left -= cost;
                    cost += Increase;
                }

                return (int) Math.Floor(100.0f*left/cost);
            }
        }

        /// The current level.
        /// 
        /// Starts at zero and increases.
        public int Level
        {
            get
            {
                var level = 0;
                var left = Count;
                var cost = Cost;

                while (left >= cost)
                {
                    level++;
                    left -= cost;
                    cost += Increase;
                }

                return level;
            }
        }

        /// Add [count] occurrences to the count.
        /// 
        /// Returns `true` if the level increased.
        public bool Increment(int count)
        {
            var oldLevel = Level;
            Count += count;
            return Level != oldLevel;
        }
    }
}