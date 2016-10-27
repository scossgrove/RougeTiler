using System;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public enum CloseDoorDialogResult
    {
        None = 0
    }

    /// Modal dialog for letting the user perform an [Action] on an [Item]
    /// accessible to the [Hero].
    public class CloseDoorDialog : DialogBase
    {
        public new CloseDoorDialogResult DialogResult { get; private set; }

        public CloseDoorDialog(Game game) : 
            this("CloseDoorDialog", 20, 10, 24, Console.WindowWidth - 20, -1, game)
        {}

        public CloseDoorDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game) 
            : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; }

        public override void Draw(BufferContainer buffer)
        {
            int linePostion = 0;
            
            WriteAt(buffer, 0, linePostion++, "There are open door(s) to the:");
            foreach (var direction in Direction.All)
            {
                var position = Game.Hero.Position + direction;
                if (Game.CurrentStage[position].Type.ClosesTo != null)
                {
                    WriteAt(buffer, 0, linePostion++, $" {direction}");
                }
            }
            linePostion++;
            WriteAt(buffer, 0, linePostion, "Close which door?");
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();
                VectorBase direction = null;
                if (input == Inputs.cancel)
                {
                    DialogResult = CloseDoorDialogResult.None;
                    return true;
                }
                if (input == Inputs.nw)
                {
                    direction = Direction.NorthWest;
                }
                if (input == Inputs.n)
                {
                    direction = Direction.North;
                }
                if (input == Inputs.ne)
                {
                    direction = Direction.NorthEast;
                }
                if (input == Inputs.w)
                {
                    direction = Direction.West;
                }
                if (input == Inputs.e)
                {
                    direction = Direction.East;
                }
                if (input == Inputs.sw)
                {
                    direction = Direction.SouthWest;
                }
                if (input == Inputs.s)
                {
                    direction = Direction.South;
                }
                if (input == Inputs.se)
                {
                    direction = Direction.SouthEast;
                }

                if (direction != null)
                {
                    if (TryToCloseDoor(direction))
                    {
                        return true;
                    }
                }

            } while (true);
        }

        private bool TryToCloseDoor(VectorBase direction)
        {
            var position = Game.Hero.Position + direction;
            if (Game.CurrentStage[position].Type.ClosesTo != null)
            {
                Game.Hero.SetNextAction(new CloseDoorAction(position));
                return true;
            }
            
            // TODO: Don't like this at all, find a way to trigger a refresh of current buffer.
            Console.SetCursorPosition(Left + Padding_Left, Bottom - Padding_Bottom - 1);
            var message = "There is not an open door to the " + ((Direction) direction) + ".";
            Console.Write(message.PadRight(MaxWidth, ' '));
            return false;
        }
    }
}