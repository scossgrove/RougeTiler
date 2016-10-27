using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Creates a 45° swath of damage that radiates out from a point.
    public class RayAction : Action
    {
        private readonly RangedAttack _attack;

        /// The centerpoint that the cone is radiating from.
        private readonly VectorBase _from;

        /// The tiles that have already been touched by the effect. Used to make sure
        /// we don't hit the same tile multiple times.
        private readonly List<VectorBase> _hitTiles = new List<VectorBase>();

        // We "fill" the cone by tracing a number of rays, each of which can get
        // obstructed. This is the angle of each ray still being traced.
        private readonly List<double> _rays = new List<double>();

        /// The tile being targeted. The arc of the cone will center on a line from
        /// [_from] to this.
        private readonly VectorBase _to;

        /// The cone incrementally spreads outward. This is how far we currently are.
        private int _radius = 1;

        /// Creates a [RayAction] radiating from [_from] centered on [_to] (which
        /// may be the same as [_from] if the ray is a full circle. It applies
        /// [_attack] to each touched tile. The rays cover a chord whose width is
        /// [fraction] which varies from 0 (an infinitely narrow line) to 1.0 (a full
        /// circle.
        public RayAction(VectorBase from, VectorBase to, Attack attack, double fraction)
        {
            _from = from;
            _attack = (RangedAttack) attack;
            _to = to;

            // Don't hit the creator of the cone.
            _hitTiles.Add(_from);

            // We "fill" the cone by tracing a number of rays. We need enough of them
            // to ensure there are no gaps when the cone is at its maximum extent.
            var circumference = Math.PI*2*_attack.Range;
            var numRays = Math.Ceiling(circumference*fraction);

            // Figure out the center angle of the cone.
            var offset = _to - _from;
            // TODO: Make atan2 getter on VectorBase?
            var centerTheta = 0.0;
            if (_from != _to)
            {
                centerTheta = Math.Atan2(offset.x, offset.y);
            }

            // Create the rays.
            for (var i = 0; i < numRays; i++)
            {
                var range = (i/(numRays - 1)) - 0.5;
                _rays.Add(centerTheta + range*(Math.PI*2*fraction));
            }
        }

        /// A 45° cone of [attack] centered on the line from [from] to [to].
        public static RayAction Cone(VectorBase from, VectorBase to, Attack attack)
        {
            return new RayAction(from, to, attack, 1/8);
        }

        /// A complete ring of [attack] radiating outwards from [center].
        public static RayAction ring(VectorBase center, Attack attack)
        {
            return new RayAction(center, center, attack, 1.0);
        }


        private void TrimRay()
        {
            for (var index = _rays.Count - 1; index > 0; index--)
            {
                var ray = _rays[index];

                var pos = new VectorBase((int) (_from.x + Math.Round(Math.Sin(ray)*_radius)), (int) (_from.y + Math.Round(Math.Cos(ray)*_radius)));

                // Kill the ray if it's obstructed.
                if (!Game.CurrentStage[pos].IsTransparent)
                {
                    _rays.RemoveAt(index);
                    continue;
                }

                // Don't hit the same tile twice.
                if (_hitTiles.Contains(pos))
                {
                    continue;
                }

                AddEvent(EventType.Cone, null, _attack.Element, pos, null, null);

                _hitTiles.Add(pos);

                // See if there is an actor there.
                var target = Game.CurrentStage.ActorAt(pos);
                if (target != null && target != Actor)
                {
                    // TODO: Modify damage based on range?
                    _attack.Perform(this, Actor, target, false);
                }
            }
        }

        public override ActionResult OnPerform()
        {
            TrimRay();

            _radius++;
            if (_radius > _attack.Range || _rays.Count == 0)
            {
                return ActionResult.Success;
            }

            // Still going.
            return ActionResult.NotDone;
        }
    }
}