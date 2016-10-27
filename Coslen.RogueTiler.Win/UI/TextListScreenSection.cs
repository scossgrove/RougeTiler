using System;
using System.Collections.Generic;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win
{
    public class TextListScreenSection : ScreenSectionBase, IScreenSection
    {
        private int previousLinesHashCode;

        public TextListScreenSection(string key, int left, int top, int bottom, int right, int renderOrder) : base(key, left, top, bottom, right, renderOrder)
        {
        }

        public List<string> Lines { get; set; } = new List<string>();

        // UI Console Setup
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public string Title { get; set; }

        public bool CanWrap { get; set; } = false;
        public string LinePrefix { get; set; }

        public bool HasChanged { get; set; }

        public override void Draw(BufferContainer buffer)
        {
            var lineOffset = 0;
            if (!string.IsNullOrWhiteSpace(Title))
            {
                buffer.Write(Title + ":", Left, lineOffset + Top, ConsoleColor.DarkYellow, BackgroundColor);
                lineOffset++;
            }

            // Stuff for all writting
            var prefixLength = 0;
            if (!string.IsNullOrWhiteSpace(LinePrefix))
            {
                prefixLength = LinePrefix.Length;
            }

            foreach (var line in Lines)
            {
                var workingLine = line.Trim();

                if (prefixLength > 0)
                {
                    buffer.Write(LinePrefix, Left, lineOffset + Top, ForegroundColor, BackgroundColor);
                }

                if (MaxWidth >= workingLine.Length + prefixLength)
                {
                    buffer.Write(workingLine, Left + prefixLength, lineOffset + Top, ForegroundColor, BackgroundColor);
                }
                else
                {
                    if (CanWrap)
                    {
                        var maxWrappedLineLength = MaxWidth - prefixLength;
                        var numerOfLoops = workingLine.Length/maxWrappedLineLength;
                        for (var lineLooper = 0; lineLooper <= numerOfLoops; lineLooper++)
                        {
                            var start = lineLooper*maxWrappedLineLength;
                            var end = start + maxWrappedLineLength;

                            var output = string.Empty;
                            if (end > workingLine.Length)
                            {
                                output = line.Substring(start);
                            }
                            else
                            {
                                output = workingLine.Substring(start, end - start);
                            }
                            output = output.Trim().PadRight(maxWrappedLineLength, ' ');

                            buffer.Write(output, Left + prefixLength, lineOffset + Top, ForegroundColor, BackgroundColor);

                            lineOffset++;
                        }
                        lineOffset--; // WriteAt(prefixLength, lineOffset, line, ForegroundColor, BackgroundColor);
                    }
                    else
                    {
                        // This will cause truncation and elispes being inserted.
                        buffer.Write(workingLine, Left + prefixLength, lineOffset + Top, ForegroundColor, BackgroundColor);
                    }
                }

                lineOffset++;
            }
        }

        private int GetLinesHashCode()
        {
            unchecked
            {
                var hashCode = 13;
                foreach (var item in Lines)
                {
                    var itemHashCode = !string.IsNullOrEmpty(item) ? item.GetHashCode() : 0;
                    hashCode = (hashCode*397) ^ itemHashCode;
                }
                return hashCode;
            }
        }
    }
}