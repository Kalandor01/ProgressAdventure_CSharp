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
    public class Settings : ISettings
    {
        #region Private Fields
        /// <summary>
        /// If an instance of <see cref="Settings"/> is already initializing.
        /// </summary>
        private static bool _isInitializing;
        
        /// <summary>
        /// The dictionary pairing up settings keys, to the type, that they are expected to be in the settings file.
        /// </summary>
        private static readonly Dictionary<SettingsKey, JsonObjectType> _settingValueTypeMap = new()
        {
            [SettingsKey.AUTO_SAVE] = JsonObjectType.Bool,
            [SettingsKey.LOGGING_LEVEL] = JsonObjectType.WholeNumber,
            [SettingsKey.KEYBINDS] = JsonObjectType.Dictionary,
            [SettingsKey.ASK_DELETE_SAVE] = JsonObjectType.Bool,
            [SettingsKey.ASK_REGENERATE_SAVE] = JsonObjectType.Bool,
            [SettingsKey.DEF_BACKUP_ACTION] = JsonObjectType.WholeNumber,
            [SettingsKey.ENABLE_COLORED_TEXT] = JsonObjectType.Bool,
            [SettingsKey.CURRENT_LANGUAGE] = JsonObjectType.String,
        };

        /// <inheritdoc cref="AutoSave"/>
        private bool _autoSave;
        /// <inheritdoc cref="LoggingLevel"/>
        private LogSeverity _loggingLevel;
        /// <inheritdoc cref="Keybinds"/>
        private Keybinds _keybinds;
        /// <inheritdoc cref="AskDeleteSave"/>
        private bool _askDeleteSave;
        /// <inheritdoc cref="AskRegenerateSave"/>
        private bool _askRegenerateSave;
        /// <inheritdoc cref="DefBackupAction"/>
        private int _defBackupAction;
        /// <inheritdoc cref="EnableColoredText"/>
        private bool _enableColoredText;
        /// <inheritdoc cref="CurrentLanguage"/>
        private EnumValue<Language>? _currentLanguage;
        #endregion

        #region Public properties
        public bool AutoSave
        {
            get => _autoSave;
            set
            {
                SettingsManager(SettingsKey.AUTO_SAVE, value);
                _autoSave = GetAutoSave();
            }
        }

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

        public Keybinds Keybinds
        {
            get => _keybinds;
            set
            {
                SettingsManager(SettingsKey.KEYBINDS, value);
                _keybinds = GetKeybins();
                var kbCount = _keybinds.KeybindList.Count();
                if (kbCount is > 0 and < 6)
                {
                    PACSingletons.Instance.Logger.Log(
                        "Too few keybinds",
                        $"there are less than 6 keybinds ({kbCount}), so if {nameof(ConsoleUI.UIList)} or {nameof(ConsoleUI.OptionsUI)} is called, these keybinds will be ignored.",
                        LogSeverity.WARN
                    );
                }
            }
        }

        public bool AskDeleteSave
        {
            get => _askDeleteSave;
            set
            {
                SettingsManager(SettingsKey.ASK_DELETE_SAVE, value);
                _askDeleteSave = GetAskDeleteSave();
            }
        }

        public bool AskRegenerateSave
        {
            get => _askRegenerateSave;
            set
            {
                SettingsManager(SettingsKey.ASK_REGENERATE_SAVE, value);
                _askRegenerateSave = GetAskRegenerateSave();
            }
        }

        public int DefBackupAction
        {
            get => _defBackupAction;
            set
            {
                SettingsManager(SettingsKey.DEF_BACKUP_ACTION, (long)value);
                _defBackupAction = GetDefBackupAction();
            }
        }

        public bool EnableColoredText
        {
            get => _enableColoredText;
            set
            {
                SettingsManager(SettingsKey.ENABLE_COLORED_TEXT, value);
                _enableColoredText = GetEnableColoredText();
            }
        }

        public EnumValue<Language> CurrentLanguage
        {
            get
            {
                if (_currentLanguage is null)
                {
                    return PASingletons.Instance.Localizer.CurrentLanguage;
                }

                PASingletons.Instance.Localizer.CurrentLanguage = _currentLanguage;
                _currentLanguage = null;
                return PASingletons.Instance.Localizer.CurrentLanguage;
            }
            set
            {
                SettingsManager(SettingsKey.CURRENT_LANGUAGE, value.Name);
                PASingletons.Instance.Localizer.CurrentLanguage = GetCurrentLanguage();
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
        /// <param name="enableColoredText"><inheritdoc cref="_enableColoredText" path="//summary"/></param>
        /// <param name="currentLanguage"><inheritdoc cref="Localization.Localizer.CurrentLanguage" path="//summary"/></param>
        /// <param name="dontUpdateSettingsIfValueSet">If this value is true, if a settings value is set in this constructor, it won't update the settings file.</param>
        /// <param name="isInitializing">If this constructor was called as part of the <see cref="PASingletons"/> initializer.</param>
        public Settings(
            bool? autoSave = null,
            LogSeverity? loggingLevel = null,
            Keybinds? keybinds = null,
            bool? askDeleteSave = null,
            bool? askRegenerateSave = null,
            int? defBackupAction = null,
            bool? enableColoredText = null,
            EnumValue<Language>? currentLanguage = null,
            bool dontUpdateSettingsIfValueSet = false,
            bool isInitializing = false
        )
        {
            UpdateOrNotHelper((newValue) => AutoSave = newValue, ref _autoSave, autoSave, GetAutoSave, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => LoggingLevel = newValue, ref _loggingLevel, loggingLevel, GetLoggingLevel, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => Keybinds = newValue, ref _keybinds!, keybinds, GetKeybins, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => AskDeleteSave = newValue, ref _askDeleteSave, askDeleteSave, GetAskDeleteSave, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => AskRegenerateSave = newValue, ref _askRegenerateSave, askRegenerateSave, GetAskRegenerateSave, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => DefBackupAction = newValue, ref _defBackupAction, defBackupAction, GetDefBackupAction, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => EnableColoredText = newValue, ref _enableColoredText, enableColoredText, GetEnableColoredText, dontUpdateSettingsIfValueSet);
            UpdateOrNotHelper((newValue) => EnableColoredText = newValue, ref _enableColoredText, enableColoredText, GetEnableColoredText, dontUpdateSettingsIfValueSet);
            InitializeCurrentLanguageValue(currentLanguage, dontUpdateSettingsIfValueSet, isInitializing);
        }
        #endregion

        #region Public functions
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool GetAutoSave()
        {
            return (bool)GetFromSettingAsType(SettingsKey.AUTO_SAVE);
        }

        public LogSeverity GetLoggingLevel()
        {
            if (
                !TryGetFromSettingAsType(SettingsKey.LOGGING_LEVEL, out var logLevel) ||
                !ILogger.TryParseSeverityValue((int)(long)logLevel.Value, out var severity)
                )
            {
                PACSingletons.Instance.Logger.Log("Settings parse error", $"unknown logging level value: {logLevel}", LogSeverity.WARN);
                _ = ILogger.TryParseSeverityValue((int)SettingsUtils.GetDefaultSettings()[nameof(SettingsKey.LOGGING_LEVEL)]!.Value, out severity);
            }
            return severity;
        }

        public Keybinds GetKeybins()
        {
            var keybindsDict = SettingsManager(SettingsKey.KEYBINDS);
            Keybinds? keybinds = null;
            string? errorMessage = null;
            try
            {
                PACTools.TryFromJson(keybindsDict as JsonDictionary, Constants.SAVE_VERSION, out keybinds);
            }
            catch (Exception e)
            {
                errorMessage = e.ToString();
            }
            if (keybinds is null)
            {
                errorMessage = "The keybinds could not be parsed.";
            }

            if (errorMessage is not null)
            {
                PACSingletons.Instance.Logger.Log("Error while reading keybinds from the settings file", "the keybinds will now be regenerated from the default. Error: " + errorMessage, LogSeverity.ERROR);
                keybinds = new Keybinds();
                SettingsManager(SettingsKey.KEYBINDS, keybinds);
            }
            return keybinds!;
        }

        public bool GetAskDeleteSave()
        {
            return (bool)GetFromSettingAsType(SettingsKey.ASK_DELETE_SAVE);
        }

        public bool GetAskRegenerateSave()
        {
            return (bool)GetFromSettingAsType(SettingsKey.ASK_REGENERATE_SAVE);
        }

        public int GetDefBackupAction()
        {
            return (int)(long)GetFromSettingAsType(SettingsKey.DEF_BACKUP_ACTION);
        }

        public bool GetEnableColoredText()
        {
            return (bool)GetFromSettingAsType(SettingsKey.ENABLE_COLORED_TEXT);
        }

        public EnumValue<Language> GetCurrentLanguage()
        {
            var languageStr = (string)GetFromSettingAsType(SettingsKey.CURRENT_LANGUAGE);
            if (!Language.TryGetValue(languageStr, out var language))
            {
                PACSingletons.Instance.Logger.Log(
                    "Invalid language type in the settings file",
                    "the language will now be set back to the default",
                    LogSeverity.ERROR
                );
                language = Language.ENGLISH;
                SettingsManager(SettingsKey.CURRENT_LANGUAGE, language);
            }
            return language;
        }
        #endregion
        
        #region Private functions
        private static void UpdateOrNotHelper<T>(
            Action<T> setPropertyValue,
            ref T value,
            object? newValue,
            Func<T> getValueFromSettings,
            bool noUpdateIfNotNull
        )
        {
            if (noUpdateIfNotNull && newValue is not null)
            {
                value = (T)newValue;
                return;
            }
            setPropertyValue(getValueFromSettings());
        }

        /// <summary>
        /// Initializes the <see cref="CurrentLanguage"/> value in a way that minimizes the chance and amount of times that the <see cref="Settings"/> construcor gets recursively called.
        /// </summary>
        /// <param name="value">The new value of the <see cref="CurrentLanguage"/>.</param>
        /// <param name="dontUpdateSettingsIfValueSet">If this value is true, if a settings value is set in the constructor, it won't update the settings file.</param>
        /// <param name="isInitializing">If the constructor was called as part of the <see cref="PASingletons"/> initializer.</param>
        private void InitializeCurrentLanguageValue(EnumValue<Language>? value, bool dontUpdateSettingsIfValueSet, bool isInitializing)
        {
            if (dontUpdateSettingsIfValueSet && value is not null)
            {
                _currentLanguage = value;
                return;
            }

            var languageFromSettings = GetCurrentLanguage();
            if (_isInitializing || isInitializing)
            {
                _currentLanguage = languageFromSettings;
                return;
            }

            _isInitializing = true;
            CurrentLanguage = languageFromSettings;
            _isInitializing = false;
        }

        /// <summary>
        /// Recreates the settings file from the default values, and returns the result.
        /// </summary>
        private static JsonDictionary RecreateSettings()
        {
            var newSettings = SettingsUtils.GetDefaultSettings();
            PACTools.SaveJsonFile(newSettings, Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), format: true);
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
                settingsJson = PACTools.LoadJsonFile(Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), null, expected: false);
                if (settingsJson is null)
                {
                    PACSingletons.Instance.Logger.Log("Decode error", "settings file data is null", LogSeverity.ERROR);
                }
            }
            catch (FormatException)
            {
                PACSingletons.Instance.Logger.Log("Decode error", "settings", LogSeverity.ERROR);
                PACSingletons.Instance.ConsoleProxy.PressKey("The settings file is corrupted, and will now be recreated!");
            }

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
                PACTools.SaveJsonFile(settings, Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), format: true);
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
                else
                {
                    value = ((Keybinds)value).ToJson();
                }
            }

            if (!(keybindsEqual || value.Equals(settingValue?.Value)))
            {
                PACSingletons.Instance.Logger.Log("Changed settings", $"{settingsKey}: {settingValue} -> {value}", LogSeverity.DEBUG);
                settings[settingsKeyName] = PACTools.ParseToJsonValue(value);
                PACTools.SaveJsonFile(settings, Path.Join(PACConstants.ROOT_FOLDER, Constants.SETTINGS_FILE_NAME), format: true);
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
            return value.Type == _settingValueTypeMap[settingsKey];
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
                PACSingletons.Instance.Logger.Log("Settings value type missmatch", $"value at {settingsKey} should be {_settingValueTypeMap[settingsKey]} but is {rawValue.Type}, correcting...", LogSeverity.WARN);
                var newValue = SettingsUtils.GetDefaultSettings()[settingsKey.ToString()];
                SettingsManager(settingsKey, newValue!);
                return newValue!.Value;
            }
        }
        #endregion
    }
}
