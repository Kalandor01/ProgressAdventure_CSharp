using PACommon.ConfigManagement;
using PACommon.JsonUtils;
using PACommon.Logging;

namespace PACommon
{
    /// <summary>
    /// Contains commonly used classes from PACommon, that should only have 1 instance per project.
    /// </summary>
    public class PACSingletons : IDisposable
    {
        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static PACSingletons? _instance = null;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static PACSingletons Instance
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

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// The json data correcter.
        /// </summary>
        public IJsonDataCorrecter JsonDataCorrecter { get; private set; }

        /// <summary>
        /// The configuration manager.
        /// </summary>
        public AConfigManager ConfigManager { get; private set; }
        #endregion

        #region Private Constructors
        /// <summary>
        /// <inheritdoc cref="PACSingletons"/>
        /// </summary>
        /// <param name="logger"><inheritdoc cref="Logger" path="//summary"/></param>
        /// <param name="jsonDataCorrecter"><inheritdoc cref="JsonDataCorrecter" path="//summary"/></param>
        /// <param name="configManager"><inheritdoc cref="ConfigManager" path="//summary"/></param>
        private PACSingletons(
            ILogger logger,
            IJsonDataCorrecter jsonDataCorrecter,
            AConfigManager configManager
        )
        {
            Logger = logger;
            JsonDataCorrecter = jsonDataCorrecter;
            ConfigManager = configManager;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="logger"><inheritdoc cref="Logger" path="//summary"/></param>
        /// <param name="jsonDataCorrecter"><inheritdoc cref="JsonDataCorrecter" path="//summary"/></param>
        /// <param name="configManager"><inheritdoc cref="ConfigManager" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        public static PACSingletons Initialize(
            ILogger? logger = null,
            IJsonDataCorrecter? jsonDataCorrecter = null,
            AConfigManager? configManager = null,
            bool logInitialization = true
        )
        {
            _instance = new PACSingletons(
                logger ?? Logging.Logger.Instance,
                jsonDataCorrecter ?? JsonUtils.JsonDataCorrecter.Instance,
                configManager ?? ConfigManagement.ConfigManager.Instance
            );
            if (logInitialization)
            {
                _instance.Logger.Log($"{nameof(Logging.Logger)} initialized", newLine: true);
                _instance.Logger.Log($"{nameof(JsonUtils.JsonDataCorrecter)} initialized");
                _instance.Logger.Log($"{nameof(AConfigManager)} initialized");
                _instance.Logger.Log($"{nameof(PACSingletons)} initialized");
            }
            return _instance;
        }

        public void Dispose()
        {
            _instance?.Logger.Dispose();
        }
        #endregion
    }
}
