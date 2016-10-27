using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Coslen.RogueTiler.Win.Utilities.BufferUtilities
{
    public class BufferRender
    {
        private readonly SafeFileHandle windowHandle;

        public BufferRender()
        {
            windowHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            Console.OutputEncoding = Encoding.Unicode;
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] uint fileAccess, [MarshalAs(UnmanagedType.U4)] uint fileShare, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, [MarshalAs(UnmanagedType.U4)] int flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool WriteConsoleOutput(SafeFileHandle hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion);

        public void Render(BufferContainer buffer)
        {
            if (!windowHandle.IsInvalid)
            {
                var rect = new SmallRect {Left = buffer.Left, Top = buffer.Top, Right = buffer.Right, Bottom = buffer.Bottom};

                var b = WriteConsoleOutput(windowHandle, buffer.Buffer,
                    // dwBufferSize
                    new Coord {X = (short) buffer.Width, Y = (short) buffer.Height},
                    // dwBufferCoord
                    //new Coord() { X = buffer.Left, Y = buffer.Top },
                    new Coord {X = 0, Y = 0}, ref rect);
            }
        }
    }
}