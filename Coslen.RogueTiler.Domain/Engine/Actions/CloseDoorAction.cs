using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class CloseDoorAction : Action
    {
        private readonly VectorBase doorPos;

        public CloseDoorAction(VectorBase doorPos)
        {
            this.doorPos = doorPos;
        }

        public override ActionResult OnPerform()
        {
            var blockingActor = Game.CurrentStage.ActorAt(doorPos);
            if (blockingActor != null)
            {
                return Fail("{1} [are|is] in the way!", blockingActor);
            }

            Game.CurrentStage[doorPos].Type = Game.CurrentStage[doorPos].Type.ClosesTo;
            Game.CurrentStage.dirtyVisibility();

            return Succeed("{1} close[s] the door.", Actor);
        }
    }
}