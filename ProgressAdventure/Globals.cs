namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing global variables.
    /// </summary>
    public class Globals
    {
        #region Private fields
        private object _inGameLoopLock = new();
        private object _inFightLock = new();
        private object _exitingLock = new();
        private object _savingLock = new();
        private object _pausedLock = new();
        private object _pausingLock = new();

        /// <summary>
        /// If the program is in a game (save file loaded).
        /// </summary>
        private bool _inGameLoop;
        /// <summary>
        /// If a fight is currently happening.
        /// </summary>
        private bool _inFight;
        /// <summary>
        /// If the program is currently exiting a save file.
        /// </summary>
        private bool _exiting;
        /// <summary>
        /// If the program is currently saving a save file.
        /// </summary>
        private bool _saving;
        /// <summary>
        /// If the game is paused.
        /// </summary>
        private bool _paused;
        /// <summary>
        /// If the game is playing, but trying to pause.
        /// </summary>
        private bool _pausing;
        #endregion

        #region Public properties
        /// <inheritdoc cref="_inGameLoop"/>
        public bool InGameLoop
        {
            get => _inGameLoop;
            set
            {
                lock (_inGameLoopLock)
                {
                    _inGameLoop = value;
                }
            }
        }

        /// <inheritdoc cref="_inFight"/>
        public bool InFight
        {
            get => _inFight;
            set
            {
                lock (_inFightLock)
                {
                    _inFight = value;
                }
            }
        }

        /// <inheritdoc cref="_exiting"/>
        public bool Exiting
        {
            get => _exiting;
            set
            {
                lock (_exitingLock)
                {
                    _exiting = value;
                }
            }
        }

        /// <inheritdoc cref="_saving"/>
        public bool Saving
        {
            get => _saving;
            set
            {
                lock (_savingLock)
                {
                    _saving = value;
                }
            }
        }

        /// <inheritdoc cref="_paused"/>
        public bool Paused
        {
            get => _paused;
            private set
            {
                lock (_pausedLock)
                {
                    _paused = value;
                }
            }
        }

        /// <inheritdoc cref="_pausing"/>
        public bool Pausing
        {
            get => _pausing;
            private set
            {
                lock (_pausingLock)
                {
                    _pausing = value;
                }
            }
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Globals" path="//summary"/>
        /// </summary>
        /// <param name="inGameLoop"><inheritdoc cref="_inGameLoop" path="//summary"/></param>
        /// <param name="inFight"><inheritdoc cref="_inFight" path="//summary"/></param>
        /// <param name="exiting"><inheritdoc cref="_exiting" path="//summary"/></param>
        /// <param name="saving"><inheritdoc cref="_saving" path="//summary"/></param>
        /// <param name="paused"><inheritdoc cref="_paused" path="//summary"/></param>
        public Globals(
            bool inGameLoop = false,
            bool inFight = false,
            bool exiting = false,
            bool saving = false,
            bool paused = false
        )
        {
            _inGameLoop = inGameLoop;
            _inFight = inFight;
            _exiting = exiting;
            _saving = saving;
            _paused = paused;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Pauses the game.
        /// </summary>
        /// <returns>True if the game shouldn't be paused (because it's exiting).</returns>
        public bool Pause()
        {
            Pausing = true;
            while (!Paused)
            {
                if (!InGameLoop || Exiting)
                {
                    return true;
                }
                Thread.Sleep(Constants.GLOBALS_PAUSED_CHECK_FREQUENCY);
            }
            return false;
        }

        /// <summary>
        /// Unpauses the game.
        /// </summary>
        public void Unpause()
        {
            Pausing = false;
            Paused = false;
        }

        /// <summary>
        /// Pauses the thread while the game is paused.
        /// </summary>
        public void PauseLock()
        {
            if (Pausing)
            {
                Paused = true;
                Pausing = false;
            }
            while (Paused)
            {
                Thread.Sleep(Constants.GLOBALS_PAUSED_CHECK_FREQUENCY);
            }
        }
        #endregion
    }
}
