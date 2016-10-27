using System.Collections.Generic;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// A [Drop] that drops all of a list of child drops.
    public class AllOfDrop : Drop
    {
        public List<Drop> _drops;

        public AllOfDrop(List<Drop> drops)
        {
            _drops = drops;
        }

        public override void SpawnDrop(AddItem addItem)
        {
            foreach (var drop in _drops)
            {
                drop.SpawnDrop(addItem);
            }
        }
    }
}