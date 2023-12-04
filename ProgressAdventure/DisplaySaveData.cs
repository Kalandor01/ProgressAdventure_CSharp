using PACommon.JsonUtils;
using System.Diagnostics.CodeAnalysis;

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

        static bool IJsonConvertable<DisplaySaveData>.FromJsonWithoutCorrection(IDictionary<string, object?> objectJson, string fileVersion, [NotNullWhen(true)] ref DisplaySaveData? convertedObject)
        {
            var success = true;

            success &= Tools.TryParseJsonValue<DisplaySaveData, string?>(objectJson, Constants.JsonKeys.SaveData.SAVE_VERSION, out var saveVersion);
            success &= Tools.TryParseJsonValue<DisplaySaveData, string?>(objectJson, Constants.JsonKeys.SaveData.DISPLAY_NAME, out var displayName);
            success &= Tools.TryParseJsonValue<DisplaySaveData, DateTime?>(objectJson, Constants.JsonKeys.SaveData.LAST_SAVE, out var lastSave);
            success &= Tools.TryParseJsonValue<DisplaySaveData, TimeSpan?>(objectJson, Constants.JsonKeys.SaveData.PLAYTIME, out var playtime);
            success &= Tools.TryParseJsonValue<DisplaySaveData, string?>(objectJson, Constants.JsonKeys.DisplaySaveData.PLAYER_NAME, out var playerName);

            convertedObject = new DisplaySaveData(saveVersion, displayName, lastSave, playtime, playerName);
            return success;
        }
        #endregion
    }
}
