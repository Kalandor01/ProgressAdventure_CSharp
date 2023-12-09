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
        /// <inheritdoc cref="ProgressAdventure.Globals"/>
        /// </summary>
        public Globals Globals { get; private set; }

        /// <summary>
        /// <inheritdoc cref="SettingsManagement.Settings"/>
        /// </summary>
        public Settings Settings { get; private set; }
        #endregion

        #region Private Constructors
        /// <summary>
        /// <inheritdoc cref="PASingletons"/>
        /// </summary>
        /// <param name="globals"><inheritdoc cref="Globals" path="//summary"/></param>
        /// <param name="globals"><inheritdoc cref="Globals" path="//summary"/></param>
        private PASingletons(
            Globals globals,
            Settings settings
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
        public static PASingletons Initialize(
            Globals? globals = null,
            Settings? settings = null,
            bool logInitialization = true
        )
        {
            _instance = new PASingletons(
                globals ?? new Globals(),
                settings ?? new Settings()
            );
            if (logInitialization)
            {
                PACSingletons.Instance.Logger.Log($"{nameof(ProgressAdventure.Globals)} initialized");
                PACSingletons.Instance.Logger.Log($"{nameof(SettingsManagement.Settings)} initialized");
                PACSingletons.Instance.Logger.Log($"{nameof(PASingletons)} initialized");
            }
            return _instance;
        }

        public void Dispose()
        {
            
        }
        #endregion
    }
}
