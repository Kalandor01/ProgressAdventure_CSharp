using PACommon.Logging;

namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing global variables.
    /// </summary>
    public class Globals
    {
        #region Public fields
        /// <summary>
        /// If the program is in a game (save file loaded), or not.
        /// </summary>
        public bool inGameLoop;
        /// <summary>
        /// If a fight is currently happening, or not.
        /// </summary>
        public bool inFight;
        /// <summary>
        /// If the program is currently exiting a save file.
        /// </summary>
        public bool exiting;
        /// <summary>
        /// If the program is currently saving a save file.
        /// </summary>
        public bool saving;
        #endregion

        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static Globals? _instance = null;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static Globals Instance
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

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Globals" path="//summary"/>
        /// </summary>
        /// <param name="inGameLoop"><inheritdoc cref="inGameLoop" path="//summary"/></param>
        /// <param name="inFight"><inheritdoc cref="inFight" path="//summary"/></param>
        /// <param name="exiting"><inheritdoc cref="exiting" path="//summary"/></param>
        /// <param name="saving"><inheritdoc cref="saving" path="//summary"/></param>
        private Globals(
            bool inGameLoop = false,
            bool inFight = false,
            bool exiting = false,
            bool saving = false
        )
        {
            this.inGameLoop = inGameLoop;
            this.inFight = inFight;
            this.exiting = exiting;
            this.saving = saving;
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the values in the object.
        /// </summary>
        /// <param name="inGameLoop"><inheritdoc cref="inGameLoop" path="//summary"/></param>
        /// <param name="inFight"><inheritdoc cref="inFight" path="//summary"/></param>
        /// <param name="exiting"><inheritdoc cref="exiting" path="//summary"/></param>
        /// <param name="saving"><inheritdoc cref="saving" path="//summary"/></param>
        public static Globals Initialize(
            bool inGameLoop = false,
            bool inFight = false,
            bool exiting = false,
            bool saving = false
        )
        {
            _instance = new Globals(inGameLoop, inFight, exiting, saving);
            Logger.Instance.Log($"{nameof(Globals)} initialized");
            return _instance;
        }
        #endregion
    }
}
