using System.Runtime.InteropServices;

namespace PACommon
{
    internal partial class NativeMethods
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        internal static partial IntPtr GetStdHandle(int nStdHandle);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetOpenFileName(ref OpenFileName ofn);
    }
}
