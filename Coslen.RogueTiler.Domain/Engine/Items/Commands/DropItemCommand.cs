using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items.Commands
{
    public class DropItemCommand : ItemCommand
    {
        public override List<ItemLocation> AllowedLocations() => new List<ItemLocation> {ItemLocations.Inventory, ItemLocations.Equipment};

        public override string Query(ItemLocation location)
        {
            if (location == ItemLocations.Inventory)
            {
                return "Drop which item?";
            }
            if (location == ItemLocations.Equipment)
            {
                return "Unequip and drop which item?";
            }
            throw new ApplicationException("unreachable");
        }

        public override bool CanSelect(Item item) => true;

        public override void SelectItem(Game game, Item item, ItemLocation location, int index, VectorBase target)
        {
            game.Hero.SetNextAction(new DropAction(location, index));
        }
    }
}