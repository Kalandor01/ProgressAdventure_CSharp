using System.Diagnostics.CodeAnalysis;
using PACommon;
using PACommon.Enums;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;

namespace ProgressAdventure.Localization
{
    /// <summary>
    /// Class used to localize text.
    /// </summary>
    public class Localizer : ALocalizer<Language, LocalizationKey>, IDisposable
    {
        #region Constants
        /// <summary>
        /// The default language for P.A.
        /// </summary>
        public static readonly EnumValue<Language> DEFAULT_LANGUAGE = Language.ENGLISH;
        #endregion
        
        #region Default config values
        /// <summary>
        /// The default value for the config used for the values of <see cref="Language"/>.
        /// </summary>
        private static readonly List<EnumValue<Language>> _defaultLanguages =
        [
            Language.ENGLISH,
        ];

        /// <summary>
        /// The default value for the config used for the values of <see cref="LanguageProperties"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<Language>, LanguageProperties> _defaultLanguageProperties = new()
        {
            [Language.ENGLISH] = new LanguageProperties("English"),
        };
        
        /// <summary>
        /// The default value for the config used for the values of <see cref="LocalizationKey"/>.
        /// </summary>
        private static readonly List<EnumValue<LocalizationKey>> _defaultLocalizationKeys =
        [
            #region Main/Initialization
            LocalizationKey.LANGUAGE_NAME_ENGLISH_0,
            LocalizationKey.APPLICATION_TITLE_0,
            LocalizationKey.LOADING_0,
            LocalizationKey.LOADING_COMMON_SINGLETONS_0,
            LocalizationKey.ACTIVATING_ANSI_0,
            LocalizationKey.LOADING_PA_SINGLETONS_0,
            LocalizationKey.RELOADING_CONFIGS_0,
            LocalizationKey.DONE_0,
            LocalizationKey.LOADING_FROM_FOLDER_1,
            LocalizationKey.LOADING_FILE_FROM_CONFIG_1,
            LocalizationKey.FAILED_0,
            LocalizationKey.RESTART_0,
            LocalizationKey.RESTART_IN_SAFE_MODE_0,
            LocalizationKey.EXIT_0,
            LocalizationKey.ERROR_COLON_0,
            #endregion
            
            #region Keybinds
            LocalizationKey.ESCAPE_0,
            LocalizationKey.UP_0,
            LocalizationKey.DOWN_0,
            LocalizationKey.LEFT_0,
            LocalizationKey.RIGHT_0,
            LocalizationKey.ENTER_0,
            LocalizationKey.STATS_0,
            LocalizationKey.SAVE_0,
            #endregion
            
            #region Items
            LocalizationKey.COMPOUND_ITEM_DISPLAY_NAME_CLUB_WITH_TEETH_0,
            LocalizationKey.COMPOUND_ITEM_DISPLAY_NAME_PIECE_0,
            LocalizationKey.COMPOUND_ITEM_DISPLAY_NAME_BOTTLE_0,
            #endregion
            
            #region Entities
            LocalizationKey.ENTITY_DISPLAY_NAME_PLAYER_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_DEMON_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_DWARF_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_ELF_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_HUMAN_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_CAVEMAN_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_GHOUL_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_TROLL_0,
            LocalizationKey.ENTITY_DISPLAY_NAME_DRAGON_0,
            #endregion
        ];

