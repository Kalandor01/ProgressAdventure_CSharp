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
                Key.ESCAPE.ToString().Capitalize(),
                [GetKeyMode.IGNORE_ESCAPE],
                [new((char)ConsoleKey.Escape, ConsoleKey.Escape, false, false, false)]
            ),
            [ActionType.UP] = new(
                Key.UP.ToString(),
                Key.UP.ToString().Capitalize(),
                [GetKeyMode.IGNORE_VERTICAL],
                [new((char)0, ConsoleKey.UpArrow, false, false, false)]
            ),
            [ActionType.DOWN] = new(
                Key.DOWN.ToString(),
                Key.DOWN.ToString().Capitalize(),
                [GetKeyMode.IGNORE_VERTICAL],
                [new((char)0, ConsoleKey.DownArrow, false, false, false)]
            ),
            [ActionType.LEFT] = new(
                Key.LEFT.ToString(),
                Key.LEFT.ToString().Capitalize(),
                [GetKeyMode.IGNORE_HORIZONTAL],
                [new((char)0, ConsoleKey.LeftArrow, false, false, false)]
            ),
            [ActionType.RIGHT] = new(
                Key.RIGHT.ToString(),
                Key.RIGHT.ToString().Capitalize(),
                [GetKeyMode.IGNORE_HORIZONTAL],
                [new((char)0, ConsoleKey.RightArrow, false, false, false)]
            ),
            [ActionType.ENTER] = new(
                Key.ENTER.ToString(),
                Key.ENTER.ToString().Capitalize(),
                [GetKeyMode.IGNORE_ENTER],
                [new((char)ConsoleKey.Enter, ConsoleKey.Enter, false, false, false)]
            ),
            [ActionType.STATS] = new(
                "STATS",
                "Stats",
                [],
                [new('e', ConsoleKey.E, false, false, false)]
            ),
            [ActionType.SAVE] = new(
                "SAVE",
                "Save",
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
        #region Write default config or get reload common data
        private static (string configName, bool paddingData) WriteDefaultConfigOrGetReloadDataActionTypes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "action_types");
            if (!isWriteConfig)
            {
                return (basePath, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultActionTypes
                );
            return default;
        }

        private static (
            string configName,
            Func<EnumValue<ActionType>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataActionTypeAttributes(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "action_type_attributes");
            static string KeySerializer(EnumValue<ActionType> key) => key.Name;
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultActionTypeAttributes,
                    KeySerializer
                );
            return default;
        }

        private static (
            string configName,
            Func<SettingsKey, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataSettingValueTypeMap(bool isWriteConfig)
        {
            var basePath = Path.Join(Constants.CONFIGS_SETTINGS_SUBFOLDER_NAME, "setting_value_type_map");
            static string KeySerializer(SettingsKey key) => key.ToString();
            if (!isWriteConfig)
            {
                return (basePath, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultSettingValueTypeMap,
                    KeySerializer
                );
            return default;
        }
        #endregion

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
            WriteDefaultConfigOrGetReloadDataActionTypes(true);
            WriteDefaultConfigOrGetReloadDataActionTypeAttributes(true);
            WriteDefaultConfigOrGetReloadDataSettingValueTypeMap(true);
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
                WriteDefaultConfigOrGetReloadDataActionTypes(false).configName,
                namespaceFolders,
                _defaultActionTypes,
                isVanillaInvalid,
                showProgressIndentation,
                true
            );

            var actionTypeAttributesData = WriteDefaultConfigOrGetReloadDataActionTypeAttributes(false);
            ActionTypeAttributes = ConfigUtils.ReloadConfigsAggregateDict(
                actionTypeAttributesData.configName,
                namespaceFolders,
                _defaultActionTypeAttributes,
                actionTypeAttributesData.serializeKeys,
                key => ActionType.GetValue(ConfigUtils.GetNamepsacedString(key)),
                isVanillaInvalid,
                showProgressIndentation
            );

            var settingValueTypeMapData = WriteDefaultConfigOrGetReloadDataSettingValueTypeMap(false);
            SettingValueTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                settingValueTypeMapData.configName,
                namespaceFolders,
                _defaultSettingValueTypeMap,
                settingValueTypeMapData.serializeKeys,
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
