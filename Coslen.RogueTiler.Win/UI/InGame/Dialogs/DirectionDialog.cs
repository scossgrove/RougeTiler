using System;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public enum DirectionDialogResult
    {
        None = 0,
        NorthWest,
        North,
        NorthEast,
        West,
        East,
        SouthWest,
        South,
        SouthEast
    }

    /// Modal dialog for letting the user perform an [Action] on an [Item]
    /// accessible to the [Hero].
    public class DirectionDialog : DialogBase
    {
        public new DirectionDialogResult DialogResult { get; private set; }
        public DirectionCommand Command { get; set; }

        public DirectionDialog(DirectionCommand command, Game game) : 
            this("DirectionDialog", 20, 10, 24, 40, -1, command, game)
        {}

        public DirectionDialog(string key, int left, int top, int bottom, int right, int renderOrder, DirectionCommand command, Game game) 
            : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
            Command = command;
        }

        public Game Game { get; }

        public override void Draw(BufferContainer buffer)
        {
            int linePostion = 0;
            
            WriteAt(buffer, 0, linePostion++, $"Which direction?");
            
            WriteAt(buffer, 0, linePostion++, $" nw  n  ne");
            WriteAt(buffer, 0, linePostion++, $"  \\ | /");
            WriteAt(buffer, 0, linePostion++, $"w - * - e");
            WriteAt(buffer, 0, linePostion++, $"  / | \\");
            WriteAt(buffer, 0, linePostion++, $" sw  s  se");
        }

        private Direction ConvertToVector(DirectionDialogResult source)
        {
            switch (source)
            {
                case DirectionDialogResult.NorthWest: return Direction.NorthWest;
                case DirectionDialogResult.North: return Direction.North;
                case DirectionDialogResult.NorthEast: return Direction.NorthEast;
                case DirectionDialogResult.West: return Direction.West;
                case DirectionDialogResult.East: return Direction.East;
                case DirectionDialogResult.SouthWest: return Direction.SouthWest;
                case DirectionDialogResult.South: return Direction.South;
                case DirectionDialogResult.SouthEast: return Direction.SouthEast;
                default: throw new ApplicationException("Invalid result!");
            }
        }

        private void ProcessCommand()
        {
            if (DialogResult == DirectionDialogResult.None)
            {
                return;
            }

            var action = Command.GetDirectionAction(Game, ConvertToVector(DialogResult));
            Game.Hero.SetNextAction(action);
        }
        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();
                
                if (input == Inputs.cancel)
                {
                    DialogResult = DirectionDialogResult.None;
                }
                if (input == Inputs.nw)
                {
                    DialogResult = DirectionDialogResult.NorthWest;
                }
                if (input == Inputs.n)
                {
                    DialogResult = DirectionDialogResult.North;
                }
                if (input == Inputs.ne)
                {
                    DialogResult = DirectionDialogResult.NorthEast;
                }
                if (input == Inputs.w)
                {
                    DialogResult = DirectionDialogResult.West;
                }
                if (input == Inputs.e)
                {
                    DialogResult = DirectionDialogResult.East;
                }
                if (input == Inputs.sw)
                {
                    DialogResult = DirectionDialogResult.SouthWest;
                }
                if (input == Inputs.s)
                {
                    DialogResult = DirectionDialogResult.South;
                }
                if (input == Inputs.se)
                {
                    DialogResult = DirectionDialogResult.SouthEast;
                }

                if (DialogResult != null)
                {
                    ProcessCommand();
                    return true;
                }
            } while (true);
        }
    }
}