using System;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.UI;
using Coslen.RogueTiler.Win.UI.CommonDialogs;
using Coslen.RogueTiler.Win.UI.InMainMenu;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win
{
    public class MainMenuScreen : ScreenBase
    {
        public MainMenuScreen(Storage storage)
        {
            Storage = storage;
        }

        public Storage Storage { get; }
        public MainMenuLayout Layout { get; set; }

        public override void Setup()
        {
            SetUpConsoleSize();

            // create the layout
            Layout = new MainMenuLayout(Storage);
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

                if (input.Key == ConsoleKey.L)
                {
                    if (Layout.SelectedHeroIndex == -1)
                    {
                        Debug("You have not selected a hero yet....");
                        return true;
                    }
                    var selectedHero = Storage.Heroes[Layout.SelectedHeroIndex];
                    ScreenResult = new ScreenTransitionResult {FromScreen = ScreenType.NewPlayer, ToScreen = ScreenType.InGame, Result = selectedHero.Name};

                    Debug("Loading Hero....");
                    return false;
                }

                if (input.Key == ConsoleKey.N)
                {
                    Debug("New Hero....");
                    ScreenResult = new ScreenTransitionResult {FromScreen = ScreenType.MainMenu, ToScreen = ScreenType.NewPlayer};

                    return false;
                }

                if (input.Key == ConsoleKey.D)
                {
                    Debug("Deleting Hero....");

                    if (Layout.SelectedHeroIndex == -1)
                    {
                        Debug("You have not selected a hero yet....");
                    }
                    else
                    {
                        var dialog = new ConfirmDialog("Are you sure you want to delete this hero?");
                        dialog.Process();
                        var result = dialog.DialogResult;
                        Draw();
                        if (result == ConfirmDialogResult.Yes)
                        {
                            Debug("Deleting Hero = [Y]");
                            Storage.Heroes.RemoveAt(Layout.SelectedHeroIndex);
                            Storage.Save();
                            Layout.Storage = Storage;
                            Layout.Initialise();
                            var buffer = new BufferContainer(0, 0, (short)ScreenHeight, (short)ScreenWidth);
                            Layout.Draw(buffer);
                        }
                        else
                        {
                            Debug("Deleting Hero = [N]");
                        }
                    }
                    return true;
                }

                if (input == Inputs.n )
                {
                    Layout.DecrementSelectedHero();
                    return true;
                }

                if (input == Inputs.s)
                {
                    Layout.IncrementSelectedHero();
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