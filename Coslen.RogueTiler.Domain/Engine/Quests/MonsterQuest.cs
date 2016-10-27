using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Environment;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Quests
{
    /// A quest to kill a number of [Monster]s of a certain [Breed].
    public class MonsterQuest : Quest
    {
        public Breed Breed { get; set; }
        public int Remaining { get; set; }

        public override void Announce(Log log) {
            log.Quest("You must kill {1}.", new Quantity(Remaining, Breed));
        }

        public MonsterQuest(Breed breed, int remaining)
        {
            Breed = breed;
            Remaining = remaining;
        }

        // TODO: Need to handle quest monster being killed by friendly fire.
        public override bool OnKillMonster(Game game, Monster monster)
        {
            if (monster.Breed == Breed)
            {
                Remaining--;

                if (Remaining > 0)
                {
                    game.Log.Quest("{1} await[s] death at your hands.", new Quantity(Remaining, Breed));
                }
            }

            return Remaining <= 0;
        }
    }
}