using System;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win
{
    public abstract class ScreenSectionBase
    {
        public ScreenSectionBase(string key, int left, int top, int bottom, int right, int renderOrder)
        {
            Key = key;
            Left = left;
            Top = top;
            Bottom = bottom;
            Right = right;
            RenderOrder = renderOrder;
            Enabled = true;
        }

        public string Key { get; set; }

        public int Left { get; set; }
        public int Top { get; set; }

        public int Bottom { get; set; }
        public int Right { get; set; }

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }


        public int RenderOrder { get; set; }

        public bool Enabled { get; set; }
        public bool ForceRedraw { get; set; }

        public int Padding_Top { get; set; } = 1;
        public int Padding_Left { get; set; } = 1;
        public int Padding_Right { get; set; } = 1;
        public int Padding_Bottom { get; set; } = 1;

        public int MaxWidth
        {
            get { return Right - Left - Padding_Left - Padding_Right; }
        }

        public int MaxHeight
        {
            get { return Bottom - Top - Padding_Top - Padding_Bottom; }
        }

        public void ClearArea(BufferContainer buffer)
        {
            buffer.Fill(" ", ConsoleColor.White, ConsoleColor.Black);
        }

        public void FrameArea(BufferContainer buffer)
        {
            var ascii = new ASCII();

            for (var lineLooper = 0; lineLooper < Height; lineLooper++)
            {
                var text = ascii[AsciiKeys.WallNS] + string.Empty.PadLeft(Width - 2, ' ') + ascii[AsciiKeys.WallNS];
                if (lineLooper == 0)
                {
                    text = ascii[AsciiKeys.WallCornerSE] + string.Empty.PadLeft(Width - 2, ascii[AsciiKeys.WallEW][0]) + ascii[AsciiKeys.WallCornerSW];
                }
                if (lineLooper == Height - 1)
                {
                    text = ascii[AsciiKeys.WallCornerNE] + string.Empty.PadLeft(Width - 2, ascii[AsciiKeys.WallEW][0]) + ascii[AsciiKeys.WallCornerNW];
                }

                WriteAt(buffer, 0, lineLooper, text, ConsoleColor.Yellow, ConsoleColor.Black, true);
            }
        }

        /// <summary>
        ///     This is to handle the writing of a message to the buffer.
        ///     Need to remember to add padding
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="message"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="backgroundColor"></param>
        public void WriteAt(BufferContainer buffer, int x, int y, string message, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black, bool ignorePadding = false)
        {
            if (message == null)
            {
                return;
            }

            // Drawing Logic
            // 1. Add padding to position
            // 2. Truncate the message. (AUTO)
            var transformedX = x;
            var transformedY = y;

            if (!ignorePadding)
            {
                transformedX = x + Padding_Left;
                transformedY = y + Padding_Top;

                if (message.Length > MaxWidth)
                {
                    message = message.Substring(0, MaxWidth - 3) + "...";
                }
            }
            else
            {
                if (message.Length > Width)
                {
                    message = message.Substring(0, Width - 3) + "...";
                }
            }

            buffer.Write($"{message}", transformedX, transformedY, foregroundColor, backgroundColor);
        }


        public abstract void Draw(BufferContainer buffer);

        protected static ConsoleKeyInfo GetPlayerInput()
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
    }
}