namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    public class DetectItemsAction : Action
    {
        public override ActionResult OnPerform()
        {
            var numFound = 0;
            foreach (var item in Game.CurrentStage.Items)
            {
                // Ignore items already found.
                if (Game.CurrentStage[item.Position].IsExplored)
                {
                    continue;
                }

                numFound++;
                Game.CurrentStage[item.Position].IsExplored = true;

                gameResult.AddEvent(EventType.Detect, null, ElementFactory.Instance.None, item.Position, null, null);
            }

            if (numFound == 0)
            {
                return Succeed("The darkness holds no secrets.");
            }

            return Succeed("{1} sense[s] the treasures held in the dark!", Actor);
        }
    }
}