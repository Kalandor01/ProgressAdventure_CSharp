using PACommon.JsonUtils;

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
        static List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<DisplaySaveData>.VersionCorrecters { get; } = new()
        {
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
        };

        public Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                [Constants.JsonKeys.SaveData.SAVE_VERSION] = Constants.SAVE_VERSION,
                [Constants.JsonKeys.SaveData.DISPLAY_NAME] = displaySaveName,
                [Constants.JsonKeys.SaveData.LAST_SAVE] = lastSave,
                [Constants.JsonKeys.SaveData.PLAYTIME] = playtime,
                [Constants.JsonKeys.DisplaySaveData.PLAYER_NAME] = playerName,
            };
        }

        /// <summary>
        /// ToJson() using the SaveData singleton for data.
        /// </summary>
        public static Dictionary<string, object?> ToJsonFromSaveData(SaveData saveData)
        {
            return new Dictionary<string, object?>
            {
                [Constants.JsonKeys.SaveData.SAVE_VERSION] = Constants.SAVE_VERSION,
                [Constants.JsonKeys.SaveData.DISPLAY_NAME] = saveData.displaySaveName,
                [Constants.JsonKeys.SaveData.LAST_SAVE] = saveData.LastSave,
                [Constants.JsonKeys.SaveData.PLAYTIME] = saveData.GetPlaytime(),
                [Constants.JsonKeys.DisplaySaveData.PLAYER_NAME] = saveData.player.name,
            };
        }

        static bool IJsonConvertable<DisplaySaveData>.FromJsonWithoutCorrection(IDictionary<string, object?> objectJson, string fileVersion, ref DisplaySaveData? convertedObject)
        {
            var success = true;

            var saveVersion = objectJson[Constants.JsonKeys.SaveData.SAVE_VERSION] as string;
            success &= saveVersion is not null;

            var displayName = objectJson[Constants.JsonKeys.SaveData.DISPLAY_NAME] as string;
            success &= displayName is not null;

            DateTime? lastSave = DateTime.TryParse(objectJson[Constants.JsonKeys.SaveData.LAST_SAVE]?.ToString(), out DateTime lastSaveParsed) ? lastSaveParsed : null;
            success &= lastSave is not null;

            TimeSpan? playtime = TimeSpan.TryParse(objectJson[Constants.JsonKeys.SaveData.PLAYTIME]?.ToString(), out TimeSpan playtimeParsed) ? playtimeParsed : null;
            success &= playtime is not null;

            var playerName = objectJson[Constants.JsonKeys.DisplaySaveData.PLAYER_NAME] as string;
            success &= playerName is not null;

            convertedObject = new DisplaySaveData(saveVersion, displayName, lastSave, playtime, playerName);
            return success;
        }
        #endregion
    }
}
