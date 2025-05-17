using PACommon;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement;
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

        private (long x, long y)? _refrencePlayerPos;
        private Entity _playerRef;
        /// <summary>
        /// A refrence to the player object.
        /// </summary>
        public Entity PlayerRef
        {
            get
            {
                ResolvePlayerRef();
                return _playerRef;
            }
            private set => _playerRef = value;
        }

        /// <summary>
        /// The list of the configs that were loaded the last time the save was loaded.
        /// </summary>
        public List<LoadedConfigData> LastLoadedConfigs { get; private set; }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="SaveData" path="//summary"/>
        /// </summary>
        /// <param name="saveName"><inheritdoc cref="saveName" path="//summary"/></param>
        /// <param name="displaySaveName"><inheritdoc cref="displaySaveName" path="//summary"/></param>
        /// <param name="lastSave"><inheritdoc cref="LastSave" path="//summary"/></param>
        /// <param name="playtime"><inheritdoc cref="Playtime" path="//summary"/></param>
        /// <param name="player"><inheritdoc cref="PlayerRef" path="//summary"/></param>
        /// <param name="initialiseRandomGenerators">Whether to initialize the RandomStates object as well.</param>
        private SaveData(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastSave = null,
            TimeSpan? playtime = null,
            Entity? player = null,
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

            PlayerRef = player is null || player.type != EntityType.PLAYER ? new Entity(EntityType.PLAYER) : player;
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
        /// <param name="player"><inheritdoc cref="PlayerRef" path="//summary"/></param>
        /// <param name="initialiseRandomGenerators">Whether to initialize the RandomStates object as well.</param>
        public static SaveData Initialize(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastSave = null,
            TimeSpan? playtime = null,
            Entity? player = null,
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

        #region Private methods
        /// <summary>
        /// Resolves the player refrence, and makes it just an actual refrence.<br/>
        /// This should be called after initializing the <see cref="World"/>.
        /// </summary>
        private void ResolvePlayerRef()
        {
            if (_playerRef.Position is not null && _refrencePlayerPos is null)
            {
                return;
            }

            var playerPos = _playerRef.Position ?? _refrencePlayerPos ?? (0, 0);
            if (_refrencePlayerPos is null)
            {
                _playerRef.AddPosition(playerPos);
                _refrencePlayerPos = null;
                return;
            }

            World.TryGetTileAll(playerPos, out var playerTile);
            var playerTilePopMan = playerTile.populationManager;
            if (_playerRef.Position is not null)
            {
                playerTilePopMan.AddEntity(_playerRef);
                _refrencePlayerPos = null;
                return;
            }

            var playerCount = playerTilePopMan.GetEntityCount(EntityType.PLAYER, out var ulPlayers);
            if (playerCount <= 0)
            {
                _playerRef.AddPosition(playerTilePopMan);
                _refrencePlayerPos = null;
                return;
            }

            var loadedPlayers = playerTilePopMan.GetPlayers();
            if (loadedPlayers.Count == 0)
            {
                if (ulPlayers > 0)
                {
                    _playerRef = playerTilePopMan.LoadEntities(EntityType.PLAYER, 1).First();
                    _refrencePlayerPos = null;
                    return;
                }

                _playerRef.AddPosition(playerTilePopMan);
                _refrencePlayerPos = null;
                return;
            }

            if (loadedPlayers.Count == 1)
            {
                _playerRef = loadedPlayers.First();
                _refrencePlayerPos = null;
                return;
            }

            var matchingPlayer = loadedPlayers.FirstOrDefault(p =>
                p == _playerRef ||
                (
                    p.FullName == _playerRef.FullName &&
                    p.currentTeam == _playerRef.currentTeam &&
                    p.originalTeam == _playerRef.originalTeam &&
                    p.MaxHp == _playerRef.MaxHp &&
                    p.CurrentHp == _playerRef.CurrentHp
                )
            );

            if (matchingPlayer is not null)
            {
                _playerRef = matchingPlayer;
                _refrencePlayerPos = null;
                return;
            }

            _playerRef.AddPosition(playerPos);
            _refrencePlayerPos = null;
        }
        #endregion

        #region JsonConvert
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<SaveData>.VersionCorrecters { get; } =
        [
            // 2.0.1 -> 2.0.2
            (oldJson => {
                // saved entity types
                if (PACTools.TryCastJsonAnyValue<JsonDictionary>(oldJson, "player", out var playerDict, isStraigthCast: true))
                {
                    playerDict["type"] = "player";
                }
            }, "2.0.2"),
            // 2.1.1 -> 2.2
            (oldJson => {
                // snake case rename
                JsonDataCorrecterUtils.RemapKeysIfExist(oldJson, new Dictionary<string, string>
                {
                    ["displayName"] = "display_name",
                    ["lastSave"] = "last_save",
                    ["randomStates"] = "random_states",
                });
            }, "2.2"),
            // 2.5 -> 2.5.1?
            (oldJson => {
                // player -> player_ref, Entity pos stored in PopulationManager
                JsonDataCorrecterUtils.RenameKeyIfExists(oldJson, "player", "player_ref");

                if (
                    !oldJson.TryGetValue("player_ref", out var playerJsonOjs) ||
                    playerJsonOjs is not JsonDictionary playerJson
                )
                {
                    return;
                }

                if (
                    playerJson.TryGetValue("x_position", out var xPos) ||
                    playerJson.TryGetValue("xPos", out xPos)
                )
                {
                    oldJson["player_pos_x"] = xPos;
                }
                if (
                    playerJson.TryGetValue("y_position", out var yPos) ||
                    playerJson.TryGetValue("yPos", out yPos)
                )
                {
                    oldJson["player_pos_y"] = yPos;
                }
            }, "2.5.1"),
        ];

        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.SaveData.SAVE_VERSION] = Constants.SAVE_VERSION,
                [Constants.JsonKeys.SaveData.SAVE_NAME] = saveName,
                [Constants.JsonKeys.SaveData.DISPLAY_NAME] = displaySaveName,
                [Constants.JsonKeys.SaveData.LAST_SAVE] = LastSave,
                [Constants.JsonKeys.SaveData.PLAYTIME] = GetPlaytime(),
                [Constants.JsonKeys.SaveData.PLAYER_POS_X] = PlayerRef.Position?.x,
                [Constants.JsonKeys.SaveData.PLAYER_POS_Y] = PlayerRef.Position?.y,
                [Constants.JsonKeys.SaveData.PLAYER_REF] = PlayerRef.ToJson(),
                [Constants.JsonKeys.SaveData.RANDOM_STATES] = RandomStates.Instance.ToJson(),
            };
        }

        static bool IJsonConvertable<SaveData>.FromJsonWithoutCorrection(
            JsonDictionary saveDataJson,
            string fileVersion,
            [NotNullWhen(true)] ref SaveData? saveData
        )
        {
            var success = true;

            success &= PACTools.TryParseJsonValue<string?>(saveDataJson, Constants.JsonKeys.SaveData.SAVE_NAME, out var saveName);
            success &= PACTools.TryParseJsonValue<string?>(saveDataJson, Constants.JsonKeys.SaveData.DISPLAY_NAME, out var displayName);
            success &= PACTools.TryParseJsonValue<DateTime?>(saveDataJson, Constants.JsonKeys.SaveData.LAST_SAVE, out var lastSave);
            success &= PACTools.TryParseJsonValue<TimeSpan?>(saveDataJson, Constants.JsonKeys.SaveData.PLAYTIME, out var playtime);
            success &= PACTools.TryParseJsonValueNullable<long?>(saveDataJson, Constants.JsonKeys.SaveData.PLAYER_POS_X, out var playerPosX);
            success &= PACTools.TryParseJsonValueNullable<long?>(saveDataJson, Constants.JsonKeys.SaveData.PLAYER_POS_Y, out var playerPosY);
            (long, long)? playerPos = playerPosX is not null && playerPosY is not null ? ((long)playerPosX, (long)playerPosY) : null;
            success &= PACTools.TryParseJsonConvertableValue<Entity, (long, long)?>(saveDataJson, null, fileVersion, Constants.JsonKeys.SaveData.PLAYER_REF, out var player);
            success &= PACTools.TryParseJsonConvertableValue<RandomStates>(saveDataJson, fileVersion, Constants.JsonKeys.SaveData.RANDOM_STATES, out _);

            saveData = Initialize(saveName ?? Constants.DEFAULT_SAVE_DATA_SAVE_NAME, displayName, lastSave, playtime, player, false);
            saveData._refrencePlayerPos = playerPos ?? (0, 0);
            return success;
        }
        #endregion
    }
}
