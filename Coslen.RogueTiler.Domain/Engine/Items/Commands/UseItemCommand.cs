using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items.Commands
{
    public class UseItemCommand : ItemCommand
    {
        public override List<ItemLocation> AllowedLocations() => new List<ItemLocation> {ItemLocations.Inventory, ItemLocations.Equipment, ItemLocations.OnGround};

        public override string Query(ItemLocation location)
        {
            if (location == ItemLocations.Inventory)
            {
                return "Use or equip which item?";
            }
            if (location == ItemLocations.Equipment)
            {
                return "Unequip which item?";
            }
            if (location == ItemLocations.OnGround)
            {
                return "Pick up and use which item?";
            }
            throw new ApplicationException("unreachable");
        }

        public override bool CanSelect(Item item) => item == null ? false : item.canUse || item.canEquip;

        public override void SelectItem(Game game, Item item, ItemLocation location, int index, VectorBase target)
        {
            game.Hero.SetNextAction(new UseAction(location, index));
        }
    }
}