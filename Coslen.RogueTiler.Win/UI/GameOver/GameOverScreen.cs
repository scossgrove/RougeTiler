using System;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.UI.CommonDialogs;
using Coslen.RogueTiler.Win.UI.InMainMenu;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.GameOver
{
    public class GameOverScreen : ScreenBase
    {
        public GameOverScreen(Storage storage)
        {
            Storage = storage;
        }

        public Storage Storage { get; }
        public GameOverLayout Layout { get; set; }

        public override void Setup()
        {
            SetUpConsoleSize();

            // create the layout
            Layout = new GameOverLayout(Storage);
        }

        public override bool HandleInput()
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Black;

            var exitInputLoop = false;

            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    var dialog = new ConfirmDialog("Are you sure you want to exit the game?");
                    dialog.Process();
                    var result = dialog.DialogResult;
                    Draw();
                    if (result == ConfirmDialogResult.Yes)
                    {
                        Debug("Exit Game = [Y]");
                        ScreenResult = new ScreenTransitionResult {FromScreen = ScreenType.MainMenu, ToScreen = ScreenType.None};
                        return false;
                    }

                    return true;
                }
            } while (exitInputLoop == false);

            Console.ForegroundColor = originalColor;

            return false;
        }

        public int ConvertKeyToNumber(ConsoleKeyInfo key)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyz";

            return alphabet.IndexOf(key.KeyChar);
        }

        public override void Draw()
        {
            var buffer = new BufferContainer(0, 0, (short)ScreenHeight, (short)ScreenWidth);
            Layout.Draw(buffer);
        }
    }
}