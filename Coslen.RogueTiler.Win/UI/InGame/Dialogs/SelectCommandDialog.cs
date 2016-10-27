using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public class SelectCommandDialog : DialogBase
    {
        public new Command DialogResult { get; private set; }

        public SelectCommandDialog(Game game) : 
            this("SelectCommandDialog", 20, 10, Console.WindowHeight - 10, Console.WindowWidth - 20, -1, game)
        {}

        public SelectCommandDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game) 
            : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; }

        private List<Command> HeroCommands
        {
            get
            {
                return Game.Hero.HeroClass.Commands;
            }
        }

        public override void Draw(BufferContainer buffer)
        {
            WriteAt(buffer, 0, 0, "Perform which command?");

            var lineCounter = 0;
            for (var i = 0; i < HeroCommands.Count; i++)
            {
                var y = i + 1;
                lineCounter++;
                var command = HeroCommands[i];

                WriteAt(buffer, 0, y, "( )   ", ConsoleColor.Gray);
                WriteAt(buffer, 1, y, "abcdefghijklmnopqrstuvwxyz"[i].ToString(), ConsoleColor.Yellow);
                WriteAt(buffer, 4, y, command.Name);
            }

            WriteAt(buffer, 0, lineCounter + 2, "[A-Z] Select command, [1-9] Bind quick key, [Esc] Exit", ConsoleColor.Gray);
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    DialogResult = null;
                    return true;
                }
                if (ConvertKeyToNumber(input) != -1 && ConvertKeyToNumber(input) < HeroCommands.Count)
                {
                    DialogResult = HeroCommands[ConvertKeyToNumber(input)];
                    return true;
                }
            } while (true);
        }

        public int ConvertKeyToNumber(ConsoleKeyInfo key)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyz";

            return alphabet.IndexOf(key.KeyChar);
        }
    }
}