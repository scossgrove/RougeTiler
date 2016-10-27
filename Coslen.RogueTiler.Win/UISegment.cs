using System;
using System.Collections.Generic;

namespace Coslen.RogueTiler.Win
{
    public class UISegment
    {
        public UISegment(string key, int positionX, int positionY, int lineOffset, int lineLimit)
        {
            Key = key;
            position_X = positionX;
            position_Y = positionY;
            this.lineOffset = lineOffset;
            this.lineLimit = lineLimit;
        }

        public string Key { get; set; }

        public int position_X { get; set; }
        public int position_Y { get; set; }

        public int lineOffset { get; set; }
        public int lineLimit { get; set; }

        public int RenderOrder { get; set; }

        public List<string> Lines { get; set; } = new List<string>();

        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public string Title { get; set; }

        public void Draw()
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;

            int lineOffset = 0;
            if (!string.IsNullOrWhiteSpace(Title))
            {
                Console.SetCursorPosition(position_X, position_Y + lineOffset);
                Console.WriteLine(Title + ":");
                lineOffset++;
            }

            foreach (var line in Lines)
            {
                Console.SetCursorPosition(position_X, position_Y + lineOffset);
                
                if (line.Length < lineLimit)
                {
                    Console.WriteLine(line);
                }
                else
                {
                    Console.WriteLine(line.Substring(lineOffset, lineLimit));
                }
                lineOffset++;
            }
        }
    }
}