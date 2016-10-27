using System;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Win.UI;
using Coslen.RogueTiler.Win.UI.InGame;

namespace Coslen.RogueTiler.Win
{
    public class InGameLayout : ScreenLayoutBase
    {
        private readonly Game game;

        public InGameLayout(Game game)
        {
            this.game = game;

            Initialise();
        }

        public override void Initialise()
        {

            var heroStats = new HeroStatisticsScreenSection("Hero Stats", 0, 2, 3, Console.WindowWidth - 1, 5, game);
            heroStats.Enabled = true;
            heroStats.Padding_Top = 0;
            heroStats.Padding_Bottom = 0;
            if (LayoutSections.ContainsKey("Hero Stats"))
            {
                LayoutSections["Hero Stats"] = heroStats;
            }
            else
            {
                LayoutSections.Add("Hero Stats", heroStats);
            }


            var gameBoard = new GameBoardScreenSection("Game Board", 0, 5, Console.WindowHeight - 4, Console.WindowWidth - 42, 1);
            gameBoard.Enabled = true;
            gameBoard.HeroPosition = game.CurrentStage.LastHeroPosition;
            if (LayoutSections.ContainsKey("Game Board"))
            {
                LayoutSections["Game Board"] = gameBoard;
            }
            else
            {
                LayoutSections.Add("Game Board", gameBoard);
            }


            var history = new TextListScreenSection("History", Console.WindowWidth - 41, 5, 17, Console.WindowWidth - 1, 4);
            history.Title = "History";
            history.LinePrefix = "-";
            history.CanWrap = true;
            history.Enabled = true;
            history.Padding_Top = 0;

            if (LayoutSections.ContainsKey("History"))
            {
                LayoutSections["History"] = history;
            }
            else
            {
                LayoutSections.Add("History", history);
            }

            var visibleActors = new VisibleActorsScreenSection("Visible Actors", Console.WindowWidth - 41, 18, Console.WindowHeight - 4, Console.WindowWidth - 1, 4, game);
            visibleActors.Enabled = true;
            visibleActors.Padding_Top = 0;
            visibleActors.Padding_Bottom = 0;
            visibleActors.Padding_Left = 0;
            if (LayoutSections.ContainsKey("Visible Actors"))
            {
                LayoutSections["Visible Actors"] = visibleActors;
            }
            else
            {
                LayoutSections.Add("Visible Actors", visibleActors);
            }
        }
    }
}