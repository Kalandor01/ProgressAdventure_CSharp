using System.Windows.Media;

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

        public static class Colors
        {
            public static readonly Color RED = Color.FromArgb(255, 255, 0, 0);
            public static readonly Color GREEN = Color.FromArgb(255, 0, 255, 0);
            public static readonly Color BLUE = Color.FromArgb(255, 0, 0, 255);
            public static readonly Color BROWN = Color.FromArgb(255, 61, 42, 27);
            public static readonly Color SKIN = Color.FromArgb(255, 212, 154, 99);
            public static readonly Color LIGHT_BLUE = Color.FromArgb(255, 60, 60, 255);
            public static readonly Color LIGHT_GRAY = Color.FromArgb(255, 75, 75, 75);
            public static readonly Color LIGHT_BROWN = Color.FromArgb(255, 82, 56, 36);
            public static readonly Color LIGHTER_BLUE = Color.FromArgb(255, 99, 99, 255);
            public static readonly Color DARK_GREEN = Color.FromArgb(255, 28, 87, 25);
        }
    }
}
