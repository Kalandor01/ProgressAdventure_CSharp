using ConsoleUI;
using PACConstants = PACommon.Constants;

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
        /// The display name of the user actions thread.
        /// </summary>
        public const string USER_ACTIONS_THREAD_NAME = "User actions";
        #endregion

        #region Paths/folders/file names/extensions
        #region Saves
        /// <summary>
        /// The name of the saves folder.
        /// </summary>
        public const string SAVES_FOLDER = "saves";
        /// <summary>
        /// The path to the saves folder.
        /// </summary>
        public static readonly string SAVES_FOLDER_PATH = Path.Join(PACConstants.ROOT_FOLDER, SAVES_FOLDER);
        /// <summary>
        /// The extension used for encoded save files before 2.3.
        /// </summary>
        public const string OLD_SAVE_EXT = "savc";
        /// <summary>
        /// The extension used for encoded save files.
        /// </summary>
        public const string SAVE_EXT = "sav";
        #endregion

        #region Logs
        /// <summary>
        /// The name of the logs folder.
        /// </summary>
        public const string LOGS_FOLDER = "logs";
        /// <summary>
        /// The path to the logs folder.
        /// </summary>
        public static readonly string LOGS_FOLDER_PATH = Path.Join(PACConstants.ROOT_FOLDER, LOGS_FOLDER);
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
        public static readonly string BACKUPS_FOLDER_PATH = Path.Join(PACConstants.ROOT_FOLDER, BACKUPS_FOLDER);
        /// <summary>
        /// The extension used for backup files.
        /// </summary>
        public const string BACKUP_EXT = "zip";
        #endregion

        #region Configs
        /// <summary>
        /// The name of the configs folder.
        /// </summary>
        public const string CONFIGS_FOLDER = "configs";
        /// <summary>
        /// The path to the configs folder.
        /// </summary>
        public static readonly string CONFIGS_FOLDER_PATH = Path.Join(PACConstants.ROOT_FOLDER, CONFIGS_FOLDER);
        /// <summary>
        /// The extension used for config files.
        /// </summary>
        public const string CONFIG_EXT = "json";
        #endregion

        #region Configs folder structure
        /// <summary>
        /// The name of the file, where the order of the enabled config loading is stored.
        /// </summary>
        public const string CONFIGS_LOADING_ORDER_FILE_NAME = "loading_order";
        /// <summary>
        /// The name of the data file in a config file.
        /// </summary>
        public const string CONFIG_FILE_NAME_DATA = "data";
        /// <summary>
        /// The namespace of the P.A. (vanilla) configs namespace/folder.
        /// </summary>
        public const string VANILLA_CONFIGS_NAMESPACE = "pa";
        /// <summary>
        /// The configs subfolder name for configs for <see cref="SettingsManagement.SettingsUtils"/>.
        /// </summary>
        public const string CONFIGS_SETTINGS_SUBFOLDER_NAME = "settings";
        /// <summary>
        /// The configs subfolder name for configs for <see cref="ItemManagement.ItemUtils"/>.
        /// </summary>
        public const string CONFIGS_ENTITY_SUBFOLDER_NAME = "entities";
        /// <summary>
        /// The configs subfolder name for configs for <see cref="ItemManagement.ItemUtils"/>>.
        /// </summary>
        public const string CONFIGS_ITEM_SUBFOLDER_NAME = "items";
        /// <summary>
        /// The configs subfolder name for configs for <see cref="WorldManagement.WorldUtils"/>.
        /// </summary>
        public const string CONFIGS_WORLD_SUBFOLDER_NAME = "world";
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

        #region Old seeds
        /// <summary>
        /// The seed that was used for encoded saves, and most other encoded files.
        /// </summary>
        public const long OLD_SAVE_SEED = 87531;
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
            public static readonly (byte r, byte g, byte b) WARNING = (255, 200, 0);
        }
        #endregion

        #region Json keys
        public static class JsonKeys
        {
            #region ConfigData
            public static class ConfigData
            {
                public const string NAMESPACE = "namespace";
                public const string VERSION = "version";
                public const string DEPENDENCIES = "dependencies";
            }
            #endregion

            #region SaveData
            public static class SaveData
            {
                public const string OLD_SAVE_VERSION = "saveVersion";
                public const string SAVE_VERSION = "save_version";
                public const string DISPLAY_NAME = "display_name";
                public const string LAST_SAVE = "last_save";
                public const string PLAYTIME = "playtime";
                public const string SAVE_NAME = "save_name";
                public const string PLAYER = "player";
                public const string RANDOM_STATES = "random_states";
            }
            #endregion

            #region DisplaySaveData
            public static class DisplaySaveData
            {
                public const string PLAYER_NAME = "player_name";
            }
            #endregion

            #region Entity
            public static class Entity
            {
                public const string TYPE = "type";
                public const string NAME = "name";
                public const string BASE_MAX_HP = "base_max_hp";
                public const string CURRENT_HP = "current_hp";
                public const string BASE_ATTACK = "base_attack";
                public const string BASE_DEFENCE = "base_defence";
                public const string BASE_AGILITY = "base_agility";
                public const string ORIGINAL_TEAM = "original_team";
                public const string CURRENT_TEAM = "current_team";
                public const string ATTRIBUTES = "attributes";
                public const string DROPS = "drops";
                public const string X_POSITION = "x_position";
                public const string Y_POSITION = "y_position";
                public const string FACING = "facing";
            }
            #endregion

            #region Player
            public static class Player
            {
                public const string INVENTORY = "inventory";
            }
            #endregion

            #region AItem
            public static class AItem
            {
                public const string TYPE = "type";
                public const string MATERIAL = "material";
                public const string AMOUNT = "amount";
            }
            #endregion

            #region CompoundItem
            public static class CompoundItem
            {
                public const string PARTS = "parts";
            }
            #endregion

            #region Inventory
            public static class Inventory
            {
                public const string ITEMS = "items";
            }
            #endregion

            #region RandomStates
            public static class RandomStates
            {
                public const string MAIN_RANDOM = "main_random";
                public const string WORLD_RANDOM = "world_random";
                public const string MISC_RANDOM = "misc_random";
                public const string TILE_TYPE_NOISE_SEEDS = "tile_type_noise_seeds";
                public const string CHUNK_SEED_MODIFIER = "chunk_seed_modifier";
            }
            #endregion

            #region ActionKey
            public static class ActionKey
            {
                public const string KEY = "key";
                public const string KEY_CHAR = "key_char";
                public const string MODIFIERS = "modifiers";
            }
            #endregion

            #region Chunk
            public static class Chunk
            {
                public const string OLD_FILE_VERSION = "fileVersion";
                public const string POSITION_X = "position_x";
                public const string POSITION_Y = "position_y";
                public const string FILE_VERSION = "file_version";
                public const string CHUNK_RANDOM = "chunk_random";
                public const string TILES = "tiles";
            }
            #endregion

            #region Tile
            public static class Tile
            {
                public const string RELATIVE_POSITION_X = "x_position";
                public const string RELATIVE_POSITION_Y = "y_position";
                public const string VISITED = "visited";
                public const string TERRAIN = "terrain";
                public const string STRUCTURE = "structure";
                public const string POPULATION = "population";
            }
            #endregion

            #region BaseContent
            public static class BaseContent
            {
                public const string TYPE = "type";
                public const string SUBTYPE = "subtype";
                public const string NAME = "name";
            }
            #endregion

            #region PopulationContent
            public static class PopulationContent
            {
                public const string AMOUNT = "amount";
            }
            #endregion

            #region KingdomStructure
            public static class KingdomStructure
            {
                public const string POPULATION = "population";
            }
            #endregion

            #region VillageStructure
            public static class VillageStructure
            {
                public const string POPULATION = "population";
            }
            #endregion

            #region MountainTerrain
            public static class MountainTerrain
            {
                public const string HEIGHT = "height";
            }
            #endregion

            #region OceanTerrain
            public static class OceanTerrain
            {
                public const string DEPTH = "depth";
            }
            #endregion

            #region ShoreTerrain
            public static class ShoreTerrain
            {
                public const string DEPTH = "depth";
            }
            #endregion
        }
        #endregion

        #region Logging
        /// <summary>
        /// If the logger should log the milisecond in time.
        /// </summary>
        public const bool LOG_MS = false;

        /// <summary>
        /// The minimum time between, where normal logging just stores the logs to a buffer for later logging.
        /// </summary>
        public static readonly TimeSpan FORCE_LOG_INTERVAL =
