using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    // TODO: When pole-arms exist, have a 2-tile stab for spears and a 3-tile stab
    // for polearms.
    /// A piercing melee attack that penetrates a row of adjacent monsters.
    internal class LanceCommand : DirectionCommand
    {
        public override string Name
        {
            get { return "Stab"; }
        }

        public override bool CanUse(Game game)
        {
            // Must have a spear equipped.
            var weapon = game.Hero.Equipment.Weapon;
            if (weapon == null)
            {
                return false;
            }

            return weapon.type.categories.Contains("spear");
        }

        public override Action GetDirectionAction(Game game, Direction dir)
        {
            return new LanceAction(dir);
        }
    }
}