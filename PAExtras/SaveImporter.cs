﻿using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.ItemManagement;
using System.Collections;
using System.Text;
using PACConstants = PACommon.Constants;
using PAConstants = ProgressAdventure.Constants;
using PACTools = PACommon.Tools;
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
            ["BOTTLE"] = ItemType.Material.BOOTLE,
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
        /// <param name="showProggress">Whether to show the conversion proggress text.</param>
        public static void ImportSave(string saveFolderName, bool showProggress = true)
        {
            // get folder
            var exprotedSaveFolderPath = Path.Join(Constants.EXPORTED_FOLDER_PATH, saveFolderName);
            if (!Directory.Exists(exprotedSaveFolderPath))
            {
                PACSingletons.Instance.Logger.Log("Correcting save file", $"save folder doesn't exist: {saveFolderName}", LogSeverity.ERROR);
                Console.WriteLine($"Save folder \"{exprotedSaveFolderPath}\" doesn't exist");
                return;
            }

            PACSingletons.Instance.Logger.Log("Correcting save file", $"save folder name: {saveFolderName}");
            Console.WriteLine($"Correcting save file ({saveFolderName})");
            // get destination folder
            var correctedSaveFolderPath = Path.Join(PAConstants.SAVES_FOLDER_PATH, saveFolderName);

            PATools.RecreateChunksFolder(saveFolderName);

            // data file
            var (chunkSeedMod, tileNoiseSeeds) = CorrectDataFile(exprotedSaveFolderPath, correctedSaveFolderPath);
            // chunks
            CorrectChunkFiles(exprotedSaveFolderPath, correctedSaveFolderPath, chunkSeedMod, tileNoiseSeeds, showProggress);
            PACSingletons.Instance.Logger.Log("Corrected save file", $"save folder name: {saveFolderName}");
            Console.WriteLine($"Corrected save file ({saveFolderName})");
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Reads 1 line from a plain json file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="lineNum">The line number to read.</param>
        private static JsonDictionary? ReadJsonLine(string filePath, int lineNum = 0)
        {
            // get folder
            var fullFilePath = filePath + "." + Constants.EXPORTED_SAVE_EXT;
            var safeFilePath = Path.GetRelativePath(PACConstants.ROOT_FOLDER, fullFilePath);

            // get data file
            string[] dataFileLines;
            try
            {
                dataFileLines = File.ReadAllLines(fullFilePath);
            }
            catch (Exception e)
            {
                PACSingletons.Instance.Logger.Log("File read error", e.ToString(), LogSeverity.ERROR);
                throw;
            }

            // display data
            try
            {
                return JsonSerializer.DeserializeJson(dataFileLines[lineNum]);
            }
            catch (Exception)
            {
                PACSingletons.Instance.Logger.Log("Json decode error", $"file name: {safeFilePath}", LogSeverity.ERROR);
                throw;
            }
        }

        /// <summary>
        /// Corrects the data file.
        /// </summary>
        /// <param name="exprotedSaveFolderPath">The path to the exported save folder.</param>
        /// <param name="correctedSaveFolderPath">The path to the save folder for the corrected data.</param>
        private static (double chunkSeedMod, JsonDictionary tileNoiseSeeds) CorrectDataFile(string exprotedSaveFolderPath, string correctedSaveFolderPath)
        {
            var dataFilePath = Path.Join(exprotedSaveFolderPath, Constants.SAVE_FILE_NAME_DATA);
            var dataFileData = ReadJsonLine(dataFilePath, 1);

            var (correctDataFileLines, chunkSeedMod, tileNoiseSeeds) = CorrectDataFileData(dataFileData);
            var correctDataFilePath = Path.Join(correctedSaveFolderPath, PAConstants.SAVE_FILE_NAME_DATA);
            PACTools.EncodeFileShort(correctDataFileLines, correctDataFilePath, PAConstants.OLD_SAVE_SEED, PAConstants.OLD_SAVE_EXT);
            return (chunkSeedMod, tileNoiseSeeds);
        }

        /// <summary>
        /// Converts the data file part of the exported save into the correct format.
        /// </summary>
        /// <param name="dataFileData">The json data from the exported save.</param>
        /// <exception cref="Exception"></exception>
        private static (List<JsonDictionary> dataFileLines, double chunkSeedMod, JsonDictionary tileNoiseSeeds) CorrectDataFileData(JsonDictionary? dataFileData)
        {
            if (dataFileData is null || dataFileData["save_version"]?.ToString() != Constants.NEWEST_PYTHON_SAVE_VERSION)
            {
                throw new Exception("Wrong save format");
            }

            // display data
            var displayLine = new JsonDictionary
            {
                ["saveVersion"] = PACTools.ParseToJsonValue(Constants.IMPORT_SAVE_VERSION),
            };
            var displayName = (string)dataFileData["display_name"].Value;
            displayLine["displayName"] = PACTools.ParseToJsonValue(displayName);
            var lastSaveRaw = (IEnumerable<JsonObject>)dataFileData["last_access"];
            var lastSaved = new DateTime(
                (int)(long)lastSaveRaw.ElementAt(0).Value,
                (int)(long)lastSaveRaw.ElementAt(1).Value,
                (int)(long)lastSaveRaw.ElementAt(2).Value,
                (int)(long)lastSaveRaw.ElementAt(3).Value,
                (int)(long)lastSaveRaw.ElementAt(4).Value,
                (int)(long)lastSaveRaw.ElementAt(5).Value
            );
            displayLine["lastSave"] = PACTools.ParseToJsonValue(lastSaved);
            displayLine["playtime"] = PACTools.ParseToJsonValue(TimeSpan.Zero);

            // normal data
            var dataLine = displayLine.DeepCopy();

            //player data
            var playerData = (JsonDictionary)dataFileData["player"];

            // player name
            var playerName = playerData["name"];
            displayLine["playerName"] = playerName;

            var inventoryJson = new JsonArray();
            foreach (var item in (JsonArray)playerData["inventory"])
            {
                var itemJson = item as JsonDictionary;
                inventoryJson.Add(new JsonDictionary
                {
                    ["type"] = PACTools.ParseToJsonValue(itemTypeMap[(string)itemJson["type"].Value].mID),
                    ["amount"] = itemJson["amount"],
                });
            }

            dataLine["player"] = new JsonDictionary
            {
                ["name"] = playerName.Value.ToString(),
                ["baseMaxHp"] = playerData["base_hp"],
                ["currentHp"] = playerData["base_hp"],
                ["baseAttack"] = playerData["base_attack"],
                ["baseDefence"] = playerData["base_defence"],
                ["baseSpeed"] = playerData["base_speed"],
                ["currentTeam"] = playerData["team"],
                ["originalTeam"] = (bool)playerData["switched"].Value ? 1 : playerData["team"],
                ["attributes"] = playerData["attributes"],
                ["drops"] = playerData["drops"],
                ["xPos"] = playerData["x_pos"],
                ["yPos"] = playerData["y_pos"],
                ["facing"] = playerData["rotation"],
                ["inventory"] = inventoryJson,
            };

            // random states
            var seedData = (JsonDictionary)dataFileData["seeds"];
            var mainRandom = ParseRandomFromExprotedRandom((JsonDictionary)seedData["main_seed"]);
            var worldRandom = ParseRandomFromExprotedRandom((JsonDictionary)seedData["world_seed"]);
            var miscRandom = mainRandom.Split();
            var ttnsData = (JsonDictionary)seedData["tile_type_noise_seeds"];
            var ttnSeeds = new JsonDictionary
            {
                ["HEIGHT"] = ttnsData["height"],
                ["TEMPERATURE"] = ttnsData["temperature"],
                ["HUMIDITY"] = ttnsData["humidity"],
                ["HOSTILITY"] = ttnsData["hostility"],
                ["POPULATION"] = ttnsData["population"],
            };
            var chunkSeedModifier = worldRandom.GenerateDouble();

            dataLine["randomStates"] = new JsonDictionary
            {
                ["mainRandom"] = PACTools.ParseToJsonValue(PACTools.SerializeRandom(mainRandom)),
                ["worldRandom"] = PACTools.ParseToJsonValue(PACTools.SerializeRandom(worldRandom)),
                ["miscRandom"] = PACTools.ParseToJsonValue(PACTools.SerializeRandom(miscRandom)),
                ["tileTypeNoiseSeeds"] = ttnSeeds,
                ["chunkSeedModifier"] = PACTools.ParseToJsonValue(chunkSeedModifier),
            };

            return (new List<JsonDictionary> { displayLine, dataLine }, chunkSeedModifier, ttnSeeds);
        }

        /// <summary>
        /// Creates a new random from any string.
        /// </summary>
        /// <param name="rawString">The raw string.</param>
        private static SplittableRandom? ParseRandomFromString(string rawString)
        {
            var randomState = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawString));
            return PACTools.DeserializeRandom(randomState);
        }

        /// <summary>
        /// Creates a new random from the json representation of an exported random.
        /// </summary>
        /// <param name="exportedRandom">The json representation of an exported random.</param>
        private static SplittableRandom ParseRandomFromExprotedRandom(JsonDictionary exportedRandom)
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
        /// <param name="chunkSeedMod">The chunk seed modifier.</param>
        /// <param name="tileNoiseSeeds">The tile noise seeds.</param>
        /// <param name="showProggress">Whether to show the conversion proggress text.</param>
        private static void CorrectChunkFiles(
            string exprotedSaveFolderPath,
            string correctedSaveFolderPath,
            double chunkSeedMod,
            JsonDictionary tileNoiseSeeds,
            bool showProggress = true
        )
        {
            PACSingletons.Instance.Logger.Log("Correcting chunks...");
            var correctText = "Correcting chunk files...";
            if (showProggress)
            {
                Console.Write(correctText);
            }
            var chunksFolderPath = Path.Join(exprotedSaveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS);
            var correctedChunksFolderPath = Path.Join(correctedSaveFolderPath, Constants.SAVE_FOLDER_NAME_CHUNKS);

            var tileNoiseGenerators = CalculateNoiseGenerators(tileNoiseSeeds);
            var chunkPaths = Directory.GetFiles(chunksFolderPath);
            for (var x = 0; x < chunkPaths.Length; x++)
            {
                var chunkFileName = Path.GetFileNameWithoutExtension(chunkPaths[x]);
                var correctChunkFilePath = Path.Join(correctedChunksFolderPath, chunkFileName);

                var chunkFileData = ReadJsonLine(Path.Join(chunksFolderPath, chunkFileName), 0);
                var chunkCoordinatesSplit = chunkFileName.Split(Constants.CHUNK_FILE_NAME_SEP);
                var chunkX = long.Parse(chunkCoordinatesSplit[1]);
                var chunkY = long.Parse(chunkCoordinatesSplit[2]);
                var chunkPosition = (chunkX, chunkY);

                var correctChunkFileLines = CorrectChunkFileData(chunkPosition, chunkSeedMod, tileNoiseGenerators, chunkFileData);
                PACTools.EncodeFileShort(correctChunkFileLines, correctChunkFilePath, PAConstants.OLD_SAVE_SEED, PAConstants.OLD_SAVE_EXT);
                if (showProggress)
                {
                    Console.Write($"\r{correctText}{Math.Round((x + 1) * 1.0 / chunkPaths.Length * 100, 3)}%                ");
                }
            }
            if (showProggress)
            {
                Console.WriteLine($"\r{correctText}DONE!                        ");
            }
        }

        /// <summary>
        /// Generates the chunk random genrator for a chunk.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the chunk.</param>
        /// <param name="chunkSeedMod">The chunk seed modifier.</param>
        /// <param name="tileNoiseGenerators">The tile noise generators.</param>
        private static SplittableRandom GetChunkRandom((long x, long y) absolutePosition, double chunkSeedMod, Dictionary<string, PerlinNoise> tileNoiseGenerators)
        {
            var posX = Utils.FloorRound(absolutePosition.x, Constants.CHUNK_SIZE);
            var posY = Utils.FloorRound(absolutePosition.y, Constants.CHUNK_SIZE);
            var noiseValues = GetNoiseValues((posX, posY), tileNoiseGenerators);
            var noiseNum = noiseValues.Count;
            var seedNumSize = 19.0;
            var noiseTenMulti = (int)Math.Floor(seedNumSize / noiseNum);
            var noiseMulti = Math.Pow(10, noiseTenMulti);
            ulong seed = 1;
            for (int x = 0; x < noiseValues.Count; x++)
            {
                var noiseVal = noiseValues.ElementAt(x).Value;
                seed *= (ulong)(noiseVal * noiseMulti);
            }
            seed = (ulong)(seed * chunkSeedMod);
            return new SplittableRandom(seed);
        }

        /// <summary>
        /// Calculates the perlin noise generators.
        /// </summary>
        /// <param name="tileNoiseSeeds">The tile noise seeds.</param>
        private static Dictionary<string, PerlinNoise> CalculateNoiseGenerators(JsonDictionary tileNoiseSeeds)
        {
            return new Dictionary<string, PerlinNoise>
            {
                ["height"] = new PerlinNoise((ulong)(long)tileNoiseSeeds["HEIGHT"].Value),
                ["temperature"] = new PerlinNoise((ulong)(long)tileNoiseSeeds["TEMPERATURE"].Value),
                ["humidity"] = new PerlinNoise((ulong)(long)tileNoiseSeeds["HUMIDITY"].Value),
                ["hostility"] = new PerlinNoise((ulong)(long)tileNoiseSeeds["HOSTILITY"].Value),
                ["population"] = new PerlinNoise((ulong)(long)tileNoiseSeeds["POPULATION"].Value)
            };
        }

        /// <summary>
        /// Calculates the noise values for each perlin noise generator at a specific point, and normalises it between 0 and 1.
        /// </summary>
        /// <param name="absolutePosition">The absolute coordinate of the chunk.</param>
        /// <param name="tileNoiseGenerators">The tile noise generators.</param>
        private static Dictionary<string, double> GetNoiseValues((long x, long y) absolutePosition, Dictionary<string, PerlinNoise> tileNoiseGenerators)
        {
            var x = absolutePosition.x;
            var y = absolutePosition.y;
            var noiseValues = new Dictionary<string, double>();
            foreach (var noiseGeneratorEntry in tileNoiseGenerators)
            {
                var noiseKey = noiseGeneratorEntry.Key;
                var noiseGenerator = noiseGeneratorEntry.Value;
                var noiseValue = noiseGenerator.Generate(x, y, 16.0 / Constants.TILE_NOISE_DIVISION) * 1;
                noiseValue += noiseGenerator.Generate(x, y, 8.0 / Constants.TILE_NOISE_DIVISION) * 2;
                noiseValue += noiseGenerator.Generate(x, y, 4.0 / Constants.TILE_NOISE_DIVISION) * 4;
                noiseValue += noiseGenerator.Generate(x, y, 2.0 / Constants.TILE_NOISE_DIVISION) * 8;
                noiseValue += noiseGenerator.Generate(x, y, 1.0 / Constants.TILE_NOISE_DIVISION) * 16;
                noiseValue /= 31;
                noiseValues[noiseKey] = noiseValue;
            }
            return noiseValues;
        }

        /// <summary>
        /// Corrects a chunk file's data.
        /// </summary>
        /// <param name="basePosition">The base position of the chunk.</param>
        /// <param name="chunkSeedMod">The chunk seed modifier.</param>
        /// <param name="tileNoiseGenerators">The tile noise generators.</param>
        /// <param name="chunkFileData">The data from the chunk file.</param>
        private static JsonDictionary CorrectChunkFileData(
            (long x, long y) basePosition,
            double chunkSeedMod,
            Dictionary<string, PerlinNoise> tileNoiseGenerators,
            JsonDictionary chunkFileData
        )
        {
            var correctedTiles = new JsonArray();
            var tilesData = (JsonArray)chunkFileData["tiles"];
            foreach (var tileData in tilesData)
            {
                correctedTiles.Add(CorrectTileData((JsonDictionary)tileData));
            }

            return new JsonDictionary
            {
                ["saveVersion"] = PACTools.ParseToJsonValue("2.0"),
                ["chunkRandom"] = PACTools.ParseToJsonValue(PACTools.SerializeRandom(GetChunkRandom(basePosition, chunkSeedMod, tileNoiseGenerators))),
                ["tiles"] = correctedTiles,
            };
        }

        /// <summary>
        /// Corrects a tile's data.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        private static JsonDictionary CorrectTileData(JsonDictionary tileData)
        {
            return new JsonDictionary
            {
                ["xPos"] = tileData["x"],
                ["yPos"] = tileData["y"],
                ["visited"] = tileData["visited"],
                ["terrain"] = CorrectContentData((JsonDictionary)tileData["terrain"]),
                ["structure"] = CorrectContentData((JsonDictionary)tileData["structure"]),
                ["population"] = CorrectContentData((JsonDictionary)tileData["population"]),
            };
        }

        /// <summary>
        /// Corrects the content's data.
        /// </summary>
        /// <param name="contentData">The content data.</param>
        private static JsonDictionary CorrectContentData(JsonDictionary contentData)
        {
            var correctedContent = contentData.DeepCopy();
            correctedContent["type"] = correctedContent["type"];
            correctedContent["subtype"] = correctedContent["subtype"];
            if (correctedContent["subtype"]?.ToString() == "bandit_camp")
            {
                correctedContent["subtype"] = PACTools.ParseToJsonValue("banditCamp");
            }
            correctedContent["name"] = correctedContent["name"];
            if (correctedContent["name"]?.ToString() == "None")
            {
                correctedContent["name"] = null;
            }

            return correctedContent;
        }
        #endregion
    }
}
