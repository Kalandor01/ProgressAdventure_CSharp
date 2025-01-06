﻿using ConsoleUI.Keybinds;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;

namespace ProgressAdventure.SettingsManagement
{
    public static class SettingsUtils
    {
        #region Default config dicts
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
            ActionTypeAttributes = _defaultActionTypeAttributes;
            SettingValueTypeMap = _defaultSettingValueTypeMap;
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            PACSingletons.Instance.ConfigManager.SetConfig("action_type_attributes", "v.1", _defaultActionTypeAttributes);
            PACSingletons.Instance.ConfigManager.SetConfig("setting_value_type_map", "v.1", _defaultSettingValueTypeMap);
        }

        /// <summary>
        /// Reloads all variables that come from configs.
        /// </summary>
        public static void ReloadConfigs()
        {
            ActionTypeAttributes =
                PACSingletons.Instance.ConfigManager.TryGetConfig(
                    "action_type_attributes",
                    "v.1",
                    _defaultActionTypeAttributes,
                    (actionType) => actionType.Name,
                    ActionType.GetValue
                );

            SettingValueTypeMap =
                PACSingletons.Instance.ConfigManager.TryGetConfig("setting_value_type_map", "v.1", _defaultSettingValueTypeMap);
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
