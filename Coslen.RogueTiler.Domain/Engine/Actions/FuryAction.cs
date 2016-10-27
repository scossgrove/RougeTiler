using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// A [Warrior]'s [Action] that requires and spends fury to perform a powerful
    /// attack.
    public abstract class FuryAction : Action
    {
        private bool _madeContact;

        public override ActionResult OnPerform()
        {
            if (hero.Charge.Current < 1)
            {
                return Fail("You are not furious enough yet.");
            }

            var result = PerformAttack();

            // Drain fury when the attack is done if it hit something.
            if (result.done && _madeContact)
            {
                hero.Charge.Current /= 2;
            }
            return result;
        }

        public virtual ActionResult PerformAttack()
        {
            return null;
        }

        /// Attempts to perform an attack on the [Actor] as [pos], if any.
        public void Attack(VectorBase pos)
        {
            var defender = Game.CurrentStage.ActorAt(pos);
            if (defender == null)
            {
                return;
            }

            var attack = Actor.GetAttack(defender);

            // The more furious the warrior is, the stronger the attack will be (and the
            // more fury that will be spent). The attack multiplier increases more
            // quickly than the fury cost so that the player is rewarded for building
            // up fury and doing a single stronger attack.
            var multiplier = 1.0 + hero.Charge.Current / 20;
            attack = attack.MultiplyDamage(multiplier);

            if (attack.Perform(this, Actor, defender))
            {
                _madeContact = true;
            }
        }
    }
}