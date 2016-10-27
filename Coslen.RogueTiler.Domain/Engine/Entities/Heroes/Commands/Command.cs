namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    /// A command is a specific ability that the player can select for the hero to
    /// perform.
    /// 
    /// Some commands require additional data to be performed -- a target position
    /// or a direction. Those will implement one of the subclasses, [TargetCommand]
    /// or [DirectionCommand].
    public abstract class Command
    {
        /// The name of the command.
        public abstract string Name { get; }

        /// Override this to validate that the [Command] can be used right now. For
        /// example, this is only `true` for the archery command when the hero has a
        /// ranged weapon equipped.
        public virtual bool CanUse(Game game)
        {
            return true;
        }

        // target or direction.

        // TODO: Add getAction() here when there are commands that don't require a
    }
}