using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Coslen.RogueTiler.Domain.UIConnector;

namespace Coslen.RogueTiler.Domain.Engine.Items.Commands
{
    public class TossItemCommand : ItemCommand
    {
        public override List<ItemLocation> AllowedLocations() => new List<ItemLocation> {ItemLocations.Inventory, ItemLocations.Equipment, ItemLocations.OnGround};

        public override string Query(ItemLocation location)
        {
            if (location == ItemLocations.Inventory)
            {
                return "Throw which item?";
            }
            if (location == ItemLocations.Equipment)
            {
                return "Unequip and throw which item?";
            }
            if (location == ItemLocations.OnGround)
            {
                return "Pick up and throw which item?";
            }
            throw new ApplicationException("unreachable");
        }

        public override bool CanSelect(Item item) => item?.canToss ?? false;
        
        public override void SelectItem(Game game, Item item, ItemLocation location, int index, VectorBase target)
        {
            game.Hero.SetNextAction(new TossAction(location, index, target));
        }
    }
}