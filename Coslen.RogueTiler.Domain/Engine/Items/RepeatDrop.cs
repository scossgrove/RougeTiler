using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// A [Drop] that drops a child drop more than once.
    public class RepeatDrop : Drop
    {
        public int _count;
        public Drop _drop;

        public RepeatDrop(int count, Drop drop)
        {
            _count = count;
            _drop = drop;
        }

        public override void SpawnDrop(AddItem addItem)
        {
            var count = Rng.Instance.triangleInt(_count, _count/2) + Rng.Instance.taper(0, 5);
            for (var i = 0; i < count; i++)
            {
                _drop.SpawnDrop(addItem);
            }
        }
    }
}