namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing global variables.
    /// </summary>
    public class Globals
    {
        #region Public fields
        /// <summary>
        /// If the program is in a game (save file loaded).
        /// </summary>
        public bool inGameLoop;
        /// <summary>
        /// If a fight is currently happening.
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
        /// <summary>
        /// If the game is playing, but paused.
        /// </summary>
        public bool paused;
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Globals" path="//summary"/>
        /// </summary>
        /// <param name="inGameLoop"><inheritdoc cref="inGameLoop" path="//summary"/></param>
        /// <param name="inFight"><inheritdoc cref="inFight" path="//summary"/></param>
        /// <param name="exiting"><inheritdoc cref="exiting" path="//summary"/></param>
        /// <param name="saving"><inheritdoc cref="saving" path="//summary"/></param>
        /// <param name="paused"><inheritdoc cref="paused" path="//summary"/></param>
        public Globals(
            bool inGameLoop = false,
            bool inFight = false,
            bool exiting = false,
            bool saving = false,
            bool paused = false
        )
        {
            this.inGameLoop = inGameLoop;
            this.inFight = inFight;
            this.exiting = exiting;
            this.saving = saving;
            this.paused = paused;
        }
        #endregion
    }
}
