using Coslen.RogueTiler.Domain.Engine.Actions;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Behaviors
{
    /// Automatic resting. With this [Behavior], the [Hero] will rest each turn
    /// until any of the following occurs:
    /// 
    /// * He is fully rested.
    /// * He gets hungry.
    /// * He is "disturbed" and something gets hit attention, like a [Monster]
    /// moving, being hit, etc.
    public class RestBehavior : Behavior
    {
        public override bool CanPerform(Hero hero)
        {
            // See if done resting.
            if (hero.Health.IsMax)
            {
                return false;
            }

            if (hero.Food <= 0)
            {
                hero.Game.Log.Message("You must explore more before you can rest.");
                return false;
            }

            return true;
        }

        public override Action GetAction(Hero hero)
        {
            return new RestAction();
        }
    }
}