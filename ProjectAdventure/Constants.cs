using ProjectAdventure.Enums;
using SaveFileManager;
using System.Text;

namespace ProjectAdventure
{
    public class Constants
    {
        // thread names
        public static readonly string MAIN_THREAD_NAME = "Main";
        public static readonly string AUTO_SAVE_THREAD_NAME = "Auto saver";
        public static readonly string MANUAL_SAVE_THREAD_NAME = "Quit manager";
        public static readonly string TEST_THREAD_NAME = "Test";
        public static readonly string VISUALIZER_THREAD_NAME = "Visualizer";

        // paths/folders/file names
        public static readonly string ROOT_FOLDER = Directory.GetCurrentDirectory();

        // saves folder
        public static readonly string SAVES_FOLDER = "saves";
        public static readonly string SAVES_FOLDER_PATH = Path.Join(ROOT_FOLDER, SAVES_FOLDER);
        public static readonly string OLD_SAVE_NAME = "save*";
        public static readonly string SAVE_EXT = "sav";

        // logs folder
        public static bool LOGGING = true;
        public static int LOGGING_LEVEL = 0;
        public static readonly string LOGS_FOLDER = "logs";
        public static readonly string LOGS_FOLDER_PATH = Path.Join(ROOT_FOLDER, LOGS_FOLDER);
        public static readonly string LOG_EXT = "log";

        // backups folder
        public static readonly string BACKUPS_FOLDER = "backups";
        public static readonly string BACKUPS_FOLDER_PATH = Path.Join(ROOT_FOLDER, BACKUPS_FOLDER);
        public static readonly string OLD_BACKUP_EXT = SAVE_EXT + ".bak";
        public static readonly string BACKUP_EXT = "zip";

        // save folder structure
        public static readonly string SAVE_FILE_NAME_DATA = "data";
        public static readonly string SAVE_FILE_NAME_POIS = "POIs";
        public static readonly string SAVE_FOLDER_NAME_CHUNKS = "chunks";

        // seeds
        public static readonly long SAVE_SEED = 87531;
        public static readonly long SETTINGS_SEED = 1;

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

        // other
        public static readonly bool ERROR_HANDLING = false;
        public static readonly bool LOG_MS = false;
        public static readonly Encoding ENCODING = Encoding.UTF8;
        public static readonly int AUTO_SAVE_INTERVAL = 20;
        public static readonly int AUTO_SAVE_DELAY = 5;
        public static readonly int FILE_ENCODING_VERSION = 2;
        public static readonly string SETTINGS_FILE_NAME = "settings";
        public static readonly int CHUNK_SIZE = 10;
        public static readonly string CHUNK_FILE_NAME = "chunk";
        public static readonly string CHUNK_FILE_NAME_SEP = "_";
        public static readonly string SAVE_VERSION = "2.0";
        public static readonly char[] DOUBLE_KEYS = new char[] { '\xe0', '\x00' };
        public static readonly long TILE_NOISE_RESOLUTION = 1000000000000;
    }
}
