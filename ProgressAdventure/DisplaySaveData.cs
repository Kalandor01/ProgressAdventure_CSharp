using PACommon.JsonUtils;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;

namespace ProgressAdventure
{
    /// <summary>
    /// The display data of a save file.
    /// </summary>
    public class DisplaySaveData : IJsonConvertable<DisplaySaveData>
    {
        #region Public fields
        public readonly string? saveVersion;
        public readonly string? displaySaveName;
        public readonly DateTime? lastSave;
        public readonly TimeSpan? playtime;
        public readonly string? playerName;
        #endregion

        #region Public constructors
        private DisplaySaveData(string? saveVersion, string? displaySaveName, DateTime? lastSave, TimeSpan? playtime, string? playerName)
        {
            this.saveVersion = saveVersion;
            this.displaySaveName = displaySaveName;
            this.lastSave = lastSave;
            this.playtime = playtime;
            this.playerName = playerName;
        }
        #endregion

        #region JsonConvert
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<DisplaySaveData>.VersionCorrecters { get; } =
        [
            // 2.1.1 -> 2.2
            (oldJson => {
                // snake case rename
                if (oldJson.TryGetValue("displayName", out var dnRename))
                {
                    oldJson["display_name"] = dnRename;
                }
                if (oldJson.TryGetValue("playerName", out var pnRename))
                {
                    oldJson["player_name"] = pnRename;
                }
                if (oldJson.TryGetValue("lastSave", out var lsRename))
                {
                    oldJson["last_save"] = lsRename;
                }
            }, "2.2"),
        ];

        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.SaveData.SAVE_VERSION] = PACTools.ParseToJsonValue(Constants.SAVE_VERSION),
                [Constants.JsonKeys.SaveData.DISPLAY_NAME] = PACTools.ParseToJsonValue(displaySaveName),
                [Constants.JsonKeys.SaveData.LAST_SAVE] = PACTools.ParseToJsonValue(lastSave),
                [Constants.JsonKeys.SaveData.PLAYTIME] = PACTools.ParseToJsonValue(playtime),
                [Constants.JsonKeys.DisplaySaveData.PLAYER_NAME] = PACTools.ParseToJsonValue(playerName),
            };
        }

        /// <summary>
        /// ToJson() using the SaveData singleton for data.
        /// </summary>
        public static JsonDictionary ToJsonFromSaveData(SaveData saveData)
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.SaveData.SAVE_VERSION] = PACTools.ParseToJsonValue(Constants.SAVE_VERSION),
                [Constants.JsonKeys.SaveData.DISPLAY_NAME] = PACTools.ParseToJsonValue(saveData.displaySaveName),
                [Constants.JsonKeys.SaveData.LAST_SAVE] = PACTools.ParseToJsonValue(saveData.LastSave),
                [Constants.JsonKeys.SaveData.PLAYTIME] = PACTools.ParseToJsonValue(saveData.GetPlaytime()),
                [Constants.JsonKeys.DisplaySaveData.PLAYER_NAME] = PACTools.ParseToJsonValue(saveData.player.name),
            };
        }

        static bool IJsonConvertable<DisplaySaveData>.FromJsonWithoutCorrection(JsonDictionary objectJson, string fileVersion, [NotNullWhen(true)] ref DisplaySaveData? convertedObject)
        {
            var success = true;

            success &= PACTools.TryParseJsonValue<DisplaySaveData, string?>(objectJson, Constants.JsonKeys.SaveData.SAVE_VERSION, out var saveVersion);
            success &= PACTools.TryParseJsonValue<DisplaySaveData, string?>(objectJson, Constants.JsonKeys.SaveData.DISPLAY_NAME, out var displayName);
            success &= PACTools.TryParseJsonValue<DisplaySaveData, DateTime?>(objectJson, Constants.JsonKeys.SaveData.LAST_SAVE, out var lastSave);
            success &= PACTools.TryParseJsonValue<DisplaySaveData, TimeSpan?>(objectJson, Constants.JsonKeys.SaveData.PLAYTIME, out var playtime);
            success &= PACTools.TryParseJsonValue<DisplaySaveData, string?>(objectJson, Constants.JsonKeys.DisplaySaveData.PLAYER_NAME, out var playerName);

            convertedObject = new DisplaySaveData(saveVersion, displayName, lastSave, playtime, playerName);
            return success;
        }
        #endregion
    }
}
