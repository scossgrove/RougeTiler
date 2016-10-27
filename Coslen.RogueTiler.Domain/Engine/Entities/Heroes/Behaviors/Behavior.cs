using Coslen.RogueTiler.Domain.Engine.Actions;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Behaviors
{
    /// What the [Hero] is "doing". If the hero has no behavior, he is waiting for
    /// user input. Otherwise, the behavior will determine which [Action]s he
    /// performs.
    public abstract class Behavior
    {
        public abstract bool CanPerform(Hero hero);

        public abstract Action GetAction(Hero hero);
    }
}