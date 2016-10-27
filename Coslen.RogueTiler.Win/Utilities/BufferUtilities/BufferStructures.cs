using System.Runtime.InteropServices;

namespace Coslen.RogueTiler.Win.Utilities.BufferUtilities
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
        public short Y;

        public Coord(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }
    };

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
    public struct CharInfo
    {
        [FieldOffset(0)] public char UnicodeChar;
        [FieldOffset(0)] public byte AsciiChar;
        [FieldOffset(2)] public short Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }
}