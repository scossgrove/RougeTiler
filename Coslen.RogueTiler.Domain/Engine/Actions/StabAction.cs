using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// A melee attack that repeatedly hits in one direction.
    public class StabAction : FuryAction
    {
        /// How many frames it pauses between each step of the stab.
        private const int _frameRate = 4;

        private readonly Direction _dir;
        private int _step;

        public StabAction(Direction dir)
        {
            _dir = dir;
        }

        public int noise
        {
            get { return Option.NoiseHit; }
        }

        public override ActionResult PerformAttack()
        {
            var pos = Actor.Position + _dir;

            // Show the effect and perform the attack on alternate frames. This ensures
            // the effect gets a chance to be shown before the hit effect covers hit.
            if (_step%_frameRate == 0)
            {
                gameResult.AddEvent(EventType.Stab, null, ElementFactory.Instance.None, pos, _dir, null);
            }
            else if (_step%_frameRate == 1)
            {
                Attack(pos);
            }

            _step++;
            return DoneIf(_step == _frameRate*3);
        }

        public override string ToString()
        {
            return Actor + " stabs " + _dir;
        }
    }
}