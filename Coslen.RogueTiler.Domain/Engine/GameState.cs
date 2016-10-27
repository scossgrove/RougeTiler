using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Peristance;

namespace Coslen.RogueTiler.Domain.Engine
{
    public class GameState
    {
        private static GameState instance;
        private int _gameLevel;

        public Game Game { get; set; }

        public int GameLevel
        {
            get { return _gameLevel; }
            set
            {
                _gameLevel = value;
            }
        }

        public HeroSave HeroSave { get; set; }
        
        private GameState()
        {
            GameLevel = 0;
        }

        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState();
                }
                return instance;
            }
        }

        public void Reset()
        {
            Game = null;
            HeroSave = null;
            GameLevel = 0;
        }

        public void UpdateHeroSave()
        {
            if (HeroSave.Stages.Count > GameLevel)
            {
                // Update the Stage List
                HeroSave.Stages[GameLevel] = Game.CurrentStage;
            }
            else
            {
                HeroSave.Stages.Add(Game.CurrentStage);
            }

            HeroSave.CurrentStage = GameLevel;
            HeroSave.Health = Game.CurrentStage.CurrentHero.Health;
            HeroSave.Charge = Game.CurrentStage.CurrentHero.Charge;
            HeroSave.Gold = Game.CurrentStage.CurrentHero.Gold;
            HeroSave.Position = Game.CurrentStage.CurrentHero.Position;
            HeroSave.ExperienceCents = Game.CurrentStage.CurrentHero.ExperienceCents;
            HeroSave.Food = Game.CurrentStage.CurrentHero.Food;

            HeroSave.Inventory = Game.CurrentStage.CurrentHero.Inventory;
            HeroSave.Equipment = Game.CurrentStage.CurrentHero.Equipment;
            HeroSave.HeroClass = Game.CurrentStage.CurrentHero.HeroClass;
            HeroSave.BackPack = Game.CurrentStage.CurrentHero.BackPack;
            HeroSave.Crucible = Game.CurrentStage.CurrentHero.Crucible;
        }
    }
}