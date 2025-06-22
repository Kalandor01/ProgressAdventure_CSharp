using System.Runtime.Versioning;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Class for a proxy to the console.
    /// </summary>
    public class PAConsoleProxy : IPAConsoleProxy
    {
        #region Constants
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;
        #endregion

        #region Private fields
        private readonly object _writeLock = new();
        #endregion

        #region Properties
        public int ConsoleWidth => Console.BufferWidth;

        public int ConsoleHeight => Console.BufferHeight;

        public int CursorColumn
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int CursorRow
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public Encoding Encoding
        {
            get => Console.OutputEncoding;
            set => Console.OutputEncoding = value;
        }

        public string Title
        {
            [SupportedOSPlatform("windows")]
            get
            {
                if (OperatingSystem.IsWindows())
                {
                    return Console.Title;
                }
                else
                {
                    return "";
                }
            }
            set => Console.Title = value;
        }
        
        public bool KeyAvailable => Console.KeyAvailable;
        #endregion

        #region Methods
        public void Write(object? value)
        {
            lock (_writeLock)
            {
                WritePrivate(value);
            }
        }

        public void Write(string? text)
        {
            lock (_writeLock)
            {
                WritePrivate(text);
            }
        }

        public void WriteLine(object? value)
        {
            lock (_writeLock)
            {
                WriteLinePrivate(value);
            }
        }

        public void WriteLine(string? text)
        {
            lock (_writeLock)
            {
                WriteLinePrivate(text);
            }
        }

        public void WriteLine()
        {
            lock (_writeLock)
            {
                WriteLinePrivate();
            }
        }

        public ConsoleKeyInfo ReadKey(bool displayKey = true)
        {
            var key = Console.ReadKey(!displayKey);
            LogRead(key, displayKey);
            return key;
        }

        public string? ReadLine()
        {
            var line = Console.ReadLine();
            LogRead(line, true);
            return line;
        }

        public string? ReadLine(string text)
        {
            Write(text);
            return ReadLine();
        }

        public (int column, int row) GetCursorPosition()
        {
            return Console.GetCursorPosition();
        }

        public void SetCursorPosition(int column, int row)
        {
            Console.SetCursorPosition(column, row);
            LogSetPos(column, row);
        }

        public void PressKey(string text = "", bool displayKey = false)
        {
            Write(text);
            ReadKey(displayKey);
            WriteLine();
        }

        public void MoveCursor(int columnOffset, int rowOffset)
        {
            SetCursorPosition(
                Math.Clamp(CursorColumn + columnOffset, 0, ConsoleWidth - 1),
                Math.Clamp(CursorRow - rowOffset, 0, ConsoleHeight - 1)
            );
        }

        public void WriteAtPosition(string text, int column, int row, bool returnCursor = false)
        {
            lock (_writeLock)
            {
                var prewColumn = 0;
                var prewRow = 0;
                if (returnCursor)
                {
                    (prewColumn, prewRow) = GetCursorPosition();
                }

                SetCursorPosition(column, row);
                WritePrivate(text);

                if (returnCursor)
                {
                    SetCursorPosition(prewColumn, prewRow);
                }
            }
        }

        public bool TryEnableAnsiCodes()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return true;
            }

            var handle = NativeMethods.GetStdHandle(STD_OUTPUT_HANDLE);
            NativeMethods.GetConsoleMode(handle, out var mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            return NativeMethods.SetConsoleMode(handle, mode);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private methods
        private static void LogWrite(object? textObj, bool newLine)
        {
            if (!PACSingletons.IsInitialized)
            {
                return;
            }

            var logger = PACSingletons.Instance.Logger;
            if (!logger.LoggingEnabled || (int)logger.LoggingLevel > (int)Enums.LogSeverity.TRACE)
            {
                return;
            }

            logger.Log(
                "Text written to output",
                $"newline: {newLine}, text: {(textObj is null ? "[NULL]" : $"\"{textObj}\"")}",
                Enums.LogSeverity.TRACE,
                false
            );
        }

        private static void LogRead(string? readStr, bool displayed)
        {
            if (!PACSingletons.IsInitialized)
            {
                return;
            }

            var logger = PACSingletons.Instance.Logger;
            if (!logger.LoggingEnabled || (int)logger.LoggingLevel > (int)Enums.LogSeverity.TRACE)
            {
                return;
            }

            logger.Log(
                "Text read from user",
                $"displayed: {displayed}, text: {(readStr is null ? "[NULL]" : $"\"{readStr}\"")}",
                Enums.LogSeverity.TRACE,
                false
            );
        }

        private static void LogRead(ConsoleKeyInfo readKey, bool displayed)
        {
            if (!PACSingletons.IsInitialized)
            {
                return;
            }

            var logger = PACSingletons.Instance.Logger;
            if (!logger.LoggingEnabled || (int)logger.LoggingLevel > (int)Enums.LogSeverity.TRACE)
            {
                return;
            }

            var mods = string.Join(", ", Enum.GetValues<ConsoleModifiers>().Where(k => (readKey.Modifiers & k) != 0));
            logger.Log(
                "Character read from user",
                $"displayed: {displayed}, key: {readKey.Key}, char: \"{readKey.KeyChar}\", modifiers: {mods}",
                Enums.LogSeverity.TRACE,
                false
            );
        }

        private static void LogSetPos(int posCol, int posRow)
        {
            if (!PACSingletons.IsInitialized)
            {
                return;
            }

            var logger = PACSingletons.Instance.Logger;
            if (!logger.LoggingEnabled || (int)logger.LoggingLevel > (int)Enums.LogSeverity.TRACE)
            {
                return;
            }

            logger.Log(
                "Cusor position set",
                $"posX: {posCol}, posY: {posRow}",
                Enums.LogSeverity.TRACE,
                false
            );
        }

        /// <summary>
        /// Same as <see cref="Write(object?)"/> but doesn't enforce the lock.
        /// </summary>
        private static void WritePrivate(object? text)
        {
            Console.Write(text);
            LogWrite(text, false);
        }

        /// <summary>
        /// Same as <see cref="Write(string?)"/> but doesn't enforce the lock.
        /// </summary>
        private static void WritePrivate(string? text)
        {
            Console.Write(text);
            LogWrite(text, false);
        }

        /// <summary>
        /// Same as <see cref="WriteLine(object?)"/> but doesn't enforce the lock.
        /// </summary>
        private static void WriteLinePrivate(object? text)
        {
            Console.WriteLine(text);
            LogWrite(text, true);
        }

        /// <summary>
        /// Same as <see cref="WriteLine(string?)"/> but doesn't enforce the lock.
        /// </summary>
        private static void WriteLinePrivate(string? text)
        {
            Console.WriteLine(text);
            LogWrite(text, true);
        }

        /// <summary>
        /// Same as <see cref="WriteLine()"/> but doesn't enforce the lock.
        /// </summary>
        private static void WriteLinePrivate()
        {
            Console.WriteLine();
            LogWrite(null, true);
        }
        #endregion
    }
}
