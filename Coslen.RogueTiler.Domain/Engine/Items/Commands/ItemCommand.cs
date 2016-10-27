using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items.Commands
{
    /// The action the user wants to perform on the selected item.
    public abstract class ItemCommand
    {
        /// Locations of items that can be used with this command. When a command
        /// allows multiple locations, players can switch between them.
        public abstract List<ItemLocation> AllowedLocations();

        /// The query shown to the user when selecting an item in this mode from
        /// [view].
        public abstract string Query(ItemLocation location);

        /// Returns `true` if [item] is a valid selection for this command.
        public abstract bool CanSelect(Item item);

        /// Called when a valid item has been selected.
        public abstract void SelectItem(Game game, Item item, ItemLocation location, int index, VectorBase target);
    }
}