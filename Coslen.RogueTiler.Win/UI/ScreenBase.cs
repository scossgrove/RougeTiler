using System;
using Coslen.RogueTiler.Win.UI.InGame.Helpers;

namespace Coslen.RogueTiler.Win.UI
{
    public abstract class ScreenBase
    {
        public ScreenTransitionResult ScreenResult { get; set; }

        protected Inputs Inputs { get { return Inputs.Instance;  } }

        public ScreenTransitionResult Process()
        {
            Console.Clear();
            Setup();

            ScreenLoop();

            return ScreenResult;
        }

        public virtual void ScreenLoop()
        {
            Draw();
            var exitLoop = false;
            do
            {
                Predraw();
                Draw();
                Postdraw();
                var inputResult = HandleInput();

                exitLoop = !inputResult;
            } while (exitLoop == false);
        }

        public int ScreenWidth { get; private set; }
        public int ScreenHeight  { get; private set; }
        public void SetUpConsoleSize()
        {
            // Adjust the current console
            ScreenWidth = 120;
            ScreenHeight = 50;
            Console.SetWindowSize(ScreenWidth, ScreenHeight);
            Console.CursorVisible = false;
            Console.SetBufferSize(ScreenWidth, ScreenHeight);
        }
        public abstract void Setup();
        public abstract void Draw();
        public abstract bool HandleInput();

        protected ConsoleKeyInfo GetPlayerInput()
        {
            var originalBackgroundColor = Console.BackgroundColor;
            var originalForegroundColor = Console.ForegroundColor;
            var originalCursorLeft = Console.CursorLeft;
            var originalCursorTop = Console.CursorTop;

            Console.SetCursorPosition(Console.WindowWidth - 2, Console.WindowHeight - 2);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Black;
            var result = Console.ReadKey();

            Console.BackgroundColor = originalBackgroundColor;
            Console.ForegroundColor = originalForegroundColor;
            Console.SetCursorPosition(originalCursorLeft, originalCursorTop);
            return result;
        }

        protected virtual void Predraw()
        {
        }

        protected virtual void Postdraw()
        {
        }

        protected void Debug(string message)
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 3);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message.Trim().PadRight(40, ' '));
        }
    }
}