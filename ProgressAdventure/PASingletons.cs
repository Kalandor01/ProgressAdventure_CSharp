using PACommon;
using ProgressAdventure.SettingsManagement;

namespace ProgressAdventure
{
    /// <summary>
    /// Contains commonly used classes from ProgressAdventure, that should only have 1 instance per project.
    /// </summary>
    public class PASingletons : IDisposable
    {
        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static PASingletons? _instance = null;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static PASingletons Instance
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
        /// <inheritdoc cref="ProgressAdventure.Globals"/>
        /// </summary>
        public IGlobals Globals { get; private set; }

        /// <summary>
        /// <inheritdoc cref="SettingsManagement.Settings"/>
        /// </summary>
        public ISettings Settings { get; private set; }
        #endregion

        #region Private Constructors
        /// <summary>
        /// <inheritdoc cref="PASingletons"/>
        /// </summary>
        /// <param name="globals"><inheritdoc cref="Globals" path="//summary"/></param>
        /// <param name="globals"><inheritdoc cref="Globals" path="//summary"/></param>
        private PASingletons(
            IGlobals globals,
            ISettings settings
        )
        {
            Globals = globals;
            Settings = settings;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="globals"><inheritdoc cref="Globals" path="//summary"/></param>
        /// <param name="settings"><inheritdoc cref="Settings" path="//summary"/></param>
        /// <param name="logInitialization">Whether to log the fact that the singleton was initialized.</param>
        /// <param name="onlyIfUninitialized">If true, only initializes the singleton if it hasn't been initialized yet.</param>
        public static PASingletons Initialize(
            IGlobals? globals = null,
            ISettings? settings = null,
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
                _instance = new PASingletons(
                    globals ?? new Globals(),
                    settings ?? new Settings()
                );
                if (logInitialization)
                {
                    PACSingletons.Instance.Logger.Log($"{nameof(IGlobals)} initialized");
                    PACSingletons.Instance.Logger.Log($"{nameof(ISettings)} initialized");
                    PACSingletons.Instance.Logger.Log($"{nameof(PASingletons)} initialized");
                }
                return _instance;
            }
        }

        public void Dispose()
        {
            _instance?.Globals.Dispose();
            _instance?.Settings.Dispose();
            _instance = null;
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
