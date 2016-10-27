using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    /// A melee attack that repeatedly stabs an adjacent monster.
    public class StabCommand : DirectionCommand
    {
        public override string Name
        {
            get { return "Stab"; }
        }

        public override bool CanUse(Game game)
        {
            // Must have a dagger equipped.
            var weapon = game.Hero.Equipment.Weapon;
            if (weapon == null)
            {
                return false;
            }

            return weapon.type.categories.Contains("dagger");
        }

        public override Action GetDirectionAction(Game game, Direction dir)
        {
            return new StabAction(dir);
        }
    }
}