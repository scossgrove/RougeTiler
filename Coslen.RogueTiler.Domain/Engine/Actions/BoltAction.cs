using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Fires a bolt, a straight line of an elemental attack that stops at the
    /// first [Actor] is hits or opaque tile.
    public class BoltAction : LosAction
    {
        private readonly RangedAttack _attack;
        private readonly bool _canMiss;

        public BoltAction(VectorBase target, RangedAttack attack, bool canMiss = false) : base(target)
        {
            _canMiss = canMiss;
            _attack = attack;
        }

        public override int Range 
        {
            get { return _attack.Range; }
        }

        public override void OnStep(VectorBase pos)
        {
            AddEvent(EventType.Bolt, null, _attack.Element, pos, null, null);
        }

        public override bool OnHitActor(VectorBase pos, Actor actor)
        {
            var attack = _attack;

            // TODO: Should range increase odds of missing? If so, do that here. Also
            // need to tweak enemy AI then since they shouldn't always try to maximize
            // distance.
            attack.Perform(this, Actor, TargetAsActor, _canMiss);
            return true;
        }
    }
}