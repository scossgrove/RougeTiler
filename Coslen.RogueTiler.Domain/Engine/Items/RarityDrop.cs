using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// Chooses a single [Drop] from a list of possible options with a rarity for
    /// each.
    public class RarityDrop : Drop
    {
        private readonly int _total;
        public List<Rarity> _drops;

        public RarityDrop(List<Rarity> drops)
        {
            _drops = drops;
            // Convert rarity to frequency by using each drop's rarity to increase the
            // frequency of all of the others.
            foreach (var drop in _drops)
            {
                foreach (var other in _drops)
                {
                    if (other == drop)
                    {
                        continue;
                    }
                    other._frequency *= drop._rarity;
                }
            }

            _total = _drops.Select(x => x._frequency).Sum();
        }

        public override void SpawnDrop(AddItem addItem)
        {
            var roll = Rng.Instance.Range(_total);

            for (var i = 0; i < _drops.Count; i++)
            {
                roll -= _drops[i]._frequency;
                if (roll < 0)
                {
                    _drops[i]._drop.SpawnDrop(addItem);
                    return;
                }
            }
        }
    }
}