#if DEBUG
            new(0, 0, 0);
#else
            new(0, 0, 5);
#endif
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

        #region Configs
        /// <summary>
        /// Whether the default namespace, when loading namespaces strings is the currently loading namespace, or the vanilla namespace.
        /// </summary>
        public const bool DEFAULT_NAMESPACE_IS_CURRENT_NAMESPACE = true;
        /// <summary>
        /// The character to separate the namespace with the rest of the namespaced string.
        /// </summary>
        public const char NAMESPACE_SEPARATOR_CHAR = ':';
        /// <summary>
        /// The oldest recognised config version.
        /// </summary>
        public const string OLDEST_CONFIG_VERSION = "v1";
        /// <summary>
        /// The current config version.
        /// </summary>
        public const string CONFIG_VERSION = "v4";
        /// <summary>
        /// The string that should be at the beggining of a config key/enum value, to signify that that value should be removed.
        /// </summary>
        public const string CONFIG_REMOVE_BEGGINING = "-";
        #endregion

        #region Other
        /// <summary>
        /// If there should be a final exeption cacher, that catces all errors.
        /// </summary>
#if DEBUG
        public const bool ERROR_HANDLING = false;
#else
        public const bool ERROR_HANDLING = true;
#endif
        /// <summary>
        /// The interval at which the auto saver tries to save in milliseconds.
        /// </summary>
        public const int AUTO_SAVE_INTERVAL = 20000;
        /// <summary>
        /// The interval at which the auto saver tries to save in milliseconds.
        /// </summary>
        public const int AUTO_SAVE_DELAY = 5000;
        /// <summary>
        /// The oldest recognised save version.
        /// </summary>
        public const string OLDEST_SAVE_VERSION = "2.0";
        /// <summary>
        /// The current save version.
        /// </summary>
        public const string SAVE_VERSION = "2.3";
        /// <summary>
        /// The division to use in tile noises.
        /// </summary>
        public const long TILE_NOISE_DIVISION = 200;
        /// <summary>
        /// In a fight, if no one took damage in X turns, the fight automaticaly ends.
        /// </summary>
        public const int FIGHT_GIVE_UP_TURN_NUMBER = 20;
        /// <summary>
        /// The amount of digits to round an item's amount to, to counteract bad float math.
        /// </summary>
        public const int ITEM_AMOUNT_ROUNDING_DIGITS = 4;
        /// <summary>
        /// The limit to whether to display the item amount normaly, or in a scientific notation. The limit surpassed if amount > 10^x or amount < 10^-x.
        /// </summary>
        public const int ITEM_AMOUNT_SCIENTIFIC_FORMAT_DIGITS = 4;
        /// <summary>
        /// The save folder name to use, if the SaveData singleton was initialized from the Instance property.
        /// </summary>
        public const string DEFAULT_SAVE_DATA_SAVE_NAME = "[DEFAULT]";
        /// <summary>
        /// Whether to order the json correcters by version number.
        /// </summary>
        public const bool ORDER_JSON_CORRECTERS = true;
        /// <summary>
        /// How often to check, if the game is unpaused when it is in a pause lock.
        /// </summary>
        public const int GLOBALS_PAUSED_CHECK_FREQUENCY = 100;
        #endregion
    }
}
