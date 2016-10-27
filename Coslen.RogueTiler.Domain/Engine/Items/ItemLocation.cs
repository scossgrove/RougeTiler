namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// An [Item] in the game can be either on the ground in the level, or held by
    /// the [Hero] in their [Inventory] or [Equipment]. This enum describes which
    /// of those is the case.
    public class ItemLocation
    {
        public string name;

        public ItemLocation(string name)
        {
            this.name = name;
        }
    }
}