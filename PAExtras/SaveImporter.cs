using NPrng.Generators;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using System.Collections;
using System.Text;
using Logger = ProgressAdventure.Logger;
using PAConstants = ProgressAdventure.Constants;
using PATools = ProgressAdventure.Tools;

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
        /// <summary>
        /// Converts the json representation of an exproted python save file into a C# version save file.
        /// </summary>
        /// <param name="saveFolderName">The name of the save folder to import.</param>
        public static void ImportSave(string saveFolderName)
        {
            // get folder
            var exprotedSaveFolderPath = Path.Join(Constants.EXPORTED_FOLDER_PATH, saveFolderName);
            var correctedSaveFolderPath = Path.Join(PAConstants.SAVES_FOLDER_PATH, saveFolderName);

            PATools.RecreateChunksFolder(saveFolderName);

            // data file
            CorrectDataFile(exprotedSaveFolderPath, correctedSaveFolderPath);
            // chunks
            CorrectChunkFiles(exprotedSaveFolderPath, correctedSaveFolderPath);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Reads 1 line from a plain json file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="lineNum">The line number to read.</param>
        private static Dictionary<string, object?>? ReadJsonLine(string filePath, int lineNum = 0)
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

        /// <summary>
        /// Corrects the data file.
        /// </summary>
        /// <param name="exprotedSaveFolderPath">The path to the exported save folder.</param>
        /// <param name="correctedSaveFolderPath">The path to the save folder for the corrected data.</param>
        private static void CorrectDataFile(string exprotedSaveFolderPath, string correctedSaveFolderPath)
        {
            var dataFilePath = Path.Join(exprotedSaveFolderPath, Constants.SAVE_FILE_NAME_DATA);
            var dataFileData = ReadJsonLine(dataFilePath, 1);

            var correctDataFileLines = CorrectDataFileData(dataFileData);
            var correctDataFilePath = Path.Join(correctedSaveFolderPath, PAConstants.SAVE_FILE_NAME_DATA);
            PATools.EncodeSaveShort(correctDataFileLines, correctDataFilePath);
        }

        /// <summary>
        /// Converts the data file part of the exported save into the correct format.
        /// </summary>
        /// <param name="dataFileData">The json data from the exported save.</param>
        /// <exception cref="Exception"></exception>
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
            displayLine["lastSave"] = lastSaved;
            displayLine["playtime"] = TimeSpan.Zero;

            // normal data
            var dataLine = displayLine.DeepCopy();

            //player data
            var playerData = (IDictionary<string, object>)dataFileData["player"];

            var inventoryJson = new List<Dictionary<string, object>>();
            foreach (var item in (IEnumerable<object>)playerData["inventory"])
            {
                var itemJson = item as IDictionary<string, object>;
                inventoryJson.Add(new Dictionary<string, object>
                {
                    ["type"] = itemTypeMap[(string)itemJson["type"]].GetHashCode(),
                    ["amount"] = (int)(long)itemJson["amount"],
                });
            }

            dataLine["player"] = new Dictionary<string, object>
            {
                ["name"] = (string)playerData["name"],
                ["baseMaxHp"] = (int)(long)playerData["base_hp"],
                ["currentHp"] = (int)(long)playerData["base_hp"],
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

            // random states
            var seedData = (IDictionary<string, object>)dataFileData["seeds"];
            var mainRandom = ParseRandomFromExprotedRandom((Dictionary<string, object>)seedData["main_seed"]);
            var worldRandom = ParseRandomFromExprotedRandom((Dictionary<string, object>)seedData["world_seed"]);
            var miscRandom = mainRandom.Split();
            var ttnsData = (Dictionary<string, object>)seedData["tile_type_noise_seeds"];
            var ttnSeeds = new Dictionary<string, ulong>
            {
                ["HEIGHT"] = (ulong)(long)ttnsData["height"],
                ["TEMPERATURE"] = (ulong)(long)ttnsData["temperature"],
                ["HUMIDITY"] = (ulong)(long)ttnsData["humidity"],
                ["HOSTILITY"] = (ulong)(long)ttnsData["hostility"],
                ["POPULATION"] = (ulong)(long)ttnsData["population"],
            };
            var chunkSeedModifier = worldRandom.GenerateDouble();

            dataLine["randomStates"] = new Dictionary<string, object>
            {
                ["mainRandom"] = PATools.SerializeRandom(mainRandom),
                ["worldRandom"] = PATools.SerializeRandom(worldRandom),
                ["miscRandom"] = PATools.SerializeRandom(miscRandom),
                ["tileTypeNoiseSeeds"] = ttnSeeds,
                ["chunkSeedModifier"] = chunkSeedModifier,
            };

            return new List<Dictionary<string, object>> { displayLine, dataLine };
        }

        /// <summary>
        /// Creates a new random from any string.
        /// </summary>
        /// <param name="rawString">The raw string.</param>
        private static SplittableRandom? ParseRandomFromString(string rawString)
        {
            var randomState = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawString));
            return PATools.DeserializeRandom(randomState);
        }

        /// <summary>
        /// Creates a new random from the json representation of an exported random.
        /// </summary>
        /// <param name="exportedRandom">The json representation of an exported random.</param>
        private static SplittableRandom ParseRandomFromExprotedRandom(Dictionary<string, object> exportedRandom)
        {
            var randomString = new StringBuilder();
            randomString.Append(exportedRandom["type"].ToString());
            randomString.Append(exportedRandom["pos"].ToString());
            randomString.Append(exportedRandom["has_gauss"].ToString());
            randomString.Append(exportedRandom["cached_gaussian"].ToString());
            var state = (IEnumerable)exportedRandom["state"];
            randomString.Append(state.GetHashCode());
            return ParseRandomFromString(randomString.ToString());
        }

        /// <summary>
        /// Corrects all chunk files.
        /// </summary>
        /// <param name="exprotedSaveFolderPath">The path to the exported save folder.</param>
        /// <param name="correctedSaveFolderPath">The path to the save folder for the corrected data.</param>
        private static void CorrectChunkFiles(string exprotedSaveFolderPath, string correctedSaveFolderPath)
        {
            var chunksFolderPath = Path.Join(exprotedSaveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS);
            var correctedChunksFolderPath = Path.Join(correctedSaveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS);

            var chunks = Directory.GetFiles(chunksFolderPath);
            foreach ( var chunkPath in chunks)
            {
                var chunkFileName = Path.GetFileNameWithoutExtension(chunkPath);
                var correctChunkFilePath = Path.Join(correctedChunksFolderPath, chunkFileName);

                var chunkFileData = ReadJsonLine(Path.Join(chunksFolderPath, chunkFileName), 0);
                var chunkPosition = (0, 0);

                var correctChunkFileLines = CorrectChunkFileData(chunkPosition, chunkFileData);
                PATools.EncodeSaveShort(correctChunkFileLines, correctChunkFilePath);
            }
        }

        private static Dictionary<string, object> CorrectChunkFileData((long x, long y) basePosition, Dictionary<string, object> chunkFileData)
        {
            var correctedChunkData = new Dictionary<string, object>();

            return correctedChunkData;
        }
        #endregion
    }
}
