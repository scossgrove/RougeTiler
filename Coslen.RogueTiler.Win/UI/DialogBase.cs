using System;
using Coslen.RogueTiler.Win.UI.InGame.Helpers;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI
{
    public abstract class DialogBase : ScreenSectionBase
    {
        public DialogBase(string key) : this(key, 20, 10, Console.WindowHeight - 10, Console.WindowWidth - 20, -1)
        {
        }

        public DialogBase(string key, int left, int top, int bottom, int right, int renderOrder) : base(key, left, top, bottom, right, renderOrder)
        {
            Inputs = Inputs.Instance;
        }

        protected bool NoFrame { get; set; }
        protected Inputs Inputs { get; private set; }
        public object DialogResult { get; private set; }

        public void Process()
        {
            var buffer = new BufferContainer((short) Top, (short) Left, (short) Bottom, (short) Right);

            var bufferRender = new BufferRender();

            var exitLoop = false;
            do
            {
                if (NoFrame == false)
                {
                    FrameArea(buffer);
                }
                Draw(buffer);
                bufferRender.Render(buffer);
                exitLoop = HandleInput();
            } while (exitLoop == false);

            ClearArea(buffer);
        }

        /// <summary>
        ///     This handled the input processing for the dialog
        /// </summary>
        /// <returns> whether to continue the dialog loop </returns>
        protected abstract bool HandleInput();
    }
}