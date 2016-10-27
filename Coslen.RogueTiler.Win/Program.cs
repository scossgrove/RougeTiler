using System;
using Coslen.RogueTiler.Domain;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.UI.GameOver;
using Coslen.RogueTiler.Win.UI.GameVictory;
using Coslen.RogueTiler.Win.UI.InGame;
using Coslen.RogueTiler.Win.UI.NewPlayer;

namespace Coslen.RogueTiler.Win
{
    public enum ScreenType
    {
        None = 0,
        MainMenu,
        NewPlayer,
        InGame,
        GameOver,
        GameVictory
    }

    public class ScreenTransitionResult
    {
        public ScreenType FromScreen { get; set; }
        public ScreenType ToScreen { get; set; }
        public object Result { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var storage = new Storage(GameContent.Instance);
            storage.Load();

            var screenResult = new ScreenTransitionResult { ToScreen = ScreenType.MainMenu };

            var exitLoop = false;
            do
            {
                switch (screenResult.ToScreen)
                {
                    case ScreenType.None:
                        {
                            exitLoop = true;
                            break;
                        }
                    case ScreenType.MainMenu:
                        {
                            var screen = new MainMenuScreen(storage);
                            screenResult = screen.Process();
                            break;
                        }
                    case ScreenType.NewPlayer:
                        {
                            var screen = new NewPlayerScreen(storage);
                            screenResult = screen.Process();
                            break;
                        }
                    case ScreenType.InGame:
                        {
                            try
                            {
                                var screen = new InGameScreen(storage, screenResult.Result.ToString());
                                screenResult = screen.Process();
                            }
                            catch (StageNotFoundExpection ex)
                            {
                                screenResult = new ScreenTransitionResult()
                                {
                                    FromScreen = ScreenType.InGame,
                                    ToScreen = ScreenType.GameVictory
                                };
                            }
                            break;
                        }
                    case ScreenType.GameOver:
                        {
                            var screen = new GameOverScreen(storage);
                            screenResult = screen.Process();
                            break;
                        }
                    case ScreenType.GameVictory:
                        {
                            var screen = new GameVictoryScreen(storage);
                            screenResult = screen.Process();
                            break;
                        }
                    default:
                        {
                            throw new ApplicationException("Invalid Screen / Unhandled Screen");
                        }
                }
            } while (exitLoop == false);


            //GameRunner runner = new GameRunner();
            //runner.Process();
        }
    }
}