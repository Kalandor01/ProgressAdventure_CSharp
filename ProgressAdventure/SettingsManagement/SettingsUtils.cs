using ProgressAdventure.Enums;
using SaveFileManager;

namespace ProgressAdventure.SettingsManagement
{
    public static class SettingsUtils
    {
        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up action types, to their ignore modes.
        /// </summary>
        public static readonly Dictionary<ActionType, List<GetKeyMode>> actionTypeIgnoreMapping = new()
        {
            [ActionType.ESCAPE] = new List<GetKeyMode> { GetKeyMode.IGNORE_ESCAPE },
            [ActionType.UP] = new List<GetKeyMode> { GetKeyMode.IGNORE_VERTICAL },
            [ActionType.DOWN] = new List<GetKeyMode> { GetKeyMode.IGNORE_VERTICAL },
            [ActionType.LEFT] = new List<GetKeyMode> { GetKeyMode.IGNORE_HORIZONTAL },
            [ActionType.RIGHT] = new List<GetKeyMode> { GetKeyMode.IGNORE_HORIZONTAL },
            [ActionType.ENTER] = new List<GetKeyMode> { GetKeyMode.IGNORE_ENTER },
            [ActionType.STATS] = new List<GetKeyMode>(),
            [ActionType.SAVE] = new List<GetKeyMode>(),
        };

        /// <summary>
        /// The dictionary pairing up action types, to responses.
        /// </summary>
        public static readonly Dictionary<ActionType, object> actionTypeResponseMapping = new()
        {
            [ActionType.ESCAPE] = Key.ESCAPE,
            [ActionType.UP] = Key.UP,
            [ActionType.DOWN] = Key.DOWN,
            [ActionType.LEFT] = Key.LEFT,
            [ActionType.RIGHT] = Key.RIGHT,
            [ActionType.ENTER] = Key.ENTER,
            [ActionType.STATS] = "STATS",
            [ActionType.SAVE] = "SAVE",
        };

        /// <summary>
        /// The dictionary pairing up settings keys, to the type, that they are expected to be in the settings file.
        /// </summary>
        public static readonly Dictionary<SettingsKey, Type> settingValueTypeMap = new()
        {
            [SettingsKey.AUTO_SAVE] = typeof(bool),
            [SettingsKey.LOGGING_LEVEL] = typeof(long),
            [SettingsKey.KEYBINDS] = typeof(Dictionary<string, object?>),
            [SettingsKey.ASK_DELETE_SAVE] = typeof(bool),
            [SettingsKey.ASK_REGENERATE_SAVE] = typeof(bool),
            [SettingsKey.DEF_BACKUP_ACTION] = typeof(long),
            [SettingsKey.ENABLE_COLORED_TEXT] = typeof(bool),
        };
        #endregion

        #region Public functions
        /// <summary>
        /// Returns the default keybind list, for a Keybinds object.
        /// </summary>
        public static List<ActionKey> GetDefaultKeybindList()
        {
            return
            [
                new(ActionType.ESCAPE, new List<ConsoleKeyInfo> { new((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false) }),
                new(ActionType.UP, new List<ConsoleKeyInfo> { new((char)0, ConsoleKey.UpArrow, false, false, false) }),
                new(ActionType.DOWN, new List<ConsoleKeyInfo> { new((char)0, ConsoleKey.DownArrow, false, false, false) }),
                new(ActionType.LEFT, new List<ConsoleKeyInfo> { new((char)0, ConsoleKey.LeftArrow, false, false, false) }),
                new(ActionType.RIGHT, new List<ConsoleKeyInfo> { new((char)0, ConsoleKey.RightArrow, false, false, false) }),
                new(ActionType.ENTER, new List<ConsoleKeyInfo> { new((char)ConsoleKey.Enter, ConsoleKey.Enter, false, false, false) }),
                new(ActionType.STATS, new List<ConsoleKeyInfo> { new('e', ConsoleKey.E, false, false, false) }),
                new(ActionType.SAVE, new List<ConsoleKeyInfo> { new('s', ConsoleKey.S, false, false, false) }),
            ];
        }

        /// <summary>
        /// Returns the "json" representation of the default settings file.
        /// </summary>
        public static Dictionary<string, object> GetDefaultSettings()
        {
            return new Dictionary<string, object>
            {
                [SettingsKey.AUTO_SAVE.ToString()] = true,
                [SettingsKey.LOGGING_LEVEL.ToString()] = 0,
                [SettingsKey.KEYBINDS.ToString()] = new Keybinds().ToJson(),
                [SettingsKey.ASK_DELETE_SAVE.ToString()] = true,
                [SettingsKey.ASK_REGENERATE_SAVE.ToString()] = true,
                [SettingsKey.DEF_BACKUP_ACTION.ToString()] = -1,
                [SettingsKey.ENABLE_COLORED_TEXT.ToString()] = true,
            };
        }
        #endregion
    }
}
