using ProgressAdventure.Entity;

namespace ProgressAdventure
{
    public static class SaveData
    {
        #region Public fields
        /// <summary>
        /// The name of the save file.
        /// </summary>
        public static string saveName;
        /// <summary>
        /// The save name to display.
        /// </summary>
        public static string displaySaveName;
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public static DateTime lastAccess;
        /// <summary>
        /// The player object.
        /// </summary>
        public static Player player;
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initialises the object's values.
        /// </summary>
        /// <param name="saveName"><inheritdoc cref="saveName" path="//summary"/></param>
        /// <param name="displaySaveName"><inheritdoc cref="displaySaveName" path="//summary"/></param>
        /// <param name="lastAccess"><inheritdoc cref="lastAccess" path="//summary"/></param>
        /// <param name="player"><inheritdoc cref="player" path="//summary"/></param>
        public static void Initialise(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastAccess = null,
            Player? player = null,
            bool initialiseRandomGenerators = true
        )
        {
            SaveData.saveName = saveName;
            SaveData.displaySaveName = displaySaveName ?? saveName;
            SaveData.lastAccess = lastAccess ?? DateTime.Now;

            if (initialiseRandomGenerators)
            {
                RandomStates.Initialise();
            }

            SaveData.player = player ?? new Player();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the data for the display part of the data file to a json format.
        /// </summary>
        public static Dictionary<string, object?> DisplayDataToJson()
        {
            return new Dictionary<string, object?> {
                ["saveVersion"] = Constants.SAVE_VERSION,
                ["displayName"] = displaySaveName,
                ["lastAccess"] = DateTime.Now,
                ["playerName"] = player.name
            };
        }

        /// <summary>
        /// Converts the data for the main part of the data file to a json format.
        /// </summary>
        public static Dictionary<string, object?> MainDataToJson()
        {
            return new Dictionary<string, object?> {
                ["saveVersion"] = Constants.SAVE_VERSION,
                ["displayName"] = displaySaveName,
                ["lastAccess"] = DateTime.Now,
                ["player"] = player.ToJson(),
                ["randomStates"] = RandomStates.ToJson()
            };
        }
        #endregion
    }
}
