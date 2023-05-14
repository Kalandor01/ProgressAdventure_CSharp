using ProgressAdventure.Enums;
using SaveFileManager;
using PAConstants = ProgressAdventure.Constants;
using PATools = ProgressAdventure.Tools;
using Logger = ProgressAdventure.Logger;
using ProgressAdventure.ItemManagement;

namespace PAExtras
{
    /// <summary>
    /// Class containing tools for improting project adventure python saves to the C# version.
    /// </summary>
    public static class SaveImporter
    {
        #region Configs
        /// <summary>
        /// Dictionary to mat item type names to item ID-s.
        /// </summary>
        private static readonly Dictionary<string, ItemTypeID> itemTypeMap = new()
        {
            ["WOODEN_SWORD"] = ItemType.Weapon.WOODEN_SWORD,
            ["STONE_SWORD"] = ItemType.Weapon.STONE_SWORD,
            ["STEEL_SWORD"] = ItemType.Weapon.STEEL_SWORD,
            ["WOODEN_BOW"] = ItemType.Weapon.WOODEN_BOW,
            ["STEEL_ARROW"] = ItemType.Weapon.STEEL_ARROW,
            ["WOODEN_CLUB"] = ItemType.Weapon.WOODEN_CLUB,
            ["CLUB_WITH_TEETH"] = ItemType.Weapon.CLUB_WITH_TEETH,
            ["WOODEN_SHIELD"] = ItemType.Defence.WOODEN_SHIELD,
            ["LEATHER_CAP"] = ItemType.Defence.LEATHER_CAP,
            ["LEATHER_TUNIC"] = ItemType.Defence.LEATHER_TUNIC,
            ["LEATHER_PANTS"] = ItemType.Defence.LEATHER_PANTS,
            ["LEATHER_BOOTS"] = ItemType.Defence.LEATHER_BOOTS,
            ["BOOTLE"] = ItemType.Material.BOOTLE,
            ["WOOL"] = ItemType.Material.WOOL,
            ["CLOTH"] = ItemType.Material.CLOTH,
            ["WOOD"] = ItemType.Material.WOOD,
            ["STONE"] = ItemType.Material.STONE,
            ["STEEL"] = ItemType.Material.STEEL,
            ["GOLD"] = ItemType.Material.GOLD,
            ["TEETH"] = ItemType.Material.TEETH,
            ["HEALTH_POTION"] = ItemType.Misc.HEALTH_POTION,
            ["GOLD_COIN"] = ItemType.Misc.GOLD_COIN,
            ["SILVER_COIN"] = ItemType.Misc.SILVER_COIN,
            ["COPPER_COIN"] = ItemType.Misc.COPPER_COIN,
            ["ROTTEN_FLESH"] = ItemType.Misc.ROTTEN_FLESH,
        };
        #endregion

        #region Public functions
        public static void ImportSave(string saveFolderName)
        {
            // get folder
            var exprotedSaveFolderPath = Path.Join(Constants.EXPORTED_FOLDER_PATH, saveFolderName);
            var correctedSaveFolderPath = Path.Join(PAConstants.SAVES_FOLDER_PATH, saveFolderName);

            // get data file
            var dataFilePath = Path.Join(exprotedSaveFolderPath, Constants.SAVE_FILE_NAME_DATA);
            var dataFileData = ReadJsonLine(dataFilePath, 1);

            PATools.RecreateChunksFolder(saveFolderName);
            var correctDataFileLines = CorrectDataFileData(dataFileData);
            PATools.EncodeSaveShort(correctDataFileLines, correctedSaveFolderPath);
        }
        #endregion

        #region Private functions
        public static Dictionary<string, object?>? ReadJsonLine(string filePath, int lineNum = 0)
        {
            // get folder
            var fullFilePath = filePath + "." + Constants.EXPORTED_SAVE_EXT;
            var safeFilePath = Path.GetRelativePath(PAConstants.ROOT_FOLDER, fullFilePath);

            // get data file
            string[] dataFileLines;
            try
            {
                dataFileLines = File.ReadAllLines(fullFilePath);
            }
            catch (Exception e)
            {
                Logger.Log("File read error", e.ToString(), LogSeverity.ERROR);
                throw;
            }

            // display data
            try
            {
                return PATools.DeserializeJson(dataFileLines[lineNum]);
            }
            catch (Exception)
            {
                Logger.Log("Json decode error", $"file name: {safeFilePath}", LogSeverity.ERROR);
                throw;
            }
        }

        private static List<Dictionary<string, object>> CorrectDataFileData(Dictionary<string, object?>? dataFileData)
        {
            if (dataFileData is null || dataFileData["save_version"]?.ToString() != Constants.NEWEST_PYTHON_SAVE_VERSION)
            {
                throw new Exception("Wrong save format");
            }

            // display data
            var displayLine = new Dictionary<string, object>();

            displayLine["saveVersion"] = Constants.IMPORT_SAVE_VERSION;
            var displayName = (string)dataFileData["display_name"];
            displayLine["displayName"] = displayName;
            var lastSaveRaw = (IEnumerable<object>)dataFileData["last_access"];
            var lastSaved = new DateTime(
                (int)(long)lastSaveRaw.ElementAt(0),
                (int)(long)lastSaveRaw.ElementAt(1),
                (int)(long)lastSaveRaw.ElementAt(2),
                (int)(long)lastSaveRaw.ElementAt(3),
                (int)(long)lastSaveRaw.ElementAt(4),
                (int)(long)lastSaveRaw.ElementAt(5)
            );
            displayLine["lastSaved"] = lastSaved;

            // normal data
            var dataLine = displayLine.DeepCopy();


            var playerData = (IDictionary<string, object>)dataFileData["player"];

            var inventoryJson = new List<Dictionary<string, object>>();
            foreach (var item in (IEnumerable<object>)playerData["inventory"])
            {
                var itemJson = item as IDictionary<string, object>;
                inventoryJson.Add(new Dictionary<string, object>
                {
                    ["type"] = itemTypeMap[(string)itemJson["type"]],
                    ["amount"] = (int)(long)itemJson["amount"],
                });
            }

            dataLine["player"] = new Dictionary<string, object>
            {
                ["name"] = (string)playerData["name"],
                ["baseHp"] = (int)(long)playerData["base_hp"],
                ["baseAttack"] = (int)(long)playerData["base_attack"],
                ["baseDefence"] = (int)(long)playerData["base_defence"],
                ["baseSpeed"] = (int)(long)playerData["base_speed"],
                ["currentTeam"] = (int)(long)playerData["team"],
                ["originalTeam"] = (bool)playerData["switched"] ? 1 : (int)(long)playerData["team"],
                ["attributes"] = (IEnumerable<object>)playerData["attributes"],
                ["drops"] = (IEnumerable<object>)playerData["drops"],
                ["xPos"] = (long)playerData["x_pos"],
                ["yPos"] = (long)playerData["y_pos"],
                ["facing"] = (int)(long)playerData["rotation"],
                ["inventory"] = inventoryJson,
            };

            return new List<Dictionary<string, object>> { displayLine, dataLine };
        }
        #endregion
    }
}
