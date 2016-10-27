using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class OpenDoorAction : Action
    {
        private readonly VectorBase doorPos;

        public OpenDoorAction(VectorBase doorPos)
        {
            this.doorPos = doorPos;
        }

        public override ActionResult OnPerform()
        {
            Game.CurrentStage[doorPos].Type = Game.CurrentStage[doorPos].Type.OpensTo;
            Game.CurrentStage.dirtyVisibility();

            return Succeed("{1} open[s] the door.", Actor);
        }
    }
}