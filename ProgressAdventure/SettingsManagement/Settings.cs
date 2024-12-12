using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using PACommon.Logging;
using ProgressAdventure.Enums;
using PACConstants = PACommon.Constants;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Object for managing the data in the settings file.
    /// </summary>
    public class Settings
    {
        #region Private Fields
        /// <summary>
        /// If the game should auto save.
        /// </summary>
        private bool _autoSave;
        /// <summary>
        /// The minimum level of logs, that will be recorded.
        /// </summary>
        private LogSeverity _loggingLevel;
        /// <summary>
        /// The keybinds object to use, for the app.
        /// </summary>
        private Keybinds _keybinds;
        /// <summary>
        /// If the user should be asked for confirmation, when trying to delete a save file.
        /// </summary>
        private bool _askDeleteSave;
        /// <summary>
        /// If the user should be asked for confirmation, when trying to regenerate a save file.
        /// </summary>
        private bool _askRegenerateSave;
        /// <summary>
        /// The default action for backing up save files.<br/>
        /// -1: ask user<br/>
        /// 0: never backup<br/>
        /// 1: always backup
        /// </summary>
        private int _defBackupAction;
        /// <summary>
        /// Whether to enable colored text on the terminal.
        /// </summary>
        private bool _enableColoredText;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_autoSave" path="//summary"/>
        /// </summary>
        public bool AutoSave
        {
            get => _autoSave;
            set
            {
                SettingsManager(SettingsKey.AUTO_SAVE, value);
                _autoSave = GetAutoSave();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_loggingLevel" path="//summary"/>
        /// </summary>
        public LogSeverity LoggingLevel
        {
            get => _loggingLevel;
            set
            {
                var loggingLevelValue = (long)value;
                SettingsManager(SettingsKey.LOGGING_LEVEL, loggingLevelValue);
                _loggingLevel = GetLoggingLevel();

                PACSingletons.Instance.Logger.LoggingLevel = LoggingLevel;
            }
        }
        /// <summary>
        /// <inheritdoc cref="_keybinds" path="//summary"/>
        /// </summary>
        public Keybinds Keybinds
        {
            get => _keybinds;
            set
            {
                SettingsManager(SettingsKey.KEYBINDS, value);
                _keybinds = GetKeybins();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_askDeleteSave" path="//summary"/>
        /// </summary>
        public bool AskDeleteSave
        {
            get => _askDeleteSave;
            set
            {
                SettingsManager(SettingsKey.ASK_DELETE_SAVE, value);
                _askDeleteSave = GetAskDeleteSave();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_askRegenerateSave" path="//summary"/>
        /// </summary>
        public bool AskRegenerateSave
        {
            get => _askRegenerateSave;
            set
            {
                SettingsManager(SettingsKey.ASK_REGENERATE_SAVE, value);
                _askRegenerateSave = GetAskRegenerateSave();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_defBackupAction" path="//summary"/>
        /// </summary>
        public int DefBackupAction
        {
            get => _defBackupAction;
            set
            {
                SettingsManager(SettingsKey.DEF_BACKUP_ACTION, (long)value);
                _defBackupAction = GetDefBackupAction();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_enableColoredText" path="//summary"/>
        /// </summary>
        public bool EnableColoredText
        {
            get => _enableColoredText;
            set
            {
                SettingsManager(SettingsKey.ENABLE_COLORED_TEXT, value);
                _enableColoredText = GetEnableColoredText();
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Settings"/>
        /// </summary>
        /// <param name="autoSave"><inheritdoc cref="_autoSave" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        /// <param name="keybinds"><inheritdoc cref="_keybinds" path="//summary"/></param>
        /// <param name="askDeleteSave"><inheritdoc cref="_askDeleteSave" path="//summary"/></param>
        /// <param name="askRegenerateSave"><inheritdoc cref="_askRegenerateSave" path="//summary"/></param>
        /// <param name="defBackupAction"><inheritdoc cref="_defBackupAction" path="//summary"/></param>
        public Settings(
            bool? autoSave = null,
            LogSeverity? loggingLevel = null,
            Keybinds? keybinds = null,
            bool? askDeleteSave = null,
            bool? askRegenerateSave = null,
            int? defBackupAction = null,
            bool? enableColoredText = null
        )
        {
            SettingsUtils.LoadDefaultConfigs();
            AutoSave = autoSave ?? GetAutoSave();
            LoggingLevel = loggingLevel ?? GetLoggingLevel();
            Keybinds = keybinds ?? GetKeybins();
            AskDeleteSave = askDeleteSave ?? GetAskDeleteSave();
            AskRegenerateSave = askRegenerateSave ?? GetAskRegenerateSave();
            DefBackupAction = defBackupAction ?? GetDefBackupAction();
            EnableColoredText = enableColoredText ?? GetEnableColoredText();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Returns the value of the <c>autoSave</c> from the setting file.
        /// </summary>
        public static bool GetAutoSave()
        {
            return (bool)GetFromSettingAsType(SettingsKey.AUTO_SAVE);
        }

        /// <summary>
        /// Returns the value of the <c>keybinds</c> from the setting file.
        /// </summary>
        public static LogSeverity GetLoggingLevel()
        {
            if (
                !TryGetFromSettingAsType(SettingsKey.LOGGING_LEVEL, out var logLevel) ||
                !ILogger.TryParseSeverityValue((int)(long)logLevel.Value, out LogSeverity severity)
                )
            {
                PACSingletons.Instance.Logger.Log("Settings parse error", $"unknown logging level value: {logLevel}", LogSeverity.WARN);
                _ = ILogger.TryParseSeverityValue((int)SettingsUtils.GetDefaultSettings()[SettingsKey.LOGGING_LEVEL.ToString()]!.Value, out severity);
            }
            return severity;
        }

        /// <summary>
        /// Returns the value of the <c>keybinds</c> from the setting file.
        /// </summary>
        public static Keybinds GetKeybins()
        {
            var keybindsDict = SettingsManager(SettingsKey.KEYBINDS);
            Keybinds? keybinds;
            try
            {
                PACTools.TryFromJson(keybindsDict as JsonDictionary, Constants.SAVE_VERSION, out keybinds);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("Error while reading keybinds from the settings file", "the keybinds will now be regenerated from the default. Error: " + e.ToString(), LogSeverity.ERROR);
                keybinds = new Keybinds();
                SettingsManager(SettingsKey.KEYBINDS, keybinds);
            }
            return keybinds ?? new Keybinds();
        }

        /// <summary>
        /// Returns the value of the <c>askDeleteSave</c> from the setting file.
        /// </summary>
        public static bool GetAskDeleteSave()
        {
            return (bool)GetFromSettingAsType(SettingsKey.ASK_DELETE_SAVE);
        }

        /// <summary>
        /// Returns the value of the <c>askRegenerateSave</c> from the setting file.
        /// </summary>
        public static bool GetAskRegenerateSave()
        {
            return (bool)GetFromSettingAsType(SettingsKey.ASK_REGENERATE_SAVE);
        }

        /// <summary>
        /// Returns the value of the <c>defBackupAction</c> from the setting file.
        /// </summary>
        public static int GetDefBackupAction()
        {
            return (int)(long)GetFromSettingAsType(SettingsKey.DEF_BACKUP_ACTION);
        }

        /// <summary>
        /// Returns the value of the <c>enableColoredText</c> from the setting file.
        /// </summary>
        public static bool GetEnableColoredText()
        {
            return (bool)GetFromSettingAsType(SettingsKey.ENABLE_COLORED_TEXT);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Recreates the settings file from the default values, and returns the result.
        /// </summary>
        private static JsonDictionary RecreateSettings()
        {
            var newSettings = SettingsUtils.GetDefaultSettings();
            PACTools.SaveJsonFile(newSettings, Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME));
            // log
            PACSingletons.Instance.Logger.Log("Recreated settings");
            return newSettings;
        }

        /// <summary>
        /// Returns the contents of the settings file, and recreates it, if it doesn't exist.
        /// </summary>
        private static JsonDictionary GetSettingsDict()
        {
            JsonDictionary? settingsJson = null;
            try
            {
                settingsJson = PACTools.LoadJsonFile(Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), 0, expected: false);
                if (settingsJson is null)
                {
                    PACSingletons.Instance.Logger.Log("Decode error", "settings file data is null", LogSeverity.ERROR);
                }
            }
            catch (FormatException)
            {
                PACSingletons.Instance.Logger.Log("Decode error", "settings", LogSeverity.ERROR);
                Utils.PressKey("The settings file is corrupted, and will now be recreated!");
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }

            if (settingsJson is not null)
            {
                return settingsJson;
            }
            return RecreateSettings();
        }

        /// <summary>
        /// Reads a value from the settings file.
        /// </summary>
        /// <param name="settingsKey">The settings value to get.</param>
        private static JsonObject SettingsManager(SettingsKey settingsKey)
        {
            var settings = GetSettingsDict();
            var settingsKeyName = settingsKey.ToString();
            if (settings.TryGetValue(settingsKeyName, out var settingValue))
            {
                if (settingValue is not null)
                {
                    return settingValue;
                }
                else
                {
                    PACSingletons.Instance.Logger.Log("Value is null in settings", settingsKeyName, LogSeverity.WARN);
                }
            }
            else
            {
                PACSingletons.Instance.Logger.Log("Missing key in settings", settingsKeyName, LogSeverity.WARN);
            }

            var defSettings = SettingsUtils.GetDefaultSettings();
            var defSettingValue = defSettings[settingsKeyName];
            SettingsManager(settingsKey, defSettingValue?.Value);
            return defSettingValue;
        }

        /// <summary>
        /// Writes a value into the settings file.
        /// </summary>
        /// <param name="settingsKey">The settings value to set.</param>
        /// <param name="value">The value to set the settings value to.</param>
        private static void SettingsManager(SettingsKey settingsKey, object value)
        {
            var settings = GetSettingsDict();
            var settingsKeyName = settingsKey.ToString();
            if (!settings.TryGetValue(settingsKeyName, out var settingValue))
            {
                PACSingletons.Instance.Logger.Log("Recreating key in settings", settingsKey.ToString(), LogSeverity.WARN);
                settings[settingsKeyName] = PACTools.ParseToJsonValue(value);
                PACTools.SaveJsonFile(settings, Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME));
                return;
            }

            var keybindsEqual = false;
            if (settingsKey == SettingsKey.KEYBINDS && settingValue is not null)
            {
                Keybinds? oldKb = null;
                try
                {
                    PACTools.TryFromJson(settingValue as JsonDictionary, Constants.SAVE_VERSION, out oldKb);
                }
                catch (Exception e)
                {
                    PACSingletons.Instance.Logger.Log("Error while trying to modify the keybinds from the settings file", "Error: " + e.ToString(), LogSeverity.ERROR);
                }
                if (oldKb is not null)
                {
                    keybindsEqual = oldKb.Equals(value);
                    if (!keybindsEqual)
                    {
                        value = ((Keybinds)value).ToJson();
                    }
                }
            }

            if (!(keybindsEqual || value.Equals(settingValue?.Value)))
            {
                PACSingletons.Instance.Logger.Log("Changed settings", $"{settingsKey}: {settingValue} -> {value}", LogSeverity.DEBUG);
                settings[settingsKeyName] = PACTools.ParseToJsonValue(value);
                PACTools.SaveJsonFile(settings, Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME));
            }
        }

        /// <summary>
        /// Tries to get the value, associated with the key in the settings file, and returns if it's the expected type.
        /// </summary>
        /// <param name="settingsKey">The settings key to get the value from.</param>
        /// <param name="value">The value returned from the settings file.</param>
        private static bool TryGetFromSettingAsType(SettingsKey settingsKey, out JsonObject value)
        {
            value = SettingsManager(settingsKey);
            return value.Type == SettingsUtils.SettingValueTypeMap[settingsKey];
        }

        /// <summary>
        /// Tries to get the value, associated with the key in the settings file, and returns it, or the default value, if it isn't the expected type.
        /// </summary>
        /// <param name="settingsKey">The settings key to get the value from.</param>
        private static object GetFromSettingAsType(SettingsKey settingsKey)
        {
            if (TryGetFromSettingAsType(settingsKey, out var rawValue))
            {
                return rawValue.Value;
            }
            else
            {
                PACSingletons.Instance.Logger.Log("Settings value type missmatch", $"value at {settingsKey} should be {SettingsUtils.SettingValueTypeMap[settingsKey]} but is {rawValue.Type}, correcting...", LogSeverity.WARN);
                var newValue = SettingsUtils.GetDefaultSettings()[settingsKey.ToString()];
                SettingsManager(settingsKey, newValue!);
                return newValue!.Value;
            }
        }
        #endregion
    }
}
