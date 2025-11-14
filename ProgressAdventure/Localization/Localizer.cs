using PACommon;
using PACommon.Enums;
using ProgressAdventure.ConfigManagement;

namespace ProgressAdventure.Localization
{
    /// <summary>
    /// Class used to localize text.
    /// </summary>
    public class Localizer : ALocalizer<Language, Text>, IDisposable
    {
        #region Default config values
        /// <summary>
        /// The default value for the config used for the values of <see cref="Language"/>.
        /// </summary>
        private static readonly List<EnumValue<Language>> _defaultLanguages =
        [
            Language.ENGLISH,
        ];

        /// <summary>
        /// The default value for the config used for the value of the texts in <see cref="LocalizationDictionary"/> for <see cref="Language.ENGLISH"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<Text>, string> _defaultTextsEnglish = new()
        {
            [Text.LOADING_FROM_FOLDER_1] = "Loading from folder \"{0}\":",
            [Text.LOADING_FILE_FROM_CONFIG_1] = "Loading file \"{0}\" from config:",
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

        #region Protected properties
        protected override Dictionary<EnumValue<Language>, Dictionary<EnumValue<Text>, string>> LocalizationDictionary { get; set; }
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static Localizer Instance
        {
            get
            {
                _instance ??= Initialize(Language.ENGLISH, onlyIfUninitialized: true);
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
            LoadDefaultLanguageDict();
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
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        
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

        private static string GetTextsDictConfigPath(EnumValue<Language> language)
        {
            return Path.Join(Constants.CONFIGS_LOCALIZATION_SUBFOLDER_NAME, Constants.CONFIGS_LANGUAGES_SUBFOLDER_NAME, language.Name);
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<Text>, string> serializeKeys
            ) WriteDefaultConfigOrGetReloadDataTextsBase(
                bool isWriteConfig,
                EnumValue<Language> language,
                Dictionary<EnumValue<Text>, string> defaultTextsDict,
                string? comment
            )
        {
            var basePath = GetTextsDictConfigPath(language);
            static string KeySerializer(EnumValue<Text> key) => key.Name;
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
            Func<EnumValue<Text>, string> serializeKeys
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
            LoadDefaultLanguageDict();
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            WriteDefaultConfigOrGetReloadDataLanguages(true);
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

            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_LANGUAGES_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;
            
            var tempLocalizationDict = new Dictionary<EnumValue<Language>, Dictionary<EnumValue<Text>, string>>();
            foreach (var language in Language.GetValues())
            {
                Dictionary<EnumValue<Text>, string> languageDictionary;
                if (language == Language.ENGLISH)
                {
                    var actionTypeAttributesData = WriteDefaultConfigOrGetReloadDataTextsEnglish(false);
                    languageDictionary = ConfigUtils.ReloadConfigsAggregateDict(
                        actionTypeAttributesData.configName,
                        namespaceFolders,
                        _defaultTextsEnglish,
                        actionTypeAttributesData.serializeKeys,
                        key => Text.GetValue(ConfigUtils.GetNameapacedString(key)),
                        isVanillaInvalid,
                        showProgressIndentation,
                        comment: actionTypeAttributesData.comment
                    );
                }
                else
                {
                    languageDictionary = ConfigUtils.GetConfigsAggregateDict<EnumValue<Text>, string>(
                        GetTextsDictConfigPath(language),
                        namespaceFolders,
                        key => Text.GetValue(ConfigUtils.GetNameapacedString(key)),
                        showProgressIndentation
                    );
                }

                tempLocalizationDict[language] = languageDictionary;
            }

            LocalizationDictionary = tempLocalizationDict;
        }
        #endregion
        #endregion

        #region Private methods

        private void LoadDefaultLanguageDict()
        {
            LocalizationDictionary = new Dictionary<EnumValue<Language>, Dictionary<EnumValue<Text>, string>>
            {
                [Language.ENGLISH] = _defaultTextsEnglish,
            };
        }
        #endregion
    }
}