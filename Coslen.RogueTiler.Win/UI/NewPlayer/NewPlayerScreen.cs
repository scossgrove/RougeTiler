using System;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Classes;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.UI.InMainMenu;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.NewPlayer
{
    public class NewPlayerScreen : ScreenBase
    {
        public NewPlayerScreen(Storage storage)
        {
            Storage = storage;
        }

        public Storage Storage { get; }
        public NewPlayerLayout Layout { get; set; }
        public string PlayerName { get; set; } = string.Empty;

        public override void Setup()
        {
            SetUpConsoleSize();

            // create the layout
            Layout = new NewPlayerLayout(Storage);
        }

        public override bool HandleInput()
        {
            var input = GetPlayerInput();
            if (input == Inputs.cancel)
            {
                ScreenResult = new ScreenTransitionResult {FromScreen = ScreenType.NewPlayer, ToScreen = ScreenType.MainMenu};

                return false;
            }
            if (input == Inputs.tab)
            {
                PlayerName = string.Empty;
                Layout.PlayerName = PlayerName;
                Layout.RandomName();
                return true;
            }
            if (input == Inputs.ok)
            {
                // Create the Hero
                // TODO: Other classes.
                var heroClass = new Warrior();
                var newPlayerName = string.IsNullOrWhiteSpace(PlayerName) ? Layout.DefaultName : PlayerName;
                var hero = GameContent.Instance.createHero(newPlayerName, heroClass);
                Storage.Heroes.Add(hero);
                Storage.Save();

                // Start the Game
                ScreenResult = new ScreenTransitionResult {FromScreen = ScreenType.NewPlayer, ToScreen = ScreenType.InGame, Result = newPlayerName};

                return false;
            }
            if (ConvertKeyToNumber(input) != -1)
            {
                if ((input.Modifiers & ConsoleModifiers.Shift) != 0)
                {
                    PlayerName += input.KeyChar.ToString().ToUpper();
                }
                else
                {
                    PlayerName += input.KeyChar;
                }
            }

            if (input.Key == ConsoleKey.Delete)
            {
                if (string.IsNullOrWhiteSpace(PlayerName))
                {
                    PlayerName = Layout.DefaultName;
                }
                PlayerName = PlayerName.Substring(0, PlayerName.Length - 1);
            }

            Layout.PlayerName = PlayerName;
            return true;
        }

        public int ConvertKeyToNumber(ConsoleKeyInfo key)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            return alphabet.IndexOf(key.KeyChar);
        }

        public override void Draw()
        {
            var buffer = new BufferContainer(0, 0, (short)ScreenHeight, (short)ScreenWidth);
            Layout.Draw(buffer);
        }
    }
}