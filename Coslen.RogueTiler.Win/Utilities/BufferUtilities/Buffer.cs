using System;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Win.Utilities.BufferUtilities
{
    public class BufferContainer
    {
        public BufferContainer(short top, short left, short bottom, short right)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;

            Clear();
        }

        public short Top { get; set; }
        public short Left { get; set; }

        public short Bottom { get; set; }
        public short Right { get; set; }

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }

        public CharInfo[] Buffer { get; set; }

        public void Clear()
        {
            Buffer = new CharInfo[Width*Height];
        }

        public void Draw(Appearence source, int x, int y)
        {
            var bufferIndex = y*Width + x;
            Buffer[bufferIndex].Attributes = ConvertColors(source);
            if (source.Glyph[0] > 255)
            {
                int problemByte = source.Glyph[0];
                var unicodeString = "\\u" + problemByte.ToString("X").PadLeft(4, '0');
                var unicodeChar = Convert.ToChar(problemByte);

                Buffer[bufferIndex].UnicodeChar = unicodeChar;
            }
            else
            {
                Buffer[bufferIndex].AsciiChar = Convert.ToByte(source.Glyph[0]);
            }
        }

        public short ConvertColors(Appearence source)
        {
            return (short) (ConvertColour(source.ForeGroundColor) | (ConvertColour(source.BackGroundColor) << 4));
        }

        public short ConvertColour(string colour)
        {
            switch (colour.ToLower())
            {
                case "black": // The color black.
                {
                    return (short) ConsoleColor.Black;
                }
                case "blue": // The color blue.
                {
                    return (short) ConsoleColor.Blue;
                }
                case "cyan": // The color cyan (blue-green).
                {
                    return (short) ConsoleColor.Cyan;
                }
                case "darkblue": // The color dark blue.
                {
                    return (short) ConsoleColor.DarkBlue;
                }
                case "darkcyan": // The color dark cyan (dark blue-green).
                {
                    return (short) ConsoleColor.DarkCyan;
                }
                case "darkgray": // The color dark gray.
                {
                    return (short) ConsoleColor.DarkGray;
                }
                case "darkgreen": // The color dark green.
                {
                    return (short) ConsoleColor.DarkGreen;
                }
                case "darkmagenta": // The color dark magenta (dark purplish-red).
                {
                    return (short) ConsoleColor.DarkMagenta;
                }
                case "darkred": // The color dark red.
                {
                    return (short) ConsoleColor.DarkRed;
                }
                case "darkyellow": // The color dark yellow (ochre).
                {
                    return (short) ConsoleColor.DarkYellow;
                }
                case "gray": // The color gray.
                {
                    return (short) ConsoleColor.Gray;
                }
                case "green": // The color green.
                {
                    return (short) ConsoleColor.Green;
                }
                case "magenta": // The color magenta (purplish-red).
                {
                    return (short) ConsoleColor.Magenta;
                }
                case "red": // The color red.
                {
                    return (short) ConsoleColor.Red;
                }
                case "white": // The color white.
                {
                    return (short) ConsoleColor.White;
                }
                case "yellow": // The color yellow.
                {
                    return (short) ConsoleColor.Yellow;
                }
                default:
                {
                    throw new ApplicationException($"Colour converstion not done for {colour}");
                }
            }
        }

        public void Write(string message, int x, int y, ConsoleColor foreGroundColor = ConsoleColor.White, ConsoleColor backGroundColor = ConsoleColor.Black)
        {
            if (message == null)
            {
                return;
            }

            var offsetX = 0;
            foreach (var character in message)
            {
                var appearence = new Appearence {Glyph = character.ToString(), ForeGroundColor = foreGroundColor.ToString(), BackGroundColor = backGroundColor.ToString()};

                Draw(appearence, x + offsetX, y);

                offsetX++;
            }
        }

        public void Fill(string glyph, ConsoleColor foreGroundColor, ConsoleColor backGroundColor)
        {
            var appearance = new Appearence {Glyph = glyph, ForeGroundColor = foreGroundColor.ToString(), BackGroundColor = backGroundColor.ToString()};

            var numberOfElementsInBuffer = Width*Height;

            for (var i = 0; i < numberOfElementsInBuffer; i++)
            {
                Buffer[i].Attributes = ConvertColors(appearance);
                Buffer[i].AsciiChar = Convert.ToByte(appearance.Glyph[0]);
            }
        }
    }
}