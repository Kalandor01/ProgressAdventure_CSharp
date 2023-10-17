using SaveFileManager;
using System.Text;

namespace PACommon
{
    /// <summary>
    /// Object for storing constants.
    /// </summary>
    public class Constants
    {
        #region Threads
        /// <summary>
        /// The display name of the tests thread.
        /// </summary>
        public const string TESTS_THREAD_NAME = "Tests";
        #endregion

        #region Paths/folders/file names
        /// <summary>
        /// The path to the root folder.
        /// </summary>
        public static readonly string ROOT_FOLDER = Directory.GetCurrentDirectory();

        #region Logs
        /// <summary>
        /// The name of the logs folder.
        /// </summary>
        public const string DEFAULT_LOGS_FOLDER = "logs";
        /// <summary>
        /// The extension used for log files.
        /// </summary>
        public const string DEFAULT_LOG_EXT = "log";
        #endregion

        /// <summary>
        /// The name of the settings file.
        /// </summary>
        public const string SETTINGS_FILE_NAME = "settings";
        #endregion

        #region Seeds
        /// <summary>
        /// The seed used for the encoded settings file.
        /// </summary>
        public const long SETTINGS_SEED = 1;
        #endregion

        #region Cursor types
        /// <summary>
        /// The <c>CursorIcon</c> for <c>BaseUIDisplay</c>.
        /// </summary>
        public static readonly CursorIcon NO_CURSOR_ICONS = new("", "", "", "");
        #endregion

        #region Colors
        /// <summary>
        /// Object containing colors, used for writing out colored text.
        /// </summary>
        public static class Colors
        {
            public static readonly (byte r, byte g, byte b) RED = (255, 0, 0);
            public static readonly (byte r, byte g, byte b) GREEN = (0, 255, 0);
            public static readonly (byte r, byte g, byte b) BLUE = (0, 0, 255);
        }
        #endregion

        #region Logging
        /// <summary>
        /// If the logger should log the milisecond in time.
        /// </summary>
        public const bool DEFAULT_LOG_MS = false;
        #endregion

        #region Other
        /// <summary>
        /// The encoding of the text.
        /// </summary>
        public static readonly Encoding ENCODING = Encoding.UTF8;
        /// <summary>
        /// The version number to use in the file encoding.
        /// </summary>
        public const int FILE_ENCODING_VERSION = 2;
        #endregion
    }
}
