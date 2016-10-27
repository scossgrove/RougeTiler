using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// [Action] for a melee attack from one [Actor] to another.
    public class AttackAction : Action
    {
        private readonly Actor defender;

        public AttackAction(Actor defender)
        {
            this.defender = defender;
        }

        public int noise
        {
            get { return Option.NoiseHit; }
        }

        public override ActionResult OnPerform()
        {
            // Get all of the melee information from the participants.
            var attack = Actor.GetAttack(defender);
            attack.Perform(this, Actor, defender);
            return ActionResult.Success;
        }

        public override string ToString()
        {
            return Actor + " attacks " + defender;
        }
    }
}