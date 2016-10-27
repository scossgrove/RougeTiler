using System;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class LevelDownAction : Action
    {
        public override ActionResult OnPerform()
        {
            this.gameResult.ChangeLevel = true;
            this.gameResult.NewLevel = Game.CurrentStage.StageNumber + 1;

            return Succeed("{1} descends to the next level.", Actor);
        }
    }
}