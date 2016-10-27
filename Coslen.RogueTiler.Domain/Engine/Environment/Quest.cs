using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Logging;

namespace Coslen.RogueTiler.Domain.Engine.Environment
{
    public abstract class Quest
    {
        public bool IsComplete { get; set; }

        /// Logs the goal of this quest so the player knows what to do.
        public virtual void Announce(Log log)
        {
        }

        public bool PickUpItem(Game game, Item item)
        {
            if (OnPickUpItem(game, item))
            {
                Complete(game);
            }
            return IsComplete;
        }

        public virtual bool OnPickUpItem(Game game, Item item)
        {
            return false;
        }

        public bool KillMonster(Game game, Monster monster)
        {
            if (OnKillMonster(game, monster))
            {
                Complete(game);
            }
            return IsComplete;
        }

        public virtual bool OnKillMonster(Game game, Monster monster)
        {
            return false;
        }

        public bool EnterTile(Game game, Tile tile)
        {
            if (OnEnterTile(game, tile))
            {
                Complete(game);
            }
            return IsComplete;
        }

        public virtual bool OnEnterTile(Game game, Tile tile)
        {
            return false;
        }

        public void Complete(Game game)
        {
            // Only complete once.
            if (IsComplete)
            {
                return;
            }

            IsComplete = true;
            game.Log.Quest("You have completed your quest! Press 'q' to exit the level.");
        }
    }
}