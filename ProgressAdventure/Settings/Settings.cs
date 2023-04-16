using ProgressAdventure.Enums;

namespace ProgressAdventure.Settings
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
        /// Indicates the level of logs, that will be recorded.<br/>
        /// The higher the number, generaly the higher the log severity needs to be, to be writen to file.<br/>
        /// -1 to turn logging off.
        /// </summary>
        private static int _loggingLevel;
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
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_autoSave" path="//summary"/>
        /// </summary>
        public static bool AutoSave
        {
            get => _autoSave;
            set => UpdateAutoSave(value);
        }
        /// <summary>
        /// <inheritdoc cref="_loggingLevel" path="//summary"/>
        /// </summary>
        public static int LoggingLevel
        {
            get => _loggingLevel;
            set => UpdateLoggingLevel(value);
        }
        /// <summary>
        /// <inheritdoc cref="_keybinds" path="//summary"/>
        /// </summary>
        public static Keybinds Keybinds
        {
            get => _keybinds ?? GetKeybins();
            set => UpdateKeybinds(value);
        }
        /// <summary>
        /// <inheritdoc cref="_askDeleteSave" path="//summary"/>
        /// </summary>
        public static bool AskDeleteSave
        {
            get => _askDeleteSave;
            set => UpdateAskDeleteSave(value);
        }
        /// <summary>
        /// <inheritdoc cref="_askRegenerateSave" path="//summary"/>
        /// </summary>
        public static bool AskRegenerateSave
        {
            get => _askRegenerateSave;
            set => UpdateAskRegenerateSave(value);
        }
        /// <summary>
        /// <inheritdoc cref="_defBackupAction" path="//summary"/>
        /// </summary>
        public static int DefBackupAction
        {
            get => _defBackupAction;
            set => UpdateDefBackupAction(value);
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
            int? loggingLevel = null,
            Keybinds? keybinds = null,
            bool? askDeleteSave = null,
            bool? askRegenerateSave = null,
            int? defBackupAction = null
        )
        {
            AutoSave = autoSave ?? GetAutoSave();
            LoggingLevel = loggingLevel ?? GetLoggingLevel();
            Keybinds = keybinds ?? GetKeybins();
            AskDeleteSave = askDeleteSave ?? GetAskDeleteSave();
            AskRegenerateSave = askRegenerateSave ?? GetAskRegenerateSave();
            DefBackupAction = defBackupAction ?? GetDefBackupAction();
        }
        #endregion

        #region Public functions     
        #region Getters
        /// <summary>
        /// Returns the value of the <c>autoSave</c> from the setting file.
        /// </summary>
        public static bool GetAutoSave()
        {
            return (bool)SettingsManager(SettingsKey.AUTO_SAVE);
        }

        /// <summary>
        /// Returns the value of the <c>keybinds</c> from the setting file.
        /// </summary>
        public static int GetLoggingLevel()
        {
            return (int)(long)SettingsManager(SettingsKey.LOGGING_LEVEL);
        }

        /// <summary>
        /// Returns the value of the <c>keybinds</c> from the setting file.
        /// </summary>
        public static Keybinds GetKeybins()
        {
            var keybinds = SettingsManager(SettingsKey.KEYBINDS);
            return Keybinds.KeybindsFromJson((IDictionary<string, object>)keybinds);
        }

        /// <summary>
        /// Returns the value of the <c>askDeleteSave</c> from the setting file.
        /// </summary>
        public static bool GetAskDeleteSave()
        {
            return (bool)SettingsManager(SettingsKey.ASK_DELETE_SAVE);
        }

        /// <summary>
        /// Returns the value of the <c>askRegenerateSave</c> from the setting file.
        /// </summary>
        public static bool GetAskRegenerateSave()
        {
            return (bool)SettingsManager(SettingsKey.ASK_REGENERATE_SAVE);
        }

        /// <summary>
        /// Returns the value of the <c>defBackupAction</c> from the setting file.
        /// </summary>
        public static int GetDefBackupAction()
        {
            return (int)(long)SettingsManager(SettingsKey.DEF_BACKUP_ACTION);
        }
        #endregion

        #region Setters
        /// <summary>
        /// Updates the value of the <c>autoSave</c> in the object and in the settings file.
        /// </summary>
        /// <param name="autoSave"><inheritdoc cref="_autoSave" path="//summary"/></param>
        public static void UpdateAutoSave(bool autoSave)
        {
            SettingsManager(SettingsKey.AUTO_SAVE, autoSave);
            _autoSave = GetAutoSave();
        }

        /// <summary>
        /// Updates the value of the <c>loggingLevel</c> in the object and in the settings file.
        /// </summary>
        /// <param name="loggingLevel"><inheritdoc cref="_loggingLevel" path="//summary"/></param>
        public static void UpdateLoggingLevel(int loggingLevel)
        {
            SettingsManager(SettingsKey.LOGGING_LEVEL, (long)loggingLevel);
            _loggingLevel = GetLoggingLevel();

            Logger.ChangeLoggingLevel(_loggingLevel);
        }

        /// <summary>
        /// Updates the value of the <c>keybinds</c> in the object and in the settings file.
        /// </summary>
        /// <param name="keybinds"><inheritdoc cref="_keybinds" path="//summary"/></param>
        public static void UpdateKeybinds(Keybinds keybinds)
        {
            SettingsManager(SettingsKey.KEYBINDS, keybinds.ToJson());
            _keybinds = GetKeybins();
        }

        /// <summary>
        /// Updates the value of <c>askDeleteSave</c> in the object and in the settings file.
        /// </summary>
        /// <param name="askDeleteSave"><inheritdoc cref="_askDeleteSave" path="//summary"/></param>
        public static void UpdateAskDeleteSave(bool askDeleteSave)
        {
            SettingsManager(SettingsKey.ASK_DELETE_SAVE, askDeleteSave);
            _askDeleteSave = GetAskDeleteSave();
        }

        /// <summary>
        /// Updates the value of <c>askRegenerateSave</c> in the object and in the settings file.
        /// </summary>
        /// <param name="askRegenerateSave"><inheritdoc cref="_askRegenerateSave" path="//summary"/></param>
        public static void UpdateAskRegenerateSave(bool askRegenerateSave)
        {
            SettingsManager(SettingsKey.ASK_REGENERATE_SAVE, askRegenerateSave);
            _askRegenerateSave = GetAskRegenerateSave();
        }

        /// <summary>
        /// Updates the value of <c>defBackupAction</c> in the object and in the settings file.
        /// </summary>
        /// <param name="defBackupAction"><inheritdoc cref="_defBackupAction" path="//summary"/></param>
        public static void UpdateDefBackupAction(int defBackupAction)
        {
            SettingsManager(SettingsKey.DEF_BACKUP_ACTION, (long)defBackupAction);
            _defBackupAction = GetDefBackupAction();
        }
        #endregion
        #endregion

        #region Private functions
        /// <summary>
        /// Recreates the settings file from the default values, and returns the result.
        /// </summary>
        private static Dictionary<string, object?> RecreateSettings()
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
            try
            {
                return Tools.DecodeSaveShort(Path.Join(Constants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), 0, Constants.SETTINGS_SEED);
            }
            catch (FormatException)
            {
                Logger.Log("Decode error", "settings", LogSeverity.ERROR);
                Utils.PressKey("The settings file is corrupted, and will now be recreated!");
                return RecreateSettings();
            }
            catch (FileNotFoundException)
            {
                return RecreateSettings();
            }
        }

        /// <summary>
        /// Reads a value from the settings file.
        /// </summary>
        /// <param name="settingsKey">The settings value to get.</param>
        private static object SettingsManager(SettingsKey settingsKey)
        {
            var settings = GetSettingsDict();
            var settingsKeyName = SettingsUtils.settingsKeyNames[settingsKey];
            if (settings.ContainsKey(settingsKeyName))
            {
                return settings[settingsKeyName];
            }
            else
            {
                Logger.Log("Missing key in settings", settingsKeyName, LogSeverity.WARN);
                var defSettings = SettingsUtils.GetDefaultSettings();
                SettingsManager(settingsKey, defSettings[settingsKeyName]);
                return defSettings[settingsKeyName];
            }
        }

        /// <summary>
        /// Writes a value into the settings file.
        /// </summary>
        /// <param name="settingsKey">The settings value to set.</param>
        /// <param name="value">The value to set the settings value to.</param>
        private static void SettingsManager(SettingsKey settingsKey, object value)
        {
            var settings = GetSettingsDict();
            var settingsKeyName = SettingsUtils.settingsKeyNames[settingsKey];
            if (settings.ContainsKey(settingsKeyName))
            {
                if (!value.Equals(settings[settingsKeyName]))
                {
                    Logger.Log("Changed settings", $"{settingsKey}: {settings[settingsKeyName]} -> {value}", LogSeverity.DEBUG);
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
        #endregion
    }
}
