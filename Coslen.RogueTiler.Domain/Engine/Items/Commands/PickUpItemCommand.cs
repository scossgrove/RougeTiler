using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items.Commands
{
    public class PickUpItemCommand : ItemCommand
    {
        public override List<ItemLocation> AllowedLocations() => new List<ItemLocation> {ItemLocations.OnGround};

        public override string Query(ItemLocation location)
        {
            if (location == ItemLocations.OnGround)
            {
                return "Pick up which item?";
            }
            throw new ApplicationException("unreachable");
        }

        public override bool CanSelect(Item item) => true;

        public override void SelectItem(Game game, Item item, ItemLocation location, int index, VectorBase target)
        {
            // Pick up item and return to the game
            game.Hero.SetNextAction(new PickUpAction(index));
        }
    }
}