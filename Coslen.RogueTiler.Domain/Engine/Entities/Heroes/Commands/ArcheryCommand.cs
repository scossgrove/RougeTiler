using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands
{
    public class ArcheryCommand : TargetCommand
    {
        public override string Name
        {
            get { return "Archery"; }
        }

        public double getRange(Game game)
        {
            return (game.Hero.Equipment.Weapon.attack as RangedAttack).Range;
        }

        public override bool CanUse(Game game)
        {
            // Get the equipped ranged weapon, if any.
            var weapon = game.Hero.Equipment.Weapon;
            return weapon != null && weapon.isRanged;
        }

        public override Action GetTargetAction(Game game, VectorBase target)
        {
            var weapon = game.Hero.Equipment.Weapon;
            return new BoltAction(target, (RangedAttack) weapon.attack, true);
        }
    }
}