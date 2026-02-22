using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PACommon
{
    internal static partial class NativeMethods
    {
        [SupportedOSPlatform("windows")]
        [LibraryImport("kernel32.dll", SetLastError = true)]
        internal static partial nint GetStdHandle(int nStdHandle);

        [SupportedOSPlatform("windows")]
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

        [SupportedOSPlatform("windows")]
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

        [SupportedOSPlatform("windows")]
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetOpenFileName(ref OpenFileName ofn);
    }
}
