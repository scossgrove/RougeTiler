using System;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public enum ForfeitDialogResult
    {
        None = 0,
        Yes,
        No
    }

    /// Modal dialog for letting the user perform an [Action] on an [Item]
    /// accessible to the [Hero].
    public class ForfeitDialog : DialogBase
    {
        public new ForfeitDialogResult DialogResult { get; private set; }

        public ForfeitDialog(Game game) : 
            this("ForfeitDialog", 20, 10, 24, Console.WindowWidth - 20, -1, game)
        {}

        public ForfeitDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game) 
            : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; }

        public override void Draw(BufferContainer buffer)
        {
            WriteAt(buffer, 0, 0, "Are you sure you want to forfeit the level? [Y]/[N]");
            WriteAt(buffer, 0, 1, "You will lose all items and experience gained on the level.");
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();
                
                if (input == Inputs.cancel)
                {
                    DialogResult = ForfeitDialogResult.No;
                    return true;
                }
                if (input == Inputs.yes)
                {
                    DialogResult = ForfeitDialogResult.Yes;
                    return true;
                }
                if (input == Inputs.no)
                {
                    DialogResult = ForfeitDialogResult.No;
                    return true;
                }

            } while (true);
        }
    }
}