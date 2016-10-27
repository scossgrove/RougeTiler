using System;
using Coslen.RogueTiler.Domain.Utilities;

namespace Coslen.RogueTiler.Win.UI.GameOver
{
    public class GameOverLayout : ScreenLayoutBase
    {
        public GameOverLayout(Storage storage)
        {
            Storage = storage;
            Initialise();
        }

        public Storage Storage { get; set; }

        public override void Initialise()
        {
            var game = new GameOverScreenSection("gameOver", 0, 2, Console.WindowHeight - 2, Console.WindowWidth - 1, 1, Storage);
            game.Enabled = true;
            if (LayoutSections.ContainsKey("gameOver"))
            {
                LayoutSections["gameOver"] = game;
            }
            else
            {
                LayoutSections.Add("gameOver", game);
            }
        }
    }
}