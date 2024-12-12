using ConsoleUI.Keybinds;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;

namespace ProgressAdventure.SettingsManagement
{
    public static class SettingsUtils
    {
        #region Default config dicts
        /// <summary>
        /// The default value for the config used for the value of <see cref="ActionTypeIgnoreMapping"/>.
        /// </summary>
        private static readonly Dictionary<ActionType, List<GetKeyMode>> _defaultActionTypeIgnoreMapping = new()
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
        /// The default value for the config used for the value of <see cref="ActionTypeResponseMapping"/>.
        /// </summary>
        private static readonly Dictionary<ActionType, string> _defaultActionTypeResponseMapping = new()
        {
            [ActionType.ESCAPE] = Key.ESCAPE.ToString(),
            [ActionType.UP] = Key.UP.ToString(),
            [ActionType.DOWN] = Key.DOWN.ToString(),
            [ActionType.LEFT] = Key.LEFT.ToString(),
            [ActionType.RIGHT] = Key.RIGHT.ToString(),
            [ActionType.ENTER] = Key.ENTER.ToString(),
            [ActionType.STATS] = "STATS",
            [ActionType.SAVE] = "SAVE",
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="SettingValueTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<SettingsKey, JsonObjectType> _defaultSettingValueTypeMap = new()
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

        #region Config dictionaries
        /// <summary>
        /// The dictionary pairing up action types, to their ignore modes.
        /// </summary>
        public static Dictionary<ActionType, List<GetKeyMode>> ActionTypeIgnoreMapping { get; private set; }

        /// <summary>
        /// The dictionary pairing up action types, to responses.
        /// </summary>
        public static Dictionary<ActionType, string> ActionTypeResponseMapping { get; private set; }

        /// <summary>
        /// The dictionary pairing up settings keys, to the type, that they are expected to be in the settings file.
        /// </summary>
        public static Dictionary<SettingsKey, JsonObjectType> SettingValueTypeMap { get; private set; }
        #endregion

        #region Public functions
        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            ActionTypeIgnoreMapping = _defaultActionTypeIgnoreMapping;
            ActionTypeResponseMapping = _defaultActionTypeResponseMapping;
            SettingValueTypeMap = _defaultSettingValueTypeMap;
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            ConfigManager.Instance.SetConfig("action_type_ignore_mapping", "v.1", _defaultActionTypeIgnoreMapping);
            ConfigManager.Instance.SetConfig("action_type_response_mapping", "v.1", _defaultActionTypeResponseMapping);
            ConfigManager.Instance.SetConfig("setting_value_type_map", "v.1", _defaultSettingValueTypeMap);
        }

        /// <summary>
        /// Reloads all variables that come from configs.
        /// </summary>
        public static void ReloadConfigs()
        {
            ActionTypeIgnoreMapping =
                ConfigManager.Instance.TryGetConfig("action_type_ignore_mapping", "v.1", _defaultActionTypeIgnoreMapping);
            ActionTypeResponseMapping =
                ConfigManager.Instance.TryGetConfig("action_type_response_mapping", "v.1", _defaultActionTypeResponseMapping);
            SettingValueTypeMap =
                ConfigManager.Instance.TryGetConfig("setting_value_type_map", "v.1", _defaultSettingValueTypeMap);
        }

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
