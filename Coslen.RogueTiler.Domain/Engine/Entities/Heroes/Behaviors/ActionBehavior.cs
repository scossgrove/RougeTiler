using Coslen.RogueTiler.Domain.Engine.Actions;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Behaviors
{
    /// A simple one-shot behavior that performs a given [Action] and then reverts
    /// back to waiting for input.
    public class ActionBehavior : Behavior
    {
        public ActionBehavior(Action action)
        {
            Action = action;
        }

        public Action Action { get; set; }

        public override bool CanPerform(Hero hero)
        {
            return true;
        }

        public override Action GetAction(Hero hero)
        {
            hero.WaitForInput();
            return Action;
        }
    }
}