using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// A [Drop] that will create an inner drop some random percentage of the time.
    public class PercentDrop : Drop
    {
        public int _chance;
        public Drop _drop;

        public PercentDrop(int chance, Drop drop)
        {
            _chance = chance;
            _drop = drop;
        }

        public override void SpawnDrop(AddItem addItem)
        {
            if (Rng.Instance.Range(100) >= _chance)
            {
                return;
            }
            _drop.SpawnDrop(addItem);
        }
    }
}