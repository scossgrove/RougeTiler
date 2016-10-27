using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    /// A slashing melee attack that hits a number of adjacent monsters.
    public class SlashCommand : DirectionCommand
    {
        public override string Name
        {
            get { return "Slash"; }
        }

        public override bool CanUse(Game game)
        {
            // Must have a sword equipped.
            var weapon = game.Hero.Equipment.Weapon;
            if (weapon == null)
            {
                return false;
            }

            return weapon.type.categories.Contains("sword");
        }

        public override Action GetDirectionAction(Game game, Direction dir)
        {
            return new SlashAction(dir);
        }
    }
}