namespace ProgressAdventure
{
    /// <summary>
    /// Object for storing global variables.
    /// </summary>
    public static class Globals
    {
        #region Public fields
        /// <summary>
        /// If the program is in a game (save file loaded), or not.
        /// </summary>
        public static bool inGameLoop;
        /// <summary>
        /// If a fight is currently happening, or not.
        /// </summary>
        public static bool inFight;
        /// <summary>
        /// If the program is currently exiting a save file.
        /// </summary>
        public static bool exiting;
        /// <summary>
        /// If the program is currently saving a save file.
        /// </summary>
        public static bool saving;
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initializes the values in the object.
        /// </summary>
        /// <param name="inGameLoop"><inheritdoc cref="inGameLoop" path="//summary"/></param>
        /// <param name="inFight"><inheritdoc cref="inFight" path="//summary"/></param>
        /// <param name="exiting"><inheritdoc cref="exiting" path="//summary"/></param>
        /// <param name="saving"><inheritdoc cref="saving" path="//summary"/></param>
        public static void Initialize(
            bool inGameLoop = false,
            bool inFight = false,
            bool exiting = false,
            bool saving = false
        )
        {
            Globals.inGameLoop = inGameLoop;
            Globals.inFight = inFight;
            Globals.exiting = exiting;
            Globals.saving = saving;
        }
        #endregion
    }
}
