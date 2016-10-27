using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    /// A command that requires a direction to perform.
    public abstract class DirectionCommand : Command
    {
        /// Override this to create the [Action] that the [Hero] should perform when
        /// using this [Command].
        public virtual Action GetDirectionAction(Game game, Direction dir)
        {
            return null;
        }
    }
}