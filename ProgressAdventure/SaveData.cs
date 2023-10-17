using PACommon;
using PACommon.Enums;
using ProgressAdventure.Entity;
using PACTools = PACommon.Tools;

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
        /// Initializes the object's values.
        /// </summary>
        /// <param name="saveName"><inheritdoc cref="saveName" path="//summary"/></param>
        /// <param name="displaySaveName"><inheritdoc cref="displaySaveName" path="//summary"/></param>
        /// <param name="lastSave"><inheritdoc cref="LastSave" path="//summary"/></param>
        /// <param name="playtime"><inheritdoc cref="Playtime" path="//summary"/></param>
        /// <param name="player"><inheritdoc cref="player" path="//summary"/></param>
        public static void Initialize(
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
                RandomStates.Initialize();
            }

            SaveData.player = player ?? new Player();
        }
        #endregion

        #region Public functions
        public static TimeSpan GetPlaytime()
        {
            return Playtime + DateTime.Now.Subtract(LastLoad);
        }
        #endregion

        #region JsonConvert
        private static readonly List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> versionCorrecters = new()
        {
            // 2.0.1 -> 2.0.2
            (oldJson => {
                // saved entity types
                if (
                    oldJson.TryGetValue("player", out object? playerJson) &&
                    playerJson is IDictionary<string, object?> playerDict
                )
                {
                    playerDict["type"] = "player";
                }
            }, "2.0.2"),
            // 2.1.1 -> 2.2
            (oldJson => {
                // snake case rename
                if (oldJson.TryGetValue("displayName", out object? dnRename))
                {
                    oldJson["display_name"] = dnRename;
                }
                if (oldJson.TryGetValue("lastSave", out object? lsRename))
                {
                    oldJson["last_save"] = lsRename;
                }
            }, "2.2"),
        };

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

        /// <summary>
        /// Converts the json representation of the object to object format.
        /// </summary>
        /// <param name="saveName">The name of the save file.</param>
        /// <param name="saveDataJson">The json representation of the SaveData.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static bool FromJson(string saveName, IDictionary<string, object?>? saveDataJson, string fileVersion)
        {
            if (saveDataJson is null)
            {
                Logger.Instance.Log($"{typeof(SaveData)} parse error", $"{typeof(SaveData).ToString().ToLower()} json is null", LogSeverity.ERROR);
                Initialize(saveName);
                return false;
            }

            PACTools.CorrectJsonData(typeof(SaveData).ToString(), ref saveDataJson, versionCorrecters, fileVersion);

            return FromJsonWithoutCorrection(saveName, saveDataJson, fileVersion);
        }

        /// <summary>
        /// Converts the json representation of the object to object format.
        /// </summary>
        /// <param name="saveName">The name of the save file.</param>
        /// <param name="saveDataJson">The json representation of the RandomState.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        private static bool FromJsonWithoutCorrection(string saveName, IDictionary<string, object?> saveDataJson, string fileVersion)
        {
            // display name
            var displayName = saveDataJson["display_name"] as string;
            // last save
            var lastSave = saveDataJson["last_save"] as DateTime?;
            // playtime
            _ = TimeSpan.TryParse(saveDataJson["playtime"]?.ToString(), out var playtime);
            // player
            var playerData = saveDataJson["player"] as IDictionary<string, object?>;
            PACTools.TryFromJson(playerData, fileVersion, out Player? player);

            Initialize(saveName, displayName, lastSave, playtime, player, false);
            return true;
        }
        #endregion
    }
}
