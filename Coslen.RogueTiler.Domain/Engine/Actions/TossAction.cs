using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// [Action] for throwing an [Item].
    /// 
    /// This is referred to as "toss" in the code but as "throw" in the user
    /// interface. "Toss" is used just to avoid using "throw" in code, which is a
    /// reserved word.
    internal class TossAction : ItemAction
    {
        private readonly VectorBase _target;

        public TossAction(ItemLocation location, int index, VectorBase target) : base(location, index)
        {
            _target = target;
        }

        public override ActionResult OnPerform()
        {
            if (!item.canToss)
            {
                return Fail("{1} can't be thrown.", item);
            }

            // Take the item and throw it.
            return alternate(new TossLosAction(_target, removeItem()));
        }
    }

    /// Action for handling the path of a thrown item while it's in flight.
    public class TossLosAction : LosAction
    {
        private readonly Item _item;

        /// `true` if the item has reached an [Actor] and failed to hit it. When this
        /// happens, the item will keep flying past its target until the end of its
        /// range.
        private bool _missed;

        public TossLosAction(VectorBase target, Item item) : base(target)
        {
            _item = item;
        }

        public override int Range
        {
            get { return _item.type.tossAttack.Range; }
        }

        public override void OnStep(VectorBase pos)
        {
            gameResult.AddEvent(EventType.Toss, null, ElementFactory.Instance.None, pos, null, _item);
        }

        public override bool OnHitActor(VectorBase pos, Actor actor)
        {
            var attack = _item.type.tossAttack;

            // TODO: Range should affect strike.
            if (!attack.Perform(this, Actor, TargetAsActor))
            {
                // The item missed, so keep flying.
                _missed = true;
                return false;
            }

            _endThrow(pos);
            return true;
        }

        public override void OnEnd(VectorBase pos)
        {
            _endThrow(pos);
        }

        public override bool OnTarget(VectorBase pos)
        {
            // If the item failed to make contact with an actor, it's no longer well
            // targeted and just keeps going.
            if (_missed)
            {
                return false;
            }

            _endThrow(pos);

            // Let the player aim at a specific tile on the ground.
            return true;
        }

        public void _endThrow(VectorBase pos)
        {
            // See if it breaks.
            if (Rng.Instance.Range(100) < _item.type.breakage)
            {
                Log("{1} breaks!", _item.type.tossAttack.Noun);
                return;
            }

            // Drop the item onto the ground.
            _item.Position = pos;
            Game.CurrentStage.Items.Add(_item);

            // TODO: Secondary actions: potions explode etc.
        }
    }
}