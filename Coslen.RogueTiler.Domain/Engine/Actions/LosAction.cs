using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Base class for an [Action] that traces a path from the Actor along a [Los].
    public abstract class LosAction : Action
    {
        private readonly VectorBase _target;
        private VectorBase _lastPos;
        private IEnumerator<VectorBase> _los;

        public LosAction(VectorBase target)
        {
            _target = target;
        }

        public VectorBase Target {
            get { return _target; }
        }

        public Actor TargetAsActor
        {
            get { return GameState.Instance.Game.CurrentStage.ActorAt(_target); }
        }

        /// Override this to provide the range of the line.
        public abstract int Range { get; }


        public override ActionResult OnPerform()
        {
            if (_los == null)
            {
                _los = new Los(Actor.Position, _target).GetEnumerator();
                // Advance to the first tile.
                _los.MoveNext();

                _lastPos = Actor.Position;
            }

            var pos = (VectorBase) _los.Current;

            // Stop if we hit a wall or went out of range.
            if (!Game.CurrentStage[pos].IsTransparent || pos - Actor.Position > Range)
            {
                OnEnd(_lastPos);
                return Succeed();
            }

            OnStep(pos);

            // See if there is an Actor there.
            var target = Game.CurrentStage.ActorAt(pos);
            if (target != null && target != Actor)
            {
                if (OnHitActor(pos, target))
                {
                    return ActionResult.Success;
                }
            }

            if (pos == _target)
            {
                if (OnTarget(pos))
                {
                    return ActionResult.Success;
                }
            }

            _lastPos = pos;
            return DoneIf(!_los.MoveNext());
        }

        /// Override this to handle the LOS reaching an open tile.
        public abstract void OnStep(VectorBase pos);

        /// Override this to handle the LOS hitting an [Actor].
        /// 
        /// Return `true` if the LOS should stop here or `false` if it should keep
        /// going.
        public virtual bool OnHitActor(VectorBase pos, Actor actor)
        {
            return true;
        }

        /// Override this to handle the LOS hitting a wall or going out of range.
        /// 
        /// [pos] is the position on the path *before* failure. It's the last good
        /// position. It may be the Actor's position if the LOS hit a wall directly
        /// adjacent to the Actor.
        public virtual void OnEnd(VectorBase pos)
        {
        }

        /// Override this to handle the LOS reaching the target on an open tile.
        /// 
        /// If this returns `true`, the LOS will stop there. Otherwise it will
        /// continue until it reaches the end of its range or hits something.
        public virtual bool OnTarget(VectorBase pos)
        {
            return false;
        }
    }
}