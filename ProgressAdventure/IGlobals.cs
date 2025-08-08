namespace ProgressAdventure
{
    /// <summary>
    /// Interface for storing global variables.
    /// </summary>
    public interface IGlobals : IDisposable
    {
        #region Public properties
        /// <summary>
        /// If the program is in a game (save file loaded).
        /// </summary>
        public bool InGameLoop { get; set; }

        /// <summary>
        /// If a fight is currently happening.
        /// </summary>
        public bool InFight { get; set; }

        /// <summary>
        /// If the program is currently exiting a save file.
        /// </summary>
        public bool Exiting { get; set; }

        /// <summary>
        /// If the program is currently saving a save file.
        /// </summary>
        public bool Saving { get; set; }

        /// <summary>
        /// If the game is paused.
        /// </summary>
        public bool Paused { get; }

        /// <summary>
        /// If the game is playing, but trying to pause.
        /// </summary>
        public bool Pausing { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Pauses the game.
        /// </summary>
        /// <returns>True if the game shouldn't be paused (because it's exiting).</returns>
        public bool Pause();

        /// <summary>
        /// Unpauses the game.
        /// </summary>
        public void Unpause();

        /// <summary>
        /// Pauses the thread while the game is paused.
        /// </summary>
        public void PauseLock();
        #endregion
    }
}
