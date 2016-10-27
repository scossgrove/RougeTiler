using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Coslen.RogueTiler.Domain.Content.Factories;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;
using DropFactory = Coslen.RogueTiler.Domain.Content.Factories.DropFactory;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public class ConsoleDialog : DialogBase
    {
        public ConsoleDialog(Game game) :
            this("ConsoleDialog", 10, 10, Console.WindowHeight - 4, Console.WindowWidth - 10, -1, game)
        { }

        public ConsoleDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game)
            : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; }

        private string Command { get; set; }

        public override void Draw(BufferContainer buffer)
        {
            WriteAt(buffer, 0, 0, "What is your wish master? [Enter] to process.");
            WriteAt(buffer, 0, 1, Command + ">");
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    return true;
                }
                if (ConvertKeyToNumber(input) != -1)
                {
                    if ((input.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        Command += input.KeyChar.ToString().ToUpper();
                        return false;
                    }
                    else
                    {
                        Command += input.KeyChar;
                        return false;
                    }
                }

                if (input.Key == ConsoleKey.Delete || input.Key == ConsoleKey.Backspace)
                {
                    Command = Command.Substring(0, Command.Length - 1);
                    return false;
                }

                if (input == Inputs.ok)
                {
                    ProcessCommand();
                    return false;
                }



            } while (true);
        }

        private void ProcessCommand()
        {
            var rex = new Regex(@"("".*?""|[^ ""]+)+");
            //string test = "CALL \"C:\\My File Name With Space\" /P1 P1Value /P1 P2Value";
            //var array = rex.Matches(test).OfType<Match>().Select(m => m.Groups[0]).ToArray();

            var matches = rex.Matches(Command).OfType<Match>().Select(m => m.Groups[0]).ToArray();
            var primaryCommand = matches[0].Value;
            switch (primaryCommand.ToLower())
            {
                case "goto":
                    {
                        var location = matches[1].Value;

                        VectorBase pos = null;
                        switch (location.ToLower())
                        {
                            case "stairsdown":
                                {
                                    pos = Game.CurrentStage.StairDownPosition;
                                    break;
                                }
                            case "stairsup":
                                {
                                    pos = Game.CurrentStage.StairUpPosition;
                                    break;
                                }
                        }

                        if (pos == null)
                        {
                            Command = string.Empty;
                            break;
                        }
                        pos = Game.CurrentStage.FindDistantOpenTileNear(pos);

                        Game.CurrentStage.currentActor.Position = pos;
                        Game.CurrentStage.refreshVisibility(Game.CurrentStage.CurrentHero);
                        Command = string.Empty;
                        break;
                    }
                case "giveitem":
                    {
                        var itemType = matches[1].Value;
                        if (itemType[0] == '"')
                        {
                            itemType = itemType.Substring(1, itemType.Length - 2);
                        }
                        var quantity = 1;
                        if (matches.Length == 3)
                        {
                            quantity = int.Parse(matches[2].Value);
                        }
                        Drop processedDrop = DropFactory.parseDrop(itemType);
                        for (int index = 0; index < quantity; index++)
                        {
                            processedDrop.SpawnDrop(Game.Hero.Inventory.tryAdd);
                        }
                        Command = string.Empty;
                        break;
                    }

                case "giveset":
                    {
                        var setType = matches[1].Value;
                        var quantity = 1;

                        switch (setType.ToLower())
                        {
                            case "warrior":
                                {
                                    DropFactory.parseDrop("Chain Mail Cap").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Chain Mail Gloves").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Large Shield").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Falchion").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Crossbow").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Greaves").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Scale Mail").SpawnDrop(Game.Hero.Equipment.replace);
                                    DropFactory.parseDrop("Fur Cloak").SpawnDrop(Game.Hero.Equipment.replace);
                                    break;
                                }
                        }


                        Command = string.Empty;
                        break;
                    }
                case "sethealth":
                    {
                        var newHealth = int.Parse(matches[1].Value);
                        Game.Hero.Health.Current = newHealth;
                        Command = string.Empty;
                        break;
                    }
            }

        }

        public int ConvertKeyToNumber(ConsoleKeyInfo key)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456 \"";

            return alphabet.IndexOf(key.KeyChar);
        }
    }
}
