using System;
using Coslen.RogueTiler.Domain.Utilities;

namespace Coslen.RogueTiler.Win.UI.GameVictory
{
    public class GameVictoryLayout : ScreenLayoutBase
    {
        public GameVictoryLayout(Storage storage)
        {
            Storage = storage;
            Initialise();
        }

        public Storage Storage { get; set; }

        public override void Initialise()
        {
            var game = new GameVictoryScreenSection("GameVictory", 0, 2, Console.WindowHeight - 2, Console.WindowWidth - 1, 1, Storage);
            game.Enabled = true;
            if (LayoutSections.ContainsKey("GameVictory"))
            {
                LayoutSections["GameVictory"] = game;
            }
            else
            {
                LayoutSections.Add("GameVictory", game);
            }
        }
    }
}