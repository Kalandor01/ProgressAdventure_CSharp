using System.Runtime.InteropServices;

namespace PACommon
{
    internal static class NativeStructs
    {
        /// <summary>
        /// Struct used in <see cref="Utils.OpenFileDialog"/>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct OpenFileName
        {
            public int lStructSize;
            public nint hwndOwner;
            public nint hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public nint lCustData;
            public nint lpfnHook;
            public string lpTemplateName;
            public nint pvReserved;
            public int dwReserved;
            public int flagsEx;
        }
        
        internal struct ControlDeviceWinSize
        {
            public ushort rowSize;
            public ushort columnSize;
            public ushort sizeX;  /* unused */
            public ushort sizeY;  /* unused */
        };
    }
}
