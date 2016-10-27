using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class TeleportAction : Action
    {
        private readonly int distance;

        public TeleportAction(int distance)
        {
            this.distance = distance;
        }

        public override ActionResult OnPerform()
        {
            var targets = new List<VectorBase>();

            var bounds = Rect.intersect(Rect.LeftTopRightBottom(Actor.Position.x - distance, Actor.Position.y - distance, Actor.Position.x + distance, Actor.Position.y + distance), Game.CurrentStage.Bounds());

            foreach (var pos in bounds.PointsInRect())
            {
                if (!Game.CurrentStage.IsInStage(pos))
                {
                    continue;
                }

                if (!Game.CurrentStage[pos].IsPassable)
                {
                    continue;
                }
                if (Game.CurrentStage.ActorAt(pos) != null)
                {
                    continue;
                }
                if (pos - Actor.Position > distance)
                {
                    continue;
                }
                targets.Add(pos);
            }

            if (targets.Count == 0)
            {
                return Fail("{1} couldn't escape.", Actor);
            }

            // Try to teleport as far as possible.
            var best = Rng.Instance.Item(targets);

            for (var tries = 0; tries < 10; tries++)
            {
                var pos = Rng.Instance.Item(targets);
                if (pos - Actor.Position > best - Actor.Position)
                {
                    best = pos;
                }
            }

            var from = Actor.Position;
            Actor.Position = best;

            gameResult.AddEvent(EventType.Teleport, Actor, ElementFactory.Instance.None, from, null, null);

            return Succeed("{1} teleport[s]!", Actor);
        }
    }
}