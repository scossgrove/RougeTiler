using Coslen.RogueTiler.Domain.Engine.AI;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Sends out a wave of sound, alerting nearby monsters.
    public class HowlAction : Action
    {
        private readonly int _range;
        private Flow _flow;
        private int _step;

        public HowlAction(int range)
        {
            _range = range;
        }

        public override ActionResult OnPerform()
        {
            if (_flow == null)
            {
                _flow = new Flow(Actor.Game.CurrentStage, Actor.Position, _range, false, true);

                Log("{1} howls!", Actor);
            }

            var howlCircle = new Circle(Actor.Position, _step);

            //while (howlCircle.edge.MoveNext())
            foreach(var point in howlCircle.Points)
            {
                //var pos = (VectorBase) howlCircle.edge.Current;
                if (!Game.CurrentStage.Bounds().Contains(point))
                {
                    continue;
                }
                if (_flow.GetDistance(point) == null)
                {
                    continue;
                }

                gameResult.AddEvent(EventType.Howl, null, ElementFactory.Instance.None, point, null, _step/_range);

                var actor = Game.CurrentStage.ActorAt(point);

                if (!(actor is Monster))
                {
                    continue;
                }
                var monster = actor as Monster;

                // TODO: Should also reduce fear.

                if (monster.IsAsleep)
                {
                    monster.wakeUp();
                    monster.Log("{1} wakes up!", monster);
                }
            }

            _step++;
            if (_step > _range)
            {
                return ActionResult.Success;
            }
            return ActionResult.NotDone;
        }
    }
}