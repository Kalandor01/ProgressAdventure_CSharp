namespace ProgressAdventureTests
{
    /// <summary>
    /// Object for storing constants.
    /// </summary>
    public static class Constants
    {
        #region Misc
        /// <summary>
        /// Whether to initialise Globals and Setting in the main preload.
        /// </summary>
        public const bool PRELOAD_GLOBALS_ON_PRELOAD = false;
        /// <summary>
        /// The path to the folder where the compressed saves are stored for the save correction test.
        /// </summary>
        public const string TEST_REFERENCE_SAVES_FOLDER_PATH = ".\\TestSaves";
        /// <summary>
        /// The path to the folder where the compressed imported saves are stored for the save correction test.
        /// </summary>
        public const string IMPORTED_REFERENCE_SAVES_FOLDER_PATH = ".\\ImportedSaves";
        #endregion
    }
}
