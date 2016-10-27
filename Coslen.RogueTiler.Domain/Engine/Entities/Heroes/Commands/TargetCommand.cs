using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    /// A command that requires a target position to perform.
    public abstract class TargetCommand : Command
    {
        /// The maximum range of the target from the hero.
        public double GetRange(Game game)
        {
            return 0;
        }

        /// Override this to create the [Action] that the [Hero] should perform when
        /// using this [Command].
        public virtual Action GetTargetAction(Game game, VectorBase target)
        {
            return null;
        }
    }
}