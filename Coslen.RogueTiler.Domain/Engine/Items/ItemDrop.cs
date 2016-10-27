using Coslen.RogueTiler.Domain.Content;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// Drops an item of a given type.
    public class ItemDrop : Drop
    {
        public ItemType _type;

        public ItemDrop(ItemType type)
        {
            _type = type;
        }

        public override void SpawnDrop(AddItem addItem)
        {
            addItem(Affixes.Instance.CreateItem(_type));
        }
    }
}