using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes
{
    /// Base class for a Hero's character class.
    /// 
    /// Each class has its own unique behavior and game mechanics. To support this,
    /// there are a number of abstract methods here that will be called at
    /// appropriate times during the game. Specific classes can then decide how to
    /// handle that.
    public abstract class HeroClass
    {
        /// Gets the [Hero] that has this class.
        public Hero Hero { get; private set; }

        public string Name { get; protected set; }

        /// The [Command]s that the class enables.
        public List<Command> Commands { get; protected set; }

        /// Gets the armor bonus conferred by this class.
        public int Armor { get; private set; }

        /// Attaches this class to a [hero].
        public void Bind(Hero hero)
        {
            if (Hero != null)
            {
                throw new ApplicationException("A hero is already bound to the Hero Class");
            }

            Hero = hero;
        }

        /// Gives the class a chance to modify the attack the hero is about to perform
        /// on [defender].
        public abstract Attack ModifyAttack(Attack attack, Actor defender);

        /// Called when the [Hero] has taken [damage] from [attacker].
        public abstract void TookDamage(Action action, Actor attacker, int damage);

        /// Called when the [Hero] has killed [monster].
        public abstract void KilledMonster(Action action, Monster monster);

        /// Called when the [Hero] has finished taking a turn.
        public abstract void FinishedTurn(Action action);

        /// Clones this object.
        /// 
        /// Called when the hero enters the level so that if they die, all changes
        /// can be discarded.
        public abstract HeroClass Clone();
    }
}