        /// <summary>
        /// The default value for the config used for the value of the texts in <see cref="LocalizationDictionary"/> for <see cref="Language.ENGLISH"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<LocalizationKey>, string> _defaultTextsEnglish = new()
        {
            #region Main/Initialization
            [LocalizationKey.LANGUAGE_NAME_ENGLISH_0] = "English",
            [LocalizationKey.APPLICATION_TITLE_0] = "Progress Adventure",
            [LocalizationKey.LOADING_0] = "Loading...",
            [LocalizationKey.LOADING_COMMON_SINGLETONS_0] = "Loading common singletons...",
            [LocalizationKey.ACTIVATING_ANSI_0] = "Activating Ansi escape codes (Windows only)...",
            [LocalizationKey.LOADING_PA_SINGLETONS_0] = "Loading P.A. singletons...",
            [LocalizationKey.RELOADING_CONFIGS_0] = "Reloading configs...",
            [LocalizationKey.DONE_0] = "DONE!",
            [LocalizationKey.LOADING_FROM_FOLDER_1] = "Loading from folder \"{0}\":",
            [LocalizationKey.LOADING_FILE_FROM_CONFIG_1] = "Loading file \"{0}\" from config:",
            [LocalizationKey.FAILED_0] = "FAILED!",
            [LocalizationKey.RESTART_0] = "Restart",
            [LocalizationKey.RESTART_IN_SAFE_MODE_0] = "Restart in safe mode (only vanilla config enabled)",
            [LocalizationKey.EXIT_0] = "Exit",
            [LocalizationKey.ERROR_COLON_0] = "ERROR: ",
            #endregion
            
            #region Keybinds
            [LocalizationKey.ESCAPE_0] = "Escape",
            [LocalizationKey.UP_0] = "Up",
            [LocalizationKey.DOWN_0] = "Down",
            [LocalizationKey.LEFT_0] = "Left",
            [LocalizationKey.RIGHT_0] = "Right",
            [LocalizationKey.ENTER_0] = "Enter",
            [LocalizationKey.STATS_0] = "Stats",
            [LocalizationKey.SAVE_0] = "Save",
            #endregion
            
            #region Items
            [LocalizationKey.COMPOUND_ITEM_DISPLAY_NAME_CLUB_WITH_TEETH_0] = "*/0MC/* club with */1ML/*",
            [LocalizationKey.COMPOUND_ITEM_DISPLAY_NAME_PIECE_0] = "*/0MC/*",
            [LocalizationKey.COMPOUND_ITEM_DISPLAY_NAME_BOTTLE_0] = "*/0MC/* bottle of */1MC/*",
            #endregion
            
            #region Entities
            [LocalizationKey.ENTITY_DISPLAY_NAME_PLAYER_0] = "You",
            [LocalizationKey.ENTITY_DISPLAY_NAME_DEMON_0] = "Demon",
            [LocalizationKey.ENTITY_DISPLAY_NAME_DWARF_0] = "Dwarf",
            [LocalizationKey.ENTITY_DISPLAY_NAME_ELF_0] = "Elf",
            [LocalizationKey.ENTITY_DISPLAY_NAME_HUMAN_0] = "Human",
            [LocalizationKey.ENTITY_DISPLAY_NAME_CAVEMAN_0] = "Caveman",
            [LocalizationKey.ENTITY_DISPLAY_NAME_GHOUL_0] = "Ghoul",
            [LocalizationKey.ENTITY_DISPLAY_NAME_TROLL_0] = "Troll",
            [LocalizationKey.ENTITY_DISPLAY_NAME_DRAGON_0] = "Dragon",
            #endregion
        };
        #endregion
        
        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static Localizer? _instance = null;
        #endregion

        #region Private properties
        /// <summary>
        /// The properties of all languages.
        /// </summary>
        public Dictionary<EnumValue<Language>, LanguageProperties> LanguageProperties { get; private set; }
        #endregion

        #region Protected properties
        protected override Dictionary<EnumValue<Language>, Dictionary<EnumValue<LocalizationKey>, string>> LocalizationDictionary { get; set; }
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static Localizer Instance
        {
            get
            {
                _instance ??= Initialize(DEFAULT_LANGUAGE, onlyIfUninitialized: true);
                return _instance;
            }
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Localizer" path="//summary"/>
        /// </summary>
        /// <param name="defaultLanguage"><inheritdoc cref="ALocalizer{TL,TT}._defaultLanguage" path="//summary"/></param>
        private Localizer(EnumValue<Language> defaultLanguage)
            :base(defaultLanguage)
        {
            LoadDefaultConfigs();
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="defaultLanguage"><inheritdoc cref="ALocalizer{TL,TT}._defaultLanguage" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        /// <param name="onlyIfUninitialized">If true, only initializes the singleton if it hasn't been initialized yet.</param>
        public static Localizer Initialize(
            EnumValue<Language> defaultLanguage,
            bool logInitialization = true,
            bool onlyIfUninitialized = false
        )
        {
            lock (_threadLock)
            {
                if (onlyIfUninitialized && _instance is not null)
                {
                    return _instance;
                }

                _instance?.Dispose();
                _instance = new Localizer(defaultLanguage);
                if (logInitialization)
                {
                    PACSingletons.Instance.Logger.Log($"{nameof(Localizer)} initialized");
                }
                return _instance;
            }
        }
        #endregion

        #region Public methods
        #region Configs
        #region Write default config or get reload common data
        private static (string configName, string? comment, bool paddingData) WriteDefaultConfigOrGetReloadDataLanguages(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_LOCALIZATION_SUBFOLDER_NAME, "languages");
            if (!isWriteConfig)
            {
                return (basePath, comment, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultLanguages,
                    comment
                );
            return default;
        }
        
        private static (string configName, string? comment, bool paddingData) WriteDefaultConfigOrGetReloadDataLocalizationKeys(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_LOCALIZATION_SUBFOLDER_NAME, "localization_keys");
            if (!isWriteConfig)
            {
                return (basePath, comment, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                null,
                _defaultLocalizationKeys,
                comment
            );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<Language>, string> serializeKeys
            ) WriteDefaultConfigOrGetReloadDataLanguageProperties(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_LOCALIZATION_SUBFOLDER_NAME, "language_properties");
            static string KeySerializer(EnumValue<Language> key) => key.Name;
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                null,
                _defaultLanguageProperties,
                KeySerializer,
                comment
            );
            return default;
        }

