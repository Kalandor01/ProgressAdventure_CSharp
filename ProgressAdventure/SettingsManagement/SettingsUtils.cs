using ConsoleUI.Keybinds;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;

namespace ProgressAdventure.SettingsManagement
{
    public static class SettingsUtils
    {
        #region Default config values

        /// <summary>
        /// The default value for the config used for the values of <see cref="ActionType"/>.
        /// </summary>
        private static readonly List<EnumValue<ActionType>> _defaultActionTypes =
        [
            ActionType.ESCAPE,
            ActionType.UP,
            ActionType.DOWN,
            ActionType.LEFT,
            ActionType.RIGHT,
            ActionType.ENTER,
            ActionType.STATS,
            ActionType.SAVE,
        ];

        /// <summary>
        /// The default value for the config used for the value of <see cref="ActionTypeAttributes"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<ActionType>, ActionTypeAttributesDTO> _defaultActionTypeAttributes = new()
        {
            [ActionType.ESCAPE] = new(
                Key.ESCAPE.ToString(),
                [GetKeyMode.IGNORE_ESCAPE],
                [new((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)]
            ),
            [ActionType.UP] = new(
                Key.UP.ToString(),
                [GetKeyMode.IGNORE_VERTICAL],
                [new((char)0, ConsoleKey.UpArrow, false, false, false)]
            ),
            [ActionType.DOWN] = new(
                Key.DOWN.ToString(),
                [GetKeyMode.IGNORE_VERTICAL],
                [new((char)0, ConsoleKey.DownArrow, false, false, false)]
            ),
            [ActionType.LEFT] = new(
                Key.LEFT.ToString(),
                [GetKeyMode.IGNORE_HORIZONTAL],
                [new((char)0, ConsoleKey.LeftArrow, false, false, false)]
            ),
            [ActionType.RIGHT] = new(
                Key.RIGHT.ToString(),
                [GetKeyMode.IGNORE_HORIZONTAL],
                [new((char)0, ConsoleKey.RightArrow, false, false, false)]
            ),
            [ActionType.ENTER] = new(
                Key.ENTER.ToString(),
                [GetKeyMode.IGNORE_ENTER],
                [new((char)ConsoleKey.Enter, ConsoleKey.Enter, false, false, false)]
            ),
            [ActionType.STATS] = new(
                "STATS",
                [],
                [new('e', ConsoleKey.E, false, false, false)]
            ),
            [ActionType.SAVE] = new(
                "SAVE",
                [],
                [new('s', ConsoleKey.S, false, false, false)]
            ),
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
        /// The dictionary pairing up action types, to their attributes.
        /// </summary>
        public static Dictionary<EnumValue<ActionType>, ActionTypeAttributesDTO> ActionTypeAttributes { get; private set; }

        /// <summary>
        /// The dictionary pairing up settings keys, to the type, that they are expected to be in the settings file.
        /// </summary>
        public static Dictionary<SettingsKey, JsonObjectType> SettingValueTypeMap { get; private set; }
        #endregion

        #region Constructors
        static SettingsUtils()
        {
            LoadDefaultConfigs();
        }
        #endregion

        #region Public functions
        #region Configs
        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            Tools.LoadDefultAdvancedEnum(_defaultActionTypes);
            ActionTypeAttributes = _defaultActionTypeAttributes;
            SettingValueTypeMap = _defaultSettingValueTypeMap;
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "action_types"),
                null,
                _defaultActionTypes
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "action_type_attributes"),
                null,
                _defaultActionTypeAttributes,
                (actionType) => actionType.Name
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "setting_value_type_map"),
                null,
                _defaultSettingValueTypeMap
            );
        }

        /// <summary>
        /// Reloads all values that come from configs.
        /// </summary>
        /// <param name="namespaceFolders">The name of the currently active config folders.</param>
        /// <param name="isVanillaInvalid">If the vanilla config is valid.</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public static void ReloadConfigs(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation = null
        )
        {
            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                Path.Join(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "action_types"),
                namespaceFolders,
                _defaultActionTypes,
                isVanillaInvalid,
                showProgressIndentation,
                true
            );

            ActionTypeAttributes = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "action_type_attributes"),
                namespaceFolders,
                _defaultActionTypeAttributes,
                (actionType) => actionType.Name,
                key => ActionType.GetValue(ConfigUtils.GetNamepsacedString(key)),
                isVanillaInvalid,
                showProgressIndentation
            );

            SettingValueTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "setting_value_type_map"),
                namespaceFolders,
                _defaultSettingValueTypeMap,
                key => key.ToString(),
                Enum.Parse<SettingsKey>,
                isVanillaInvalid,
                showProgressIndentation
            );
        }
        #endregion

        /// <summary>
        /// Returns the default keybind list, for a Keybinds object.
        /// </summary>
        public static List<ActionKey> GetDefaultKeybindList()
        {
            return ActionTypeAttributes.Select(action => new ActionKey(action.Key, action.Value.defaultKeys)).ToList().DeepCopy();
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
