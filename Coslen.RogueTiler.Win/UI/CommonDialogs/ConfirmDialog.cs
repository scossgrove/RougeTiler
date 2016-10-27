using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI.CommonDialogs
{
    public enum ConfirmDialogResult
    {
        None = 0,
        Yes,
        No
    }

    public class ConfirmDialog : DialogBase
    {
        public ConfirmDialog(string message) : base("Confirm Dialog")
        {
            Message = message;
        }

        public string Message { get; }
        public new ConfirmDialogResult DialogResult { get; private set; }

        public override void Draw(BufferContainer buffer)
        {
            WriteAt(buffer, 0, 0, $"{Message} [Y]/[N]");
        }

        protected override bool HandleInput()
        {
            var exitInputLoop = false;

            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    DialogResult = ConfirmDialogResult.No;
                    return true;
                }
                if (input == Inputs.yes)
                {
                    DialogResult = ConfirmDialogResult.Yes;
                    return true;
                }

                if (input == Inputs.no)
                {
                    DialogResult = ConfirmDialogResult.No;
                    return true;
                }
            } while (exitInputLoop == false);

            return false;
        }
    }
}