        private static string GetTextsDictConfigPath(EnumValue<Language> language)
        {
            return Path.Join(
                Constants.CONFIGS_LOCALIZATION_SUBFOLDER_NAME,
                Constants.CONFIGS_LANGUAGES_SUBFOLDER_NAME,
                language.Name.Replace(Constants.NAMESPACE_SEPARATOR_CHAR, Constants.LANGUAGE_FILE_NAMESPACE_SEPARATOR_REPLACE_CHAR)
            );
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<LocalizationKey>, string> serializeKeys
            ) WriteDefaultConfigOrGetReloadDataTextsBase(
                bool isWriteConfig,
                EnumValue<Language> language,
                Dictionary<EnumValue<LocalizationKey>, string> defaultTextsDict,
                string? comment
            )
        {
            var basePath = GetTextsDictConfigPath(language);
            static string KeySerializer(EnumValue<LocalizationKey> key) => key.Name;
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                null,
                defaultTextsDict,
                KeySerializer,
                comment
            );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<LocalizationKey>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataTextsEnglish(bool isWriteConfig)
        {
            const string? comment = null;
            return WriteDefaultConfigOrGetReloadDataTextsBase(isWriteConfig, Language.ENGLISH, _defaultTextsEnglish, comment);
        }
        #endregion

        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public void LoadDefaultConfigs()
        {
            Tools.LoadDefultAdvancedEnum(_defaultLanguages);
            Tools.LoadDefultAdvancedEnum(_defaultLocalizationKeys);
            LanguageProperties = _defaultLanguageProperties;
            LoadDefaultLanguageDict();
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            WriteDefaultConfigOrGetReloadDataLanguages(true);
            WriteDefaultConfigOrGetReloadDataLocalizationKeys(true);
            WriteDefaultConfigOrGetReloadDataLanguageProperties(true);
            
            WriteDefaultConfigOrGetReloadDataTextsEnglish(true);
        }

        /// <summary>
        /// Reloads all values that come from configs.
        /// </summary>
        /// <param name="namespaceFolders">The name of the currently active config folders.</param>
        /// <param name="isVanillaInvalid">If the vanilla config is valid.</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public void ReloadConfigs(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation = null
        )
        {
            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_LOCALIZATION_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            var languagesData = WriteDefaultConfigOrGetReloadDataLanguages(false);
            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                languagesData.configName,
                namespaceFolders,
                _defaultLanguages,
                isVanillaInvalid,
                showProgressIndentation,
                true,
                comment: languagesData.comment
            );
            
