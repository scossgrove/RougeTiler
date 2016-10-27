using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Action for doing nothing for a turn.
    public class RestAction : Action
    {
        public int noise
        {
            get { return Option.NoiseRest; }
        }

        public override ActionResult OnPerform()
        {
            if (Actor is Hero)
            {
                _eatFood();
            }
            else if (!Actor.IsVisible)
            {
                // Breeds can rest if out of sight.
                Actor.Health.Current++;
            }

            return Succeed();
        }

        /// Regenerates health when the hero rests, if possible.
        private void _eatFood()
        {
            if (hero.Food <= 0)
            {
                return;
            }
            if (hero.Poison.IsActive)
            {
                return;
            }
            if (hero.Health.IsMax)
            {
                return;
            }

            hero.Food--;
            hero.Health.Current++;
        }
    }
}