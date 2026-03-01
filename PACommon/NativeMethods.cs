using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PACommon
{
    internal static partial class NativeMethods
    {
        internal const string LINUX_GLIBC = "libc.so";
        internal const string LINUX_GLIBC_IOCTL = "ioctl";
        
        private const string WIN_KERNEL32 = "kernel32.dll";
        private const string WIN_COMMON_DIALOG = "comdlg32.dll";
        private const string LINUX_GLIBC_DEFAULT = "libc.so.6";
        private const string LINUX_DYNAMIC_LINKING = "libdl.so.2";
        
        [SupportedOSPlatform("windows")]
        [LibraryImport(WIN_KERNEL32, SetLastError = true)]
        internal static partial nint GetStdHandle(int nStdHandle);

        [SupportedOSPlatform("windows")]
        [LibraryImport(WIN_KERNEL32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

        [SupportedOSPlatform("windows")]
        [LibraryImport(WIN_KERNEL32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

        [SupportedOSPlatform("windows")]
        [DllImport(WIN_COMMON_DIALOG, SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool GetOpenFileName(ref NativeStructs.OpenFileName ofn);
        
        [SupportedOSPlatform("windows")]
        [LibraryImport(WIN_KERNEL32, SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        internal static partial nint LoadLibrary(string name);
        
        [SupportedOSPlatform("windows")]
        [LibraryImport(WIN_KERNEL32, SetLastError = true, StringMarshalling = StringMarshalling.Custom, StringMarshallingCustomType = typeof(System.Runtime.InteropServices.Marshalling.AnsiStringMarshaller))]
        internal static partial nint GetProcAddress(nint hModule, string name);
        
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freeBSD")]
        [LibraryImport(LINUX_GLIBC_DEFAULT, EntryPoint = LINUX_GLIBC_IOCTL)]
        internal static partial int ControllDevice(int fileDescriptor, int opcode, nint dataPointer);
        internal delegate int ControllDeviceDynamic(int fileDescriptor, int opcode, nint dataPointer);
        
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freeBSD")]
        [LibraryImport(LINUX_DYNAMIC_LINKING, EntryPoint = "dlopen", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint LoadDynamicLibrary(string fileName, int flags);
        
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freeBSD")]
        [LibraryImport(LINUX_DYNAMIC_LINKING, EntryPoint = "dlclose", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial int CloseDynamicLibrary(nint libHandle);
        
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("freeBSD")]
        [LibraryImport(LINUX_DYNAMIC_LINKING, EntryPoint = "dlsym", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial nint LoadLibrarySymbol(nint libHandle, string symbol);
    }
}
