namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Heals the [Actor] performing the action.
    public class HealAction : Action
    {
        private readonly int amount;
        private readonly bool curePoison;

        public HealAction(int amount, bool curePoison = false)
        {
            this.amount = amount;
            this.curePoison = curePoison;
        }

        public override ActionResult OnPerform()
        {
            var changed = false;

            if (Actor.Poison.IsActive && curePoison)
            {
                Actor.Poison.Cancel();
                Log("{1} [are|is] cleansed of poison.", Actor);
                changed = true;
            }

            if (!Actor.Health.IsMax && amount > 0)
            {
                Actor.Health.Current += amount;

                gameResult.AddEvent(EventType.Heal, Actor, ElementFactory.Instance.None, null, null, amount.ToString());
                Log("{1} feel[s] better.", Actor);
                changed = true;
            }

            if (changed)
            {
                return ActionResult.Success;
            }
            return Succeed("{1} [don't|doesn't] feel any different.", Actor);
        }
    }
}