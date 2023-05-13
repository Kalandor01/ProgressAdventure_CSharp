using SaveFileManager;
using System.Text;

namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing constants.
    /// </summary>
    public class Constants
    {
        #region Thread names
        /// <summary>
        /// The display name of the main thread.
        /// </summary>
        public const string MAIN_THREAD_NAME = "Main";
        /// <summary>
        /// The display name of the auto save thread.
        /// </summary>
        public const string AUTO_SAVE_THREAD_NAME = "Auto saver";
        /// <summary>
        /// The display name of the manual save thread.
        /// </summary>
        public const string MANUAL_SAVE_THREAD_NAME = "Quit manager";
        #endregion

        #region Paths/folders/file names
        /// <summary>
        /// The path to the root folder.
        /// </summary>
        public static readonly string ROOT_FOLDER = Directory.GetCurrentDirectory();

        #region Saves
        /// <summary>
        /// The name of the saves folder.
        /// </summary>
        public const string SAVES_FOLDER = "saves";
        /// <summary>
        /// The path to the saves folder.
        /// </summary>
        public static readonly string SAVES_FOLDER_PATH = Path.Join(ROOT_FOLDER, SAVES_FOLDER);
        /// <summary>
        /// The extension used for encoded save files.
        /// </summary>
        public const string SAVE_EXT = "savc";
        #endregion

        #region Logs
        /// <summary>
        /// The name of the logs folder.
        /// </summary>
        public const string LOGS_FOLDER = "logs";
        /// <summary>
        /// The path to the logs folder.
        /// </summary>
        public static readonly string LOGS_FOLDER_PATH = Path.Join(ROOT_FOLDER, LOGS_FOLDER);
        /// <summary>
        /// The extension used for log files.
        /// </summary>
        public const string LOG_EXT = "log";
        #endregion

        #region Backups
        /// <summary>
        /// The name of the backups folder.
        /// </summary>
        public const string BACKUPS_FOLDER = "backups";
        /// <summary>
        /// The path to the backups folder.
        /// </summary>
        public static readonly string BACKUPS_FOLDER_PATH = Path.Join(ROOT_FOLDER, BACKUPS_FOLDER);
        /// <summary>
        /// The extension used for backup files.
        /// </summary>
        public const string BACKUP_EXT = "zip";
        #endregion

        #region Save folder structure
        /// <summary>
        /// The name of the data file in a save file.
        /// </summary>
        public const string SAVE_FILE_NAME_DATA = "data";
        /// <summary>
        /// The name of the point of interests file in a save file.
        /// </summary>
        public const string SAVE_FILE_NAME_POIS = "POIs";
        /// <summary>
        /// The name of the chunks folder in a save file.
        /// </summary>
        public const string SAVE_FOLDER_NAME_CHUNKS = "chunks";
        #endregion

        /// <summary>
        /// The name of the settings file.
        /// </summary>
        public const string SETTINGS_FILE_NAME = "settings";
        #endregion

        #region Seeds
        /// <summary>
        /// The seed used for encoded saves, and most other encoded files.
        /// </summary>
        public const long SAVE_SEED = 87531;
        /// <summary>
        /// The seed used for the encoded settings file.
        /// </summary>
        public const long SETTINGS_SEED = 1;
        #endregion

        #region Cursor types
        /// <summary>
        /// The <c>CursorIcon</c> used in most cases.
        /// </summary>
        public static readonly CursorIcon STANDARD_CURSOR_ICONS = new(">", "", " ", "");
        /// <summary>
        /// The <c>CursorIcon</c> for deletions.
        /// </summary>
        public static readonly CursorIcon DELETE_CURSOR_ICONS = new(" X", "", "  ", "");
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
        /// If logging is enabled or not.
        /// </summary>
        public static bool LOGGING_ENABLED = true;
        /// <summary>
        /// The current logging level.
        /// </summary>
        public static int LOGGING_LEVEL = 0;
        /// <summary>
        /// If the logger should log the milisecond in time.
        /// </summary>
        public const bool LOG_MS = false;
        #endregion

        #region Chunks
        /// <summary>
        /// The size of the grid, containing the tile in a chunk.
        /// </summary>
        public const int CHUNK_SIZE = 10;
        /// <summary>
        /// The beggining of the name of a chunk file.
        /// </summary>
        public const string CHUNK_FILE_NAME = "chunk";
        /// <summary>
        /// The separation string used in the name of the chunk file name.
        /// </summary>
        public const string CHUNK_FILE_NAME_SEP = "_";
        #endregion

        #region Other
        /// <summary>
        /// If there should be a final exeption cacher, that catces all errors.
        /// </summary>
        public const bool ERROR_HANDLING = false;
        /// <summary>
        /// The encoding of the text.
        /// </summary>
        public static readonly Encoding ENCODING = Encoding.UTF8;
        /// <summary>
        /// The interval at which the auto saver tries to save in seconds.
        /// </summary>
        public const int AUTO_SAVE_INTERVAL = 20;
        /// <summary>
        /// The interval at which the auto saver tries to save in seconds.
        /// </summary>
        public const int AUTO_SAVE_DELAY = 5;
        /// <summary>
        /// The version number to use in the file encoding.
        /// </summary>
        public const int FILE_ENCODING_VERSION = 2;
        /// <summary>
        /// The current save version.
        /// </summary>
        public const string SAVE_VERSION = "2.0";
        /// <summary>
        /// The division to use in tile noises.
        /// </summary>
        public const long TILE_NOISE_DIVISION = 200;
        #endregion
    }
}
