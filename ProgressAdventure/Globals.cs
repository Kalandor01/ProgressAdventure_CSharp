namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing global variables.
    /// </summary>
    public class Globals : IGlobals
    {
        #region Private fields
        private readonly object _inGameLoopLock = new();
        private readonly object _inFightLock = new();
        private readonly object _exitingLock = new();
        private readonly object _savingLock = new();
        private readonly object _pausedLock = new();
        private readonly object _pausingLock = new();
        #endregion

        #region Public properties
        public bool InGameLoop
        {
            get;
            set
            {
                lock (_inGameLoopLock)
                {
                    field = value;
                }
            }
        }

        public bool InFight
        {
            get;
            set
            {
                lock (_inFightLock)
                {
                    field = value;
                }
            }
        }

        public bool Exiting
        {
            get;
            set
            {
                lock (_exitingLock)
                {
                    field = value;
                }
            }
        }

        public bool Saving
        {
            get;
            set
            {
                lock (_savingLock)
                {
                    field = value;
                }
            }
        }

        public bool Paused
        {
            get;
            private set
            {
                lock (_pausedLock)
                {
                    field = value;
                }
            }
        }

        public bool Pausing
        {
            get;
            private set
            {
                lock (_pausingLock)
                {
                    field = value;
                }
            }
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Globals" path="//summary"/>
        /// </summary>
        /// <param name="inGameLoop"><inheritdoc cref="InGameLoop" path="//summary"/></param>
        /// <param name="inFight"><inheritdoc cref="InFight" path="//summary"/></param>
        /// <param name="exiting"><inheritdoc cref="Exiting" path="//summary"/></param>
        /// <param name="saving"><inheritdoc cref="Saving" path="//summary"/></param>
        /// <param name="paused"><inheritdoc cref="Paused" path="//summary"/></param>
        public Globals(
            bool inGameLoop = false,
            bool inFight = false,
            bool exiting = false,
            bool saving = false,
            bool paused = false
        )
        {
            InGameLoop = inGameLoop;
            InFight = inFight;
            Exiting = exiting;
            Saving = saving;
            Paused = paused;
            Pausing = false;
        }
        #endregion

        #region Public methods
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

        public void Unpause()
        {
            Pausing = false;
            Paused = false;
        }

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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
