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
                if (_instance is null)
                {
                    _instance ??= Initialize(onlyIfUninitialized: true);
                }
                return _instance;
            }
        }

        /// <summary>
        /// If the singleton is currently initialized.
        /// </summary>
        public static bool IsInitialized => _instance is not null;

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// The proxy to the output.
        /// </summary>
        public IPAConsoleProxy ConsoleProxy { get; private set; }

        /// <summary>
        /// The json data correcter.
        /// </summary>
        public IJsonDataCorrecter JsonDataCorrecter { get; private set; }

        /// <summary>
        /// The configuration manager.
        /// </summary>
        public IConfigManager ConfigManager { get; private set; }
        #endregion

        #region Private Constructors
        /// <summary>
        /// <inheritdoc cref="PACSingletons"/>
        /// </summary>
        /// <param name="logger"><inheritdoc cref="Logger" path="//summary"/></param>
        /// <param name="consoleProxy"><inheritdoc cref="ConsoleProxy" path="//summary"/></param>
        /// <param name="jsonDataCorrecter"><inheritdoc cref="JsonDataCorrecter" path="//summary"/></param>
        /// <param name="configManager"><inheritdoc cref="ConfigManager" path="//summary"/></param>
        private PACSingletons(
            ILogger logger,
            IPAConsoleProxy consoleProxy,
            IJsonDataCorrecter jsonDataCorrecter,
            IConfigManager configManager
        )
        {
            Logger = logger;
            ConsoleProxy = consoleProxy;
            JsonDataCorrecter = jsonDataCorrecter;
            ConfigManager = configManager;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="logger"><inheritdoc cref="Logger" path="//summary"/></param>
        /// <param name="consoleProxy"><inheritdoc cref="ConsoleProxy" path="//summary"/></param>
        /// <param name="jsonDataCorrecter"><inheritdoc cref="JsonDataCorrecter" path="//summary"/></param>
        /// <param name="configManager"><inheritdoc cref="ConfigManager" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        /// <param name="onlyIfUninitialized">If true, only initializes the singleton if it hasn't been initialized yet.</param>
        public static PACSingletons Initialize(
            ILogger? logger = null,
            IPAConsoleProxy? consoleProxy = null,
            IJsonDataCorrecter? jsonDataCorrecter = null,
            IConfigManager? configManager = null,
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
                _instance = new PACSingletons(
                    logger ?? Logging.Logger.Instance,
                    consoleProxy ?? new PAConsoleProxy(),
                    jsonDataCorrecter ?? JsonUtils.JsonDataCorrecter.Instance,
                    configManager ?? ConfigManagement.ConfigManager.Instance
                );
                if (logInitialization)
                {
                    _instance.Logger.Log($"{nameof(ILogger)} initialized", newLine: true);
                    _instance.Logger.Log($"{nameof(IPAConsoleProxy)} initialized");
                    _instance.Logger.Log($"{nameof(IJsonDataCorrecter)} initialized");
                    _instance.Logger.Log($"{nameof(IConfigManager)} initialized");
                    _instance.Logger.Log($"{nameof(PACSingletons)} initialized");
                }
                return _instance;
            }
        }

        public void Dispose()
        {
            _instance?.ConfigManager?.Dispose();
            _instance?.JsonDataCorrecter?.Dispose();
            _instance?.ConsoleProxy?.Dispose();
            _instance?.Logger.Dispose();
            _instance = null;
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
