using PACommon;
using PACommon.JsonUtils;
using ProgressAdventure.Entity;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;

namespace ProgressAdventure
{
    public class SaveData : IJsonConvertable<SaveData>
    {
        #region Public fields
        /// <summary>
        /// The name of the save folder.
        /// </summary>
        public string saveName;
        /// <summary>
        /// The save name to display.
        /// </summary>
        public string displaySaveName;
        /// <summary>
        /// The player object.
        /// </summary>
        public Player player;
        #endregion

        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static SaveData? _instance = null;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static SaveData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_threadLock)
                    {
                        _instance ??= Initialize(Constants.DEFAULT_SAVE_DATA_SAVE_NAME);
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public DateTime LastSave { get; private set; }
        /// <summary>
        /// The last time, the save file was loaded.
        /// </summary>
        public DateTime LastLoad { get; private set; }
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public TimeSpan Playtime { get; private set; }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="SaveData" path="//summary"/>
        /// </summary>
        /// <param name="saveName"><inheritdoc cref="saveName" path="//summary"/></param>
        /// <param name="displaySaveName"><inheritdoc cref="displaySaveName" path="//summary"/></param>
        /// <param name="lastSave"><inheritdoc cref="LastSave" path="//summary"/></param>
        /// <param name="playtime"><inheritdoc cref="Playtime" path="//summary"/></param>
        /// <param name="player"><inheritdoc cref="player" path="//summary"/></param>
        /// <param name="initialiseRandomGenerators">Whether to initialize the RandomStates object as well.</param>
        private SaveData(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastSave = null,
            TimeSpan? playtime = null,
            Player? player = null,
            bool initialiseRandomGenerators = true
        )
        {
            this.saveName = saveName;
            this.displaySaveName = displaySaveName ?? saveName;
            LastSave = lastSave ?? DateTime.Now;
            LastLoad = DateTime.Now;
            Playtime = playtime ?? TimeSpan.Zero;

            if (initialiseRandomGenerators)
            {
                RandomStates.Initialize();
            }

            this.player = player ?? new Player();
        }
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
        /// <param name="initialiseRandomGenerators">Whether to initialize the RandomStates object as well.</param>
        public static SaveData Initialize(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastSave = null,
            TimeSpan? playtime = null,
            Player? player = null,
            bool initialiseRandomGenerators = true
        )
        {
            _instance = new SaveData(saveName, displaySaveName, lastSave, playtime, player, initialiseRandomGenerators);
            PACSingletons.Instance.Logger.Log($"{nameof(SaveData)} initialized");
            return _instance;
        }
        #endregion

        #region Public functions
        public TimeSpan GetPlaytime()
        {
            return Playtime + DateTime.Now.Subtract(LastLoad);
        }
        #endregion

        #region JsonConvert
        static List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<SaveData>.VersionCorrecters { get; } = new()
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
                if (oldJson.TryGetValue("randomStates", out object? rsRename))
                {
                    oldJson["random_states"] = rsRename;
                }
            }, "2.2"),
        };

        public Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                [Constants.JsonKeys.SaveData.SAVE_VERSION] = Constants.SAVE_VERSION,
                [Constants.JsonKeys.SaveData.SAVE_NAME] = saveName,
                [Constants.JsonKeys.SaveData.DISPLAY_NAME] = displaySaveName,
                [Constants.JsonKeys.SaveData.LAST_SAVE] = LastSave,
                [Constants.JsonKeys.SaveData.PLAYTIME] = GetPlaytime(),
                [Constants.JsonKeys.SaveData.PLAYER] = player.ToJson(),
                [Constants.JsonKeys.SaveData.RANDOM_STATES] = RandomStates.Instance.ToJson()
            };
        }

        static bool IJsonConvertable<SaveData>.FromJsonWithoutCorrection(IDictionary<string, object?> saveDataJson, string fileVersion, [NotNullWhen(true)] ref SaveData? saveData)
        {
            var success = true;

            success &= PACTools.TryParseJsonValue<SaveData, string?>(saveDataJson, Constants.JsonKeys.SaveData.SAVE_NAME, out var saveName);
            success &= PACTools.TryParseJsonValue<SaveData, string?>(saveDataJson, Constants.JsonKeys.SaveData.DISPLAY_NAME, out var displayName);
            success &= PACTools.TryParseJsonValue<SaveData, DateTime?>(saveDataJson, Constants.JsonKeys.SaveData.LAST_SAVE, out var lastSave);
            success &= PACTools.TryParseJsonValue<SaveData, TimeSpan?>(saveDataJson, Constants.JsonKeys.SaveData.PLAYTIME, out var playtime);
            success &= PACTools.TryParseJsonConvertableValue<SaveData, Player>(saveDataJson, fileVersion, Constants.JsonKeys.SaveData.PLAYER, out var player);
            success &= PACTools.TryParseJsonConvertableValue<SaveData, RandomStates>(saveDataJson, fileVersion, Constants.JsonKeys.SaveData.RANDOM_STATES, out _);

            saveData = Initialize(saveName ?? Constants.DEFAULT_SAVE_DATA_SAVE_NAME, displayName, lastSave, playtime, player);
            return success;
        }
        #endregion
    }
}
