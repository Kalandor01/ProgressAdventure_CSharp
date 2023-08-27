namespace PAVisualiser
{
    /// <summary>
    /// Object for storing constants.
    /// </summary>
    public static class Constants
    {
        #region Threads
        /// <summary>
        /// The display name of the save visualiser thread.
        /// </summary>
        public const string VISUALIZER_THREAD_NAME = "Visualizer";
        /// <summary>
        /// The display name of the save visualiser window thread.
        /// </summary>
        public const string VISUALISER_WINDOW_THREAD_NAME = "VisualiserWindow";
        #endregion

        #region Misc
        public static readonly string EXPORT_FOLDER = "visualised_saves";
        public static readonly string EXPORT_DATA_FILE = "data.txt";
        public static readonly string EXPORT_TERRAIN_FILE = "terrain.png";
        public static readonly string EXPORT_STRUCTURE_FILE = "structure.png";
        public static readonly string EXPORT_POPULATOIN_FILE = "population.png";
        public static readonly string EXPORT_COMBINED_FILE = "combined.png";
        #endregion

        public static class Colors
        {
            public static readonly ColorData TRANSPARENT = new(0, 0, 0, 0);
            public static readonly ColorData MAGENTA = new(255, 0, 255);
            public static readonly ColorData RED = new(255, 0, 0);
            public static readonly ColorData DARK_RED = new(150, 0, 0);
            public static readonly ColorData GREEN = new(0, 255, 0);
            public static readonly ColorData BLUE = new(0, 0, 255);
            public static readonly ColorData BROWN = new(61, 42, 27);
            public static readonly ColorData SKIN = new(212, 154, 99);
            public static readonly ColorData LIGHT_BLUE = new(60, 60, 255);
            public static readonly ColorData LIGHT_GRAY = new(75, 75, 75);
            public static readonly ColorData LIGHT_BROWN = new(82, 56, 36);
            public static readonly ColorData LIGHTER_BLUE = new(99, 99, 255);
            public static readonly ColorData DARK_GREEN = new(28, 87, 25);
        }
    }
}
