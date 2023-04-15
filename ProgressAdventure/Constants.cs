using SaveFileManager;
using System.Text;

namespace ProgressAdventure
{
    public class Constants
    {
        // thread names
        public const string MAIN_THREAD_NAME = "Main";
        public const string AUTO_SAVE_THREAD_NAME = "Auto saver";
        public const string MANUAL_SAVE_THREAD_NAME = "Quit manager";
        public const string TEST_THREAD_NAME = "Test";
        public const string VISUALIZER_THREAD_NAME = "Visualizer";

        // paths/folders/file names
        public static readonly string ROOT_FOLDER = Directory.GetCurrentDirectory();

        // saves folder
        public const string SAVES_FOLDER = "saves";
        public static readonly string SAVES_FOLDER_PATH = Path.Join(ROOT_FOLDER, SAVES_FOLDER);
        public const string OLD_SAVE_NAME = "save*";
        public const string SAVE_EXT = "savc";

        // logs folder
        public const string LOGS_FOLDER = "logs";
        public static readonly string LOGS_FOLDER_PATH = Path.Join(ROOT_FOLDER, LOGS_FOLDER);
        public const string LOG_EXT = "log";

        // backups folder
        public const string BACKUPS_FOLDER = "backups";
        public static readonly string BACKUPS_FOLDER_PATH = Path.Join(ROOT_FOLDER, BACKUPS_FOLDER);
        public const string OLD_BACKUP_EXT = SAVE_EXT + ".bak";
        public const string BACKUP_EXT = "zip";

        // save folder structure
        public const string SAVE_FILE_NAME_DATA = "data";
        public const string SAVE_FILE_NAME_POIS = "POIs";
        public const string SAVE_FOLDER_NAME_CHUNKS = "chunks";

        // seeds
        public const long SAVE_SEED = 87531;
        public const long SETTINGS_SEED = 1;

        // cursor types
        public static readonly CursorIcon STANDARD_CURSOR_ICONS = new CursorIcon(">", "", " ", "");
        public static readonly CursorIcon DELETE_CURSOR_ICONS = new CursorIcon(" X", "", "  ", "");

        // colors
        public static class Colors
        {
            public static readonly (byte r, byte g, byte b) RED = (255, 0, 0);
            public static readonly (byte r, byte g, byte b) GREEN = (0, 255, 0);
            public static readonly (byte r, byte g, byte b) BLUE = (0, 0, 255);
        }

        //logging
        public static bool LOGGING = true;
        public static int LOGGING_LEVEL = 0;
        public const bool LOG_MS = false;

        //chunks
        public const int CHUNK_SIZE = 10;
        public const string CHUNK_FILE_NAME = "chunk";
        public const string CHUNK_FILE_NAME_SEP = "_";

        // other
        public const bool ERROR_HANDLING = false;
        public static readonly Encoding ENCODING = Encoding.UTF8;
        public const int AUTO_SAVE_INTERVAL = 20;
        public const int AUTO_SAVE_DELAY = 5;
        public const int FILE_ENCODING_VERSION = 2;
        public const string SETTINGS_FILE_NAME = "settings";
        public const string SAVE_VERSION = "2.0";
        public const long TILE_NOISE_RESOLUTION = 1000000000000;
    }
}
