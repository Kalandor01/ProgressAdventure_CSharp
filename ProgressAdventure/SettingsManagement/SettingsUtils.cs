using PACommon.JsonUtils;
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
            [ActionType.ESCAPE] = [GetKeyMode.IGNORE_ESCAPE],
            [ActionType.UP] = [GetKeyMode.IGNORE_VERTICAL],
            [ActionType.DOWN] = [GetKeyMode.IGNORE_VERTICAL],
            [ActionType.LEFT] = [GetKeyMode.IGNORE_HORIZONTAL],
            [ActionType.RIGHT] = [GetKeyMode.IGNORE_HORIZONTAL],
            [ActionType.ENTER] = [GetKeyMode.IGNORE_ENTER],
            [ActionType.STATS] = [],
            [ActionType.SAVE] = [],
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
        public static readonly Dictionary<SettingsKey, JsonObjectType> settingValueTypeMap = new()
        {
            [SettingsKey.AUTO_SAVE] = JsonObjectType.Bool,
            [SettingsKey.LOGGING_LEVEL] = JsonObjectType.WholeNumber,
            [SettingsKey.KEYBINDS] = JsonObjectType.Dictionary,
            [SettingsKey.ASK_DELETE_SAVE] = JsonObjectType.Bool,
            [SettingsKey.ASK_REGENERATE_SAVE] = JsonObjectType.Bool,
            [SettingsKey.DEF_BACKUP_ACTION] = JsonObjectType.WholeNumber,
            [SettingsKey.ENABLE_COLORED_TEXT] = JsonObjectType.Bool,
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
                new(ActionType.ESCAPE, [new((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)]),
                new(ActionType.UP, [new((char)0, ConsoleKey.UpArrow, false, false, false)]),
                new(ActionType.DOWN, [new((char)0, ConsoleKey.DownArrow, false, false, false)]),
                new(ActionType.LEFT, [new((char)0, ConsoleKey.LeftArrow, false, false, false)]),
                new(ActionType.RIGHT, [new((char)0, ConsoleKey.RightArrow, false, false, false)]),
                new(ActionType.ENTER, [new((char)ConsoleKey.Enter, ConsoleKey.Enter, false, false, false)]),
                new(ActionType.STATS, [new('e', ConsoleKey.E, false, false, false)]),
                new(ActionType.SAVE, [new('s', ConsoleKey.S, false, false, false)]),
            ];
        }

        /// <summary>
        /// Returns the "json" representation of the default settings file.
        /// </summary>
        public static JsonDictionary GetDefaultSettings()
        {
            return new JsonDictionary
            {
                [SettingsKey.AUTO_SAVE.ToString()] = PACommon.Tools.ParseToJsonValue(true),
                [SettingsKey.LOGGING_LEVEL.ToString()] = PACommon.Tools.ParseToJsonValue(0),
                [SettingsKey.KEYBINDS.ToString()] = new Keybinds().ToJson(),
                [SettingsKey.ASK_DELETE_SAVE.ToString()] = PACommon.Tools.ParseToJsonValue(true),
                [SettingsKey.ASK_REGENERATE_SAVE.ToString()] = PACommon.Tools.ParseToJsonValue(true),
                [SettingsKey.DEF_BACKUP_ACTION.ToString()] = PACommon.Tools.ParseToJsonValue(-1),
                [SettingsKey.ENABLE_COLORED_TEXT.ToString()] = PACommon.Tools.ParseToJsonValue(true),
            };
        }
        #endregion
    }
}
