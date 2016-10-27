using System;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class LevelUpAction : Action
    {
        public override ActionResult OnPerform()
        {
            this.gameResult.ChangeLevel = true;
            this.gameResult.NewLevel = Game.CurrentStage.StageNumber - 1;
            return Succeed("{1} ascends to the next level.", Actor);
        }
    }
}