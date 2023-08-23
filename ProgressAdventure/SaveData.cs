using ProgressAdventure.Entity;

namespace ProgressAdventure
{
    public static class SaveData
    {
        #region Public fields
        /// <summary>
        /// The name of the save folder.
        /// </summary>
        public static string saveName;
        /// <summary>
        /// The save name to display.
        /// </summary>
        public static string displaySaveName;
        /// <summary>
        /// The player object.
        /// </summary>
        public static Player player;
        #endregion

        #region Public properties
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public static DateTime LastSave { get; private set; }
        /// <summary>
        /// The last time, the save file was loaded.
        /// </summary>
        public static DateTime LastLoad { get; private set; }
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public static TimeSpan Playtime { get; private set; }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initialises the object's values.
        /// </summary>
        /// <param name="saveName"><inheritdoc cref="saveName" path="//summary"/></param>
        /// <param name="displaySaveName"><inheritdoc cref="displaySaveName" path="//summary"/></param>
        /// <param name="lastSave"><inheritdoc cref="LastSave" path="//summary"/></param>
        /// <param name="playtime"><inheritdoc cref="Playtime" path="//summary"/></param>
        /// <param name="player"><inheritdoc cref="player" path="//summary"/></param>
        public static void Initialise(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastSave = null,
            TimeSpan? playtime = null,
            Player? player = null,
            bool initialiseRandomGenerators = true
        )
        {
            SaveData.saveName = saveName;
            SaveData.displaySaveName = displaySaveName ?? saveName;
            LastSave = lastSave ?? DateTime.Now;
            LastLoad = DateTime.Now;
            Playtime = playtime ?? TimeSpan.Zero;

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
                ["save_version"] = Constants.SAVE_VERSION,
                ["display_name"] = displaySaveName,
                ["last_save"] = LastSave,
                ["playtime"] = GetPlaytime(),
                ["player_name"] = player.name
            };
        }

        /// <summary>
        /// Converts the data for the main part of the data file to a json format.
        /// </summary>
        public static Dictionary<string, object?> MainDataToJson()
        {
            return new Dictionary<string, object?> {
                ["save_version"] = Constants.SAVE_VERSION,
                ["display_name"] = displaySaveName,
                ["last_save"] = LastSave,
                ["playtime"] = GetPlaytime(),
                ["player"] = player.ToJson(),
                ["random_states"] = RandomStates.ToJson()
            };
        }

        public static (Dictionary<string, object?> displayData, Dictionary<string, object?> mainData) MakeSaveData()
        {
            LastSave = DateTime.Now;
            return (DisplayDataToJson(), MainDataToJson());
        }

        public static TimeSpan GetPlaytime()
        {
            return Playtime + DateTime.Now.Subtract(LastLoad);
        }
        #endregion
    }
}
