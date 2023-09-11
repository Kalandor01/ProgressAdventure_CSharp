using PACommon;
using PACommon.Enums;
using ProgressAdventure.Enums;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.SettingsManagement
{
    /// <summary>
    /// Object for managing the data in the settings file.
    /// </summary>
    public static class Settings
    {
        #region Private Fields
        /// <summary>
        /// If the game should auto save or not.
        /// </summary>
        private static bool _autoSave;
        /// <summary>
        /// The minimum level of logs, that will be recorded.
        /// </summary>
        private static LogSeverity _loggingLevel;
        /// <summary>
        /// The keybinds object to use, for the app.
        /// </summary>
        private static Keybinds _keybinds;
        /// <summary>
        /// If the user should be asked for confirmation, when trying to delete a save file.
        /// </summary>
        private static bool _askDeleteSave;
        /// <summary>
        /// If the user should be asked for confirmation, when trying to regenerate a save file.
        /// </summary>
        private static bool _askRegenerateSave;
        /// <summary>
        /// The default action for backing up save files.<br/>
        /// -1: ask user<br/>
        /// 0: never backup<br/>
        /// 1: always backup
        /// </summary>
        private static int _defBackupAction;
        /// <summary>
        /// Whether to enable colored text on the terminal.
        /// </summary>
        private static bool _enableColoredText;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_autoSave" path="//summary"/>
        /// </summary>
        public static bool AutoSave
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
        public static LogSeverity LoggingLevel
        {
            get => _loggingLevel;
            set
            {
                var loggingLevelValue = (long)value;
                SettingsManager(SettingsKey.LOGGING_LEVEL, loggingLevelValue);
                _loggingLevel = GetLoggingLevel();

                Logger.LoggingLevel = LoggingLevel;
            }
        }
        /// <summary>
        /// <inheritdoc cref="_keybinds" path="//summary"/>
        /// </summary>
        public static Keybinds Keybinds
        {
            get
            {
                _keybinds ??= GetKeybins();
                return _keybinds;
            }
            set
            {
                SettingsManager(SettingsKey.KEYBINDS, value);
                _keybinds = GetKeybins();
            }
        }
        /// <summary>
        /// <inheritdoc cref="_askDeleteSave" path="//summary"/>
        /// </summary>
        public static bool AskDeleteSave
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
        public static bool AskRegenerateSave
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
        public static int DefBackupAction
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
        public static bool EnableColoredText
        {
            get => _enableColoredText;
            set
            {
                SettingsManager(SettingsKey.ENABLE_COLORED_TEXT, value);
                _enableColoredText = GetEnableColoredText();
            }
        }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initialises the values in the object.
        /// </summary>
        /// <param name="autoSave"><inheritdoc cref="_autoSave" path="//summary"/></param>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        /// <param name="keybinds"><inheritdoc cref="_keybinds" path="//summary"/></param>
        /// <param name="askDeleteSave"><inheritdoc cref="_askDeleteSave" path="//summary"/></param>
        /// <param name="askRegenerateSave"><inheritdoc cref="_askRegenerateSave" path="//summary"/></param>
        /// <param name="defBackupAction"><inheritdoc cref="_defBackupAction" path="//summary"/></param>
        public static void Initialise(
            bool? autoSave = null,
            LogSeverity? loggingLevel = null,
            Keybinds? keybinds = null,
            bool? askDeleteSave = null,
            bool? askRegenerateSave = null,
            int? defBackupAction = null,
            bool? enableColoredText = null
        )
        {
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
                !TryGetFromSettingAsType(SettingsKey.LOGGING_LEVEL, out object logLevel) ||
                !Logger.TryParseSeverityValue((int)(long)logLevel, out LogSeverity severity)
                )
            {
                Logger.Log("Settings parse error", $"unknown logging level value: {logLevel}", LogSeverity.WARN);
                _ = Logger.TryParseSeverityValue((int)SettingsUtils.GetDefaultSettings()[SettingsKey.LOGGING_LEVEL.ToString()], out severity);
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
                PACTools.FromJson(keybindsDict as IDictionary<string, object?>, Constants.SAVE_VERSION, out keybinds);
            }
            catch (Exception e)
            {
                Logger.Log("Error while reading keybinds from the settings file", "the keybinds will now be regenerated from the default. Error: " + e.ToString(), LogSeverity.ERROR);
                keybinds = new Keybinds();
                SettingsManager(SettingsKey.KEYBINDS, keybinds.ToJson());
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
        private static Dictionary<string, object> RecreateSettings()
        {
            var newSettings = SettingsUtils.GetDefaultSettings();
            Tools.EncodeSaveShort(newSettings, Path.Join(Constants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), Constants.SETTINGS_SEED);
            // log
            Logger.Log("Recreated settings");
            return newSettings;
        }

        /// <summary>
        /// Returns the contents of the settings file, and recreates it, if it doesn't exist.
        /// </summary>
        private static Dictionary<string, object?> GetSettingsDict()
        {
            Dictionary<string, object?>? settingsJson = null;
            try
            {
                settingsJson = Tools.DecodeSaveShort(Path.Join(Constants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), 0, Constants.SETTINGS_SEED, expected: false);
                if (settingsJson is null)
                {
                    Logger.Log("Decode error", "settings file data is null", LogSeverity.ERROR);
                }
            }
            catch (FormatException)
            {
                Logger.Log("Decode error", "settings", LogSeverity.ERROR);
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
        private static object SettingsManager(SettingsKey settingsKey)
        {
            var settings = GetSettingsDict();
            var settingsKeyName = settingsKey.ToString();
            if (settings.TryGetValue(settingsKeyName, out object? settingValue))
            {
                if (settingValue is not null)
                {
                    return settingValue;
                }
                else
                {
                    Logger.Log("Value is null in settings", settingsKeyName, LogSeverity.WARN);
                }
            }
            else
            {
                Logger.Log("Missing key in settings", settingsKeyName, LogSeverity.WARN);
            }

            var defSettings = SettingsUtils.GetDefaultSettings();
            var defSettingValue = defSettings[settingsKeyName];
            SettingsManager(settingsKey, defSettingValue);
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
            if (settings.TryGetValue(settingsKeyName, out object? settingValue))
            {
                var keybindsEqual = false;
                if (settingsKey == SettingsKey.KEYBINDS && settingValue is not null)
                {
                    Keybinds? oldKb = null;
                    try
                    {
                        PACTools.FromJson(settingValue as IDictionary<string, object?>, Constants.SAVE_VERSION, out oldKb);
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Error while trying to modify the keybinds from the settings file", "Error: " + e.ToString(), LogSeverity.ERROR);
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
                if (!(keybindsEqual || value.Equals(settingValue)))
                {
                    Logger.Log("Changed settings", $"{settingsKey}: {settingValue} -> {value}", LogSeverity.DEBUG);
                    settings[settingsKeyName] = value;
                    Tools.EncodeSaveShort(settings, Path.Join(Constants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), Constants.SETTINGS_SEED);
                }
            }
            else
            {
                Logger.Log("Recreating key in settings", settingsKey.ToString(), LogSeverity.WARN);
                settings[settingsKeyName] = value;
                Tools.EncodeSaveShort(settings, Path.Join(Constants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), Constants.SETTINGS_SEED);
            }
        }

        /// <summary>
        /// Tries to get the value, associated with the key in the settings file, and returns if it's the expected type.
        /// </summary>
        /// <param name="settingsKey">The settings key to get the value from.</param>
        /// <param name="value">The value returned from the settings file.</param>
        private static bool TryGetFromSettingAsType(SettingsKey settingsKey, out object value)
        {
            value = SettingsManager(settingsKey);
            return value.GetType() == SettingsUtils.settingValueTypeMap[settingsKey];
        }

        /// <summary>
        /// Tries to get the value, associated with the key in the settings file, and returns it, or the default value, if it isn't the expected type.
        /// </summary>
        /// <param name="settingsKey">The settings key to get the value from.</param>
        private static object GetFromSettingAsType(SettingsKey settingsKey)
        {
            if (TryGetFromSettingAsType(settingsKey, out object rawValue))
            {
                return rawValue;
            }
            else
            {
                Logger.Log("Settings value type missmatch", $"value at {settingsKey} should be {SettingsUtils.settingValueTypeMap[settingsKey]} but is {rawValue.GetType()}, correcting...", LogSeverity.WARN);
                var newValue = SettingsUtils.GetDefaultSettings()[settingsKey.ToString()];
                SettingsManager(settingsKey, newValue);
                return newValue;
            }
        }
        #endregion
    }
}
