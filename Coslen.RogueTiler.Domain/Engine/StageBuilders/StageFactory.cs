using System;
using System.Linq;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;

namespace Coslen.RogueTiler.Domain.Engine.StageBuilders
{
    public static class StageFactory
    {
        public static Stage LoadStage(Game game, string heroName, int stageNumber)
        {
            // Get the Saved Data for the hero.
            var heroSave = game.Storage.Heroes.Single(h => h.Name == heroName);
            
            Stage stage = null;

            // Does the stage already exist?
            if (heroSave.Stages.Any() && heroSave.Stages.Count >= stageNumber + 1)
            {
                stage = LoadExistingStage(stageNumber, heroSave);
                heroSave.Position = stage.LastHeroPosition;
            }
            else
            {
                stage = CreateNewStage(game, stageNumber);

                // where is the stair?
                VectorBase position = stage.LastHeroPosition;
                heroSave.Position = position;

                // Store the new stage to the system
                heroSave.Stages.Add(stage);
                game.Storage.Save();
            }

            stage.StageNumber = stageNumber;
            stage.CalculateExplorableTiles();

            // Need to associate the monsters with the game
            foreach (var actor in stage.Actors)
            {
                actor.Game = game;
            }

            // Need to load the hero into the stage
            var heroPos = heroSave.Position;
            game.Hero = new Hero(game, heroPos, heroSave, null, heroName);
            stage.CurrentHero = game.Hero;
            stage.AddActor(game.Hero);
            stage.LastHeroPosition = heroPos;

            return stage;
        }

        private static Stage CreateNewStage(Game game, int stageNumber)
        {
            Stage stage = AreaFactory.Instance.CreateStage(game, stageNumber);
            stage.FinishBuild();
            stage.SetTileExplored(false);

            // Remove the temp Hero
            stage.removeActor(stage.CurrentHero);
            stage.CurrentHero = null;
            game.Hero = null;


            return stage;
        }

        private static Stage LoadExistingStage(int stageNumber, HeroSave heroSave)
        {
            Stage stage = heroSave.Stages[stageNumber];

            // Remove the old hero as we will add them back in below.
            stage.Actors.RemoveAll(x => x is Hero);
            foreach (var actor in stage.Actors)
            {
                if (actor is Hero)
                {
                    throw new ApplicationException("There should not be any heroes in the list.");
                }
            }
            
            return stage;
        }
    }
}
