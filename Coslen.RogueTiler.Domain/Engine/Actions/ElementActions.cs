namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// These actions are side effects from taking elemental damage.
    public class BurnAction : Action
    {
        private double damage;

        public BurnAction(double damage)
        {
            this.damage = damage;
        }

        public override ActionResult OnPerform()
        {
            // TODO: Burn flammable items.

            // Being burned "cures" cold.
            if (Actor.Cold.IsActive)
            {
                Actor.Cold.Cancel();
                return Succeed("The fire warms {1} back up.", Actor);
            }

            return ActionResult.Success;
        }
    }
}