using System;
using Coslen.RogueTiler.Domain.Utilities;

namespace Coslen.RogueTiler.Win.UI.InMainMenu
{
    public class MainMenuLayout : ScreenLayoutBase
    {
        public MainMenuLayout(Storage storage)
        {
            Storage = storage;
            Initialise();
        }

        public Storage Storage { get; set; }

        public int SelectedHeroIndex
        {
            get
            {
                var mainSegment = LayoutSections["MainMenu"] as MainMenuScreenSection;
                return mainSegment.SelectedHeroIndex;
            }
        }

        public override void Initialise()
        {
            var game = new MainMenuScreenSection("mainMenu", 0, 2, Console.WindowHeight - 2, Console.WindowWidth - 1, 1, Storage);
            game.Enabled = true;
            if (LayoutSections.ContainsKey("MainMenu"))
            {
                LayoutSections["MainMenu"] = game;
            }
            else
            {
                LayoutSections.Add("MainMenu", game);
            }
        }

        public void IncrementSelectedHero()
        {
            var mainSegment = LayoutSections["MainMenu"] as MainMenuScreenSection;
            mainSegment.SelectedHeroIndex++;
            if (mainSegment.SelectedHeroIndex == mainSegment.NumberOfHeroes)
            {
                mainSegment.SelectedHeroIndex = 0;
            }
        }

        public void DecrementSelectedHero()
        {
            var mainSegment = LayoutSections["MainMenu"] as MainMenuScreenSection;
            mainSegment.SelectedHeroIndex--;
            if (mainSegment.SelectedHeroIndex < 0)
            {
                mainSegment.SelectedHeroIndex = mainSegment.NumberOfHeroes - 1;
            }
        }
    }
}