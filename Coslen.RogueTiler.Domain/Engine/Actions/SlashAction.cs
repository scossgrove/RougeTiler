using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// A sweeping melee attack that hits three adjacent tiles.
    public class SlashAction : FuryAction
    {
        /// How many frames it pauses between each step of the swing.
        private const int _frameRate = 5;

        private readonly Direction _dir;
        public int _step;

        public SlashAction(Direction dir)
        {
            _dir = dir;
        }

        public int noise
        {
            get { return Option.NoiseHit; }
        }

        public override ActionResult PerformAttack()
        {
            var dir = Direction.None;
            switch (_step/_frameRate)
            {
                case 0:
                    dir = _dir.rotateLeft45();
                    break;
                case 1:
                    dir = _dir;
                    break;
                case 2:
                    dir = _dir.rotateRight45();
                    break;
            }

            // Show the effect and perform the attack on alternate frames. This ensures
            // the effect gets a chance to be shown before the hit effect covers hit.
            if (_step%2 == 0)
            {
                gameResult.AddEvent(EventType.Slash, null, ElementFactory.Instance.None, Actor.Position + dir, _dir, null);
            }
            else if (_step%2 == 1)
            {
                Attack(Actor.Position + dir);
            }

            _step++;
            return DoneIf(_step == _frameRate*3);
        }

        public override string ToString()
        {
            return Actor + " slashes " + _dir;
        }
    }
}