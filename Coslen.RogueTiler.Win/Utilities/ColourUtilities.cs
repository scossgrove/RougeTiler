using System;

namespace Coslen.RogueTiler.Win.Utilities
{
    public static class ColourUtilities
    {
        public static ConsoleColor ConvertToConsoleColor(string color)
        {
            switch (color.ToLower())
            {
                case "darkgray":
                    {
                        return ConsoleColor.DarkGray;
                    }
                case "lightgray":
                    {
                        return ConsoleColor.Gray;
                    }
                case "gray":
                    {
                        return ConsoleColor.Gray;
                    }
                case "yellow":
                    {
                        return ConsoleColor.Yellow;
                    }
                case "darkyellow":
                    {
                        return ConsoleColor.DarkYellow;
                    }
                case "darkgold":
                    {
                        return ConsoleColor.DarkYellow;
                    }
                case "lightgold":
                    {
                        return ConsoleColor.Yellow;
                    }
                case "gold":
                    {
                        return ConsoleColor.Yellow;
                    }

                case "lightbrown":
                    {
                        return ConsoleColor.Magenta;
                    }
                case "brown":
                    {
                        return ConsoleColor.DarkMagenta;
                    }
                case "darkbrown":
                    {
                        return ConsoleColor.DarkMagenta;
                    }
                case "aqua":
                    {
                        return ConsoleColor.Blue;
                    }
                case "lightblue":
                    {
                        return ConsoleColor.Blue;
                    }
                case "blue":
                    {
                        return ConsoleColor.Blue;
                    }
                case "lightaqua":
                    {
                        return ConsoleColor.Blue;
                    }
                case "lightpurple":
                    {
                        return ConsoleColor.Blue;
                    }
                case "purple":
                    {
                        return ConsoleColor.DarkBlue;
                    }
                case "darkaqua":
                    {
                        return ConsoleColor.DarkBlue;
                    }
                case "darkblue":
                    {
                        return ConsoleColor.DarkBlue;
                    }
                case "lightorange":
                    {
                        return ConsoleColor.Red;
                    }
                case "orange":
                    {
                        return ConsoleColor.Red;
                    }
                case "lightred":
                    {
                        return ConsoleColor.Red;
                    }
                case "red":
                    {
                        return ConsoleColor.DarkRed;
                    }
                case "black":
                    {
                        return ConsoleColor.Black;
                    }
                case "white":
                    {
                        return ConsoleColor.White;
                    }
                case "lightgreen":
                    {
                        return ConsoleColor.Green;
                    }
                case "green":
                    {
                        return ConsoleColor.Green;
                    }
                case "darkgreen":
                    {
                        return ConsoleColor.Green;
                    }
                default:
                    {
                        //return ConsoleColor.White;
                        throw new ApplicationException($"Color not coded yet! Requested Colour = [{color}]");
                    }
            }
        }
    }
}