            var localizationKeysData = WriteDefaultConfigOrGetReloadDataLocalizationKeys(false);
            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                localizationKeysData.configName,
                namespaceFolders,
                _defaultLocalizationKeys,
                isVanillaInvalid,
                showProgressIndentation,
                true,
                comment: localizationKeysData.comment
            );
            
            var languagePropertiesData = WriteDefaultConfigOrGetReloadDataLanguageProperties(false);
            ConfigUtils.ReloadConfigsAggregateDict(
                languagePropertiesData.configName,
                namespaceFolders,
                _defaultLanguageProperties,
                languagePropertiesData.serializeKeys,
                key => Language.GetValue(ConfigUtils.GetNameapacedString(key)),
                isVanillaInvalid,
                showProgressIndentation,
                comment: languagePropertiesData.comment
            );

            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_LANGUAGES_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;
            
            var tempLocalizationDict = new Dictionary<EnumValue<Language>, Dictionary<EnumValue<LocalizationKey>, string>>();
            foreach (var language in Language.GetValues())
            {
                Dictionary<EnumValue<LocalizationKey>, string> languageDictionary;
                if (language == Language.ENGLISH)
                {
                    var actionTypeAttributesData = WriteDefaultConfigOrGetReloadDataTextsEnglish(false);
                    languageDictionary = ConfigUtils.ReloadConfigsAggregateDict(
                        actionTypeAttributesData.configName,
                        namespaceFolders,
                        _defaultTextsEnglish,
                        actionTypeAttributesData.serializeKeys,
                        key => LocalizationKey.GetValue(ConfigUtils.GetNameapacedString(key)),
                        isVanillaInvalid,
                        showProgressIndentation,
                        comment: actionTypeAttributesData.comment
                    );
                }
                else
                {
                    languageDictionary = ConfigUtils.GetConfigsAggregateDict<EnumValue<LocalizationKey>, string>(
                        GetTextsDictConfigPath(language),
                        namespaceFolders,
                        key => LocalizationKey.GetValue(ConfigUtils.GetNameapacedString(key)),
                        showProgressIndentation
                    );
                }

                tempLocalizationDict[language] = languageDictionary;
            }

            LocalizationDictionary = tempLocalizationDict;
            PASingletons.Instance.Settings.CurrentLanguage = PASingletons.Instance.Settings.GetCurrentLanguage();
        }
        #endregion
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Returns the name of the <see cref="LocalizationKey"/> that contains the localized name of a language.
        /// </summary>
        /// <param name="language">The language to get the <see cref="LocalizationKey"/> name for.</param>
        public static string GetLanguageNameLocalizationKeyName(EnumValue<Language> language)
        {
            return ConfigUtils.GetSpecificNamespacedString(
                $"{Constants.LANGUAGE_NATIVE_NAME_LOCALIZATION_KEY_PREFIX}_{ConfigUtils.RemoveNamespace(language.Name)}_0",
                ConfigUtils.GetNamespace(language.Name),
                false
            );
        }

        /// <summary>
        /// Gets the localized name of a language in the current language.
        /// </summary>
        /// <param name="language">The language to get the name of.</param>
        /// <param name="localizedString">The localized name of the language.</param>
        /// <returns>If the localized language name was successfuly returned.</returns>
        public bool TryGetLocalizedLanguageName(EnumValue<Language> language, [NotNullWhen(true)] out string? localizedString)
        {
            var languageNameKey = GetLanguageNameLocalizationKeyName(language);
            localizedString = null;
            return LocalizationKey.TryGetValue(languageNameKey, out var localizedKey) &&
                   TryGetLocalizedString(localizedKey, out localizedString);
        }

        /// <summary>
        /// Rtuens the localized name of a language in the current language.
        /// </summary>
        /// <param name="language">The language to get the name of.</param>
        public string GetLocalizedLanguageName(EnumValue<Language> language)
        {
            var languageNameKey = GetLanguageNameLocalizationKeyName(language);
            return LocalizationKey.TryGetValue(languageNameKey, out var localizedKey) &&
                   TryGetLocalizedString(localizedKey, out var localizedString)
                ? localizedString
                : $"[{languageNameKey}]";
        }

        /// <summary>
        /// Adds a new localization to the localization dictionary.
        /// </summary>
        /// <param name="language">The language to add the localization to.</param>
        /// <param name="localizaionKey">The key to add the localization to.</param>
        /// <param name="localizedString">The localized string.</param>
        /// <param name="addToDefaults">Whether to add the new localizatio key and localization to the defaults. Only posible if the language is <see cref="DEFAULT_LANGUAGE"/>.</param>
        /// <returns>If the localization was added.</returns>
        public bool TryAddLocalization(EnumValue<Language> language, EnumValue<LocalizationKey> localizaionKey, string localizedString, bool addToDefaults)
        {
            if (
                !LocalizationDictionary.TryGetValue(language, out var localizedStrings) ||
                !localizedStrings.TryAdd(localizaionKey, localizedString)
            )
            {
                return false;
            }

            if (!addToDefaults)
            {
                return true;
            }

            if (!_defaultLocalizationKeys.Contains(localizaionKey))
            {
                _defaultLocalizationKeys.Add(localizaionKey);
            }

            if (language == DEFAULT_LANGUAGE)
            {
                _defaultTextsEnglish.TryAdd(localizaionKey, localizedString);
            }
            return true;
        }
        #endregion

        #region Private methods
        private void LoadDefaultLanguageDict()
        {
            LocalizationDictionary = new Dictionary<EnumValue<Language>, Dictionary<EnumValue<LocalizationKey>, string>>
            {
                [Language.ENGLISH] = _defaultTextsEnglish.ToDictionary(),
            };
        }
        #endregion
    }
}