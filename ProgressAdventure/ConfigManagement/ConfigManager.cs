using PACommon;
using System.Text.Json.Serialization;

namespace ProgressAdventure.ConfigManagement
{
    /// <summary>
    /// Class for reading config files, for loading config dictionaries.
    /// </summary>
    public class ConfigManager : AConfigManager
    {
        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static ConfigManager? _instance = null;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_threadLock)
                    {
                        _instance ??= Initialize();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="ConfigManager"/>
        /// </summary>
        /// <param name="converters">The list of json serializers to use when converting to/from specific types.<br/>
        /// Loads the default list of converters if null.</param>
        /// <param name="configsFolderPath"><inheritdoc cref="AConfigManager._configsFolderPath" path="//summary"/></param>
        /// <param name="configExtension"><inheritdoc cref="AConfigManager._configExtension" path="//summary"/></param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="configsFolderParrentPath"/> doesn't exist.</exception>
        public ConfigManager(
            JsonConverter[]? converters,
            string? configsFolderPath = null,
            string configExtension = Constants.CONFIG_EXT
        )
            : base(converters, configsFolderPath, configExtension) { }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="converters">The list of json serializers to use when converting to/from specific types.<br/>
        /// Loads the default list of converters if null.</param>
        /// <param name="configsFolderPath"><inheritdoc cref="AConfigManager._configsFolderPath" path="//summary"/></param>
        /// <param name="configExtension"><inheritdoc cref="AConfigManager._configExtension" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="configsFolderParrentPath"/> doesn't exist.</exception>
        public static ConfigManager Initialize(
            JsonConverter[]? converters = null,
            string? configsFolderPath = null,
            string configExtension = Constants.CONFIG_EXT,
            bool logInitialization = true
        )
        {
            converters ??=
            [
                new JsonStringEnumConverter(allowIntegerValues: false),
                new ItemTypeIDConverter(),
                new MaterialItemAttributesDTOConverter(),
            ];

            _instance = new ConfigManager(
                converters,
                configsFolderPath,
                configExtension
            );
            if (logInitialization)
            {
                PACSingletons.Instance.Logger.Log($"{nameof(ConfigManager)} initialized");
            }
            return _instance;
        }
        #endregion
    